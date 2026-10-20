using AzureTest.Contracts.DTOs.Common;
using AzureTest.Contracts.DTOs.Debts;
using AzureTest.Application.Abstractions;


using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using AzureTest.Application.Debts;
using AzureTest.Application.Debts.Commands;
using AzureTest.Application.Debts.Interfaces;
using AzureTest.Application.Debts.Queries;
using AzureTest.Application.Interfaces.Data.NoSql;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;

namespace AzureTest.MVC.Controllers;

[Authorize]
[DisplayName("Debt")]
[Description("Manage and control Debt operations.")]
    public class DebtController : Controller
{
    private readonly IDebtCommandUnit _commands;
    private readonly IDebtQueryUnit _queries;
    private readonly IDebtPersonRepository _debtPersonRepository;
    private readonly AzureTest.Application.Users.IUserQueryUnit _userQueries;
    private readonly IValidator<CreateDebtCommand> _createValidator;
    private readonly IValidator<UpdateDebtCommand> _updateValidator;
    private readonly IValidator<DeleteDebtCommand> _deleteValidator;

    public DebtController(
        IDebtCommandUnit commands,
        IDebtQueryUnit queries,
        IDebtPersonRepository debtPersonRepository,
        AzureTest.Application.Users.IUserQueryUnit userQueries,
        IValidator<CreateDebtCommand> createValidator,
        IValidator<UpdateDebtCommand> updateValidator,
        IValidator<DeleteDebtCommand> deleteValidator)
    {
        _commands = commands;
        _queries = queries;
        _debtPersonRepository = debtPersonRepository;
        _userQueries = userQueries;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _deleteValidator = deleteValidator;
    }

