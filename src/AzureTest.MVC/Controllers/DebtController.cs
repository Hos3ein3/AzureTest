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

namespace AzureTest.MVC.Controllers;

[Authorize]
public class DebtController : Controller
{
    private readonly IDebtCommandUnit _commands;
    private readonly IDebtQueryUnit _queries;
    private readonly IDebtPersonRepository _debtPersonRepository;
    private readonly IValidator<CreateDebtCommand> _createValidator;
    private readonly IValidator<UpdateDebtCommand> _updateValidator;
    private readonly IValidator<DeleteDebtCommand> _deleteValidator;

    public DebtController(
        IDebtCommandUnit commands,
        IDebtQueryUnit queries,
        IDebtPersonRepository debtPersonRepository,
        IValidator<CreateDebtCommand> createValidator,
        IValidator<UpdateDebtCommand> updateValidator,
        IValidator<DeleteDebtCommand> deleteValidator)
    {
        _commands = commands;
        _queries = queries;
        _debtPersonRepository = debtPersonRepository;
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
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var debts = await _queries.GetAll.HandleAsync(new GetAllDebtsQuery(), cancellationToken);
        return View(debts);
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        await LoadDebtPersonsViewBagAsync();
        var cmd = new CreateDebtCommand(0, DateTime.Now.Date, null, null, 0, null, string.Empty, string.Empty, false);
        return View(cmd);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
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
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var debt = await _queries.GetById.HandleAsync(new GetDebtByIdQuery(id), cancellationToken);
        if (debt is null) return NotFound();
        return View(debt);
    }

    [HttpGet]
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
    public async Task<IActionResult> SearchDebtPersons(string term, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(term) || term.Length < 1)
            return Json(new System.Collections.Generic.List<object>());

        var results = await _debtPersonRepository.SearchByNameAsync(term);
        var mapped = results.Select(c => new { id = c.Id, name = c.FullName });
        return Json(mapped);
    }
}
