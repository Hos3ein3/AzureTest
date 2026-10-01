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
    public async Task<IActionResult> Index(int page = 1, string sort = "DateOfBorrow", string dir = "desc", string search = "", CancellationToken cancellationToken = default)
    {
        var debts = await _queries.GetAll.HandleAsync(new GetAllDebtsQuery(), cancellationToken);
        
        if (!string.IsNullOrWhiteSpace(search))
        {
            debts = debts.Where(d => (d.Description != null && d.Description.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                                     (d.DebtPersonName != null && d.DebtPersonName.Contains(search, StringComparison.OrdinalIgnoreCase)))
                         .ToList();
        }

        debts = sort switch
        {
            "DebtPersonName" => dir == "asc" ? debts.OrderBy(d => d.DebtPersonName).ToList() : debts.OrderByDescending(d => d.DebtPersonName).ToList(),
            "Amount" => dir == "asc" ? debts.OrderBy(d => d.Amount).ToList() : debts.OrderByDescending(d => d.Amount).ToList(),
            "DateOfReturn" => dir == "asc" ? debts.OrderBy(d => d.DateOfReturn).ToList() : debts.OrderByDescending(d => d.DateOfReturn).ToList(),
            "Description" => dir == "asc" ? debts.OrderBy(d => d.Description).ToList() : debts.OrderByDescending(d => d.Description).ToList(),
            _ => dir == "asc" ? debts.OrderBy(d => d.DateOfBorrow).ToList() : debts.OrderByDescending(d => d.DateOfBorrow).ToList()
        };

        int pageSize = 10;
        var totalPages = (int)Math.Ceiling(debts.Count / (double)pageSize);
        var pagedDebts = debts.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;
        ViewBag.CurrentSort = sort;
        ViewBag.CurrentDir = dir;
        ViewBag.CurrentSearch = search;

        return View(pagedDebts);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var cmd = new DeleteDebtCommand(id);
        var validation = await _deleteValidator.ValidateAsync(cmd, cancellationToken);
        if (!validation.IsValid)
        {
            return BadRequest();
        }

        await _commands.Delete.HandleAsync(cmd, cancellationToken);
        TempData["Success"] = "Debt deleted successfully.";
        return RedirectToAction(nameof(Index));
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