    private async Task LoadDebtPersonsViewBagAsync()
    {
        var persons = await _debtPersonRepository.GetAllAsync();
        ViewBag.DebtPersons = persons
            .OrderBy(p => p.FullName)
            .Select(p => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Value = p.Id.ToString(),
                Text = p.FullName
            })
            .ToList();
    }

    [HttpGet]
    [DisplayName("Index")]
    [Description("View a filtered, sorted and paged list of debts.")]
    public async Task<IActionResult> Index([FromQuery] GetAllDebtsFilters filters, [FromQuery] int? page, [FromQuery] string? search, [FromQuery] string? sort, [FromQuery] string? dir, CancellationToken cancellationToken = default)
    {
        filters ??= new GetAllDebtsFilters();
        if (page.HasValue && page.Value > 0) filters.PageNumber = page.Value;
        if (!string.IsNullOrWhiteSpace(search) && string.IsNullOrWhiteSpace(filters.SearchTerm)) filters.SearchTerm = search;
        if (!string.IsNullOrWhiteSpace(sort)) filters.SortBy = sort;
        if (!string.IsNullOrWhiteSpace(dir)) filters.SortDescending = string.Equals(dir, "desc", StringComparison.OrdinalIgnoreCase);

        var isAdmin = User.IsInRole("Admin");
        if (isAdmin)
        {
            var usersResult = await _userQueries.GetAllUsers.HandleAsync(new AzureTest.Application.Users.Queries.GetAllUsers.GetAllUsersQuery(new AzureTest.Contracts.DTOs.Users.GetAllUsersFilters { PageSize = 100 }), cancellationToken);
            ViewBag.Users = usersResult.Items;
        }

        await LoadDebtPersonsViewBagAsync();

        var pagedResult = await _queries.GetAll.HandleAsync(new GetAllDebtsQuery(filters), cancellationToken);

        ViewBag.Filters = filters;
        ViewBag.CurrentPage = pagedResult.PageNumber;
        ViewBag.TotalPages = pagedResult.TotalPages;
        ViewBag.CurrentSearch = filters.SearchTerm;
        ViewBag.CurrentSort = filters.SortBy;
        ViewBag.CurrentDir = filters.SortDescending ? "desc" : "asc";
        ViewBag.IsAdmin = isAdmin;

        return View(pagedResult);
    }

    [HttpGet]
    [DisplayName("Debt Analytics")]
    [Description("View comprehensive financial analytics and charts for debts grouped by person and status.")]
    public async Task<IActionResult> Analytics([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate, [FromQuery] List<Guid>? userIds, CancellationToken cancellationToken = default)
    {
        var isAdmin = User.IsInRole("Admin");
        if (isAdmin)
        {
            var usersResult = await _userQueries.GetAllUsers.HandleAsync(new AzureTest.Application.Users.Queries.GetAllUsers.GetAllUsersQuery(new AzureTest.Contracts.DTOs.Users.GetAllUsersFilters { PageSize = 100 }), cancellationToken);
            ViewBag.Users = usersResult.Items;
        }

        var query = new GetDebtAnalyticsQuery(UserIds: userIds, FromDate: fromDate, ToDate: toDate);
        var analytics = await _queries.GetAnalytics.HandleAsync(query, cancellationToken);

        ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
        ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
        ViewBag.SelectedUserIds = userIds ?? new List<Guid>();
        ViewBag.IsAdmin = isAdmin;

        return View(analytics);
    }

    [HttpGet]
    [DisplayName("Create")]
    [Description("Create a new Debt.")]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        await LoadDebtPersonsViewBagAsync();
        var cmd = new CreateDebtCommand(0, DateTime.Now.Date, null, null, 0, null, string.Empty, string.Empty, false);
        return View(cmd);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Create")]
    [Description("Create a new Debt.")]
    public async Task<IActionResult> Create(CreateDebtCommand dto, CancellationToken cancellationToken)
    {
        var validation = await _createValidator.ValidateAsync(dto, cancellationToken);
        if (!validation.IsValid)
        {
            foreach (var error in validation.Errors)
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            await LoadDebtPersonsViewBagAsync();
            return View(dto);
        }

        var id = await _commands.Create.HandleAsync(dto, cancellationToken);
        TempData["Success"] = "Debt created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [DisplayName("Details")]
    [Description("View details of a Debt.")]
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var debt = await _queries.GetById.HandleAsync(new GetDebtByIdQuery(id), cancellationToken);
        if (debt is null) return NotFound();
        return View(debt);
    }

    [HttpGet]
    [DisplayName("Edit")]
    [Description("Modify an existing Debt.")]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var debt = await _queries.GetById.HandleAsync(new GetDebtByIdQuery(id), cancellationToken);
        if (debt is null) return NotFound();

        await LoadDebtPersonsViewBagAsync();
        var cmd = new UpdateDebtCommand(
            debt.Id,
            debt.Amount,
            debt.DateOfBorrow,
            debt.DateOfReturn,
            debt.PromissedReturnDate,
            debt.Profit,
            debt.DebtPersonId,
            null,
            debt.Description,
            debt.IsDone
        );
        return View(cmd);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Edit")]
    [Description("Modify an existing Debt.")]
    public async Task<IActionResult> Edit(Guid id, UpdateDebtCommand dto, CancellationToken cancellationToken)
    {
        if (id != dto.Id) return BadRequest();

        var validation = await _updateValidator.ValidateAsync(dto, cancellationToken);
        if (!validation.IsValid)
        {
            foreach (var error in validation.Errors)
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            await LoadDebtPersonsViewBagAsync();
            return View(dto);
        }

        await _commands.Update.HandleAsync(dto, cancellationToken);
        TempData["Success"] = "Debt updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [DisplayName("Search Debt Persons")]
    [Description("Execute Search Debt Persons operation for Debt.")]
    public async Task<IActionResult> SearchDebtPersons(string term, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(term) || term.Length < 1)
            return Json(new System.Collections.Generic.List<object>());

        var results = await _debtPersonRepository.SearchByNameAsync(term);
        var mapped = results.Select(c => new { id = c.Id, name = c.FullName });
        return Json(mapped);
    }
}
