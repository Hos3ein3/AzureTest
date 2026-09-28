using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Expenses.Commands;
using AzureTest.Application.Expenses.Queries;
using AzureTest.Application.Expenses.Units;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Contracts.DTOs.Expenses;
using AzureTest.MVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.MVC.Controllers;

[Authorize]
public class ExpenseController : Controller
{
    private readonly IExpenseCommandUnit _commands;
    private readonly IExpenseQueryUnit _queries;
    private readonly IExpenseCategoryRepository _categoryRepository;
    private readonly ICurrentUser _currentUser;

    public ExpenseController(
        IExpenseCommandUnit commands,
        IExpenseQueryUnit queries,
        IExpenseCategoryRepository categoryRepository,
        ICurrentUser currentUser)
    {
        _commands = commands;
        _queries = queries;
        _categoryRepository = categoryRepository;
        _currentUser = currentUser;
    }

    private async Task LoadCategoriesViewBagAsync()
    {
        var categories = await _categoryRepository.GetAllAsync();
        ViewBag.Categories = categories
            .Where(c => c.UserId == _currentUser.Id)
            .OrderBy(c => c.Name)
            .Select(c => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            })
            .ToList();
    }

    [HttpGet]
    public async Task<IActionResult> Index(GetAllExpensesFilters filters, CancellationToken cancellationToken)
    {
        var pagedExpenses = await _queries.GetAll.HandleAsync(new GetAllExpensesQuery(filters), cancellationToken);
        ViewBag.Filters = filters;
        return View(pagedExpenses);
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var expense = await _queries.GetById.HandleAsync(new GetExpenseByIdQuery(id), cancellationToken);
        if (expense is null) return NotFound();
        return View(expense);
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        await LoadCategoriesViewBagAsync();
        return View(new CreateExpenseDto { Date = DateTime.Now.Date });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateExpenseDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await LoadCategoriesViewBagAsync();
            return View(dto);
        }

        try
        {
            var id = await _commands.Create.HandleAsync(new CreateExpenseCommand(dto), cancellationToken);
            TempData["Success"] = "Expense created successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (FluentValidation.ValidationException ex)
        {
            foreach (var error in ex.Errors)
            {
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }
            await LoadCategoriesViewBagAsync();
            return View(dto);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var expense = await _queries.GetById.HandleAsync(new GetExpenseByIdQuery(id), cancellationToken);
        if (expense is null) return NotFound();

        await LoadCategoriesViewBagAsync();
        var dto = new UpdateExpenseDto
        {
            Id = expense.Id,
            Amount = expense.Amount,
            Date = expense.Date,
            Description = expense.Description,
            CategoryId = expense.CategoryId
        };
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, UpdateExpenseDto dto, CancellationToken cancellationToken)
    {
        if (id != dto.Id) return BadRequest();

        if (!ModelState.IsValid)
        {
            await LoadCategoriesViewBagAsync();
            return View(dto);
        }

        try
        {
            await _commands.Update.HandleAsync(new UpdateExpenseCommand(dto), cancellationToken);
            TempData["Success"] = "Expense updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (FluentValidation.ValidationException ex)
        {
            foreach (var error in ex.Errors)
            {
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }
            await LoadCategoriesViewBagAsync();
            return View(dto);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var expense = await _queries.GetById.HandleAsync(new GetExpenseByIdQuery(id), cancellationToken);
        if (expense is null) return NotFound();
        return View(expense);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id, CancellationToken cancellationToken)
    {
        await _commands.Delete.HandleAsync(new DeleteExpenseCommand(id), cancellationToken);
        TempData["Success"] = "Expense deleted successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> SearchExpenseCategories(string term, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(term) || term.Length < 1)
            return Json(new System.Collections.Generic.List<object>());

        var results = await _categoryRepository.SearchByNameAsync(term, _currentUser.Id);
        var mapped = results.Select(c => new { id = c.Id, name = c.Name });
        return Json(mapped);
    }

    [HttpGet]
    public async Task<IActionResult> Analytics(CancellationToken cancellationToken)
    {
        var filters = new GetAllExpensesFilters { PageSize = 10000, UserId = _currentUser.Id };
        var pagedExpenses = await _queries.GetAll.HandleAsync(new GetAllExpensesQuery(filters), cancellationToken);
        var expenses = pagedExpenses.Items.OrderByDescending(e => e.Date).ToList();

        var model = new ExpenseAnalyticsViewModel();
        
        // Month over Month Calculations
        var now = DateTime.UtcNow;
        var thisMonthStart = new DateTime(now.Year, now.Month, 1);
        var lastMonthStart = thisMonthStart.AddMonths(-1);
        
        var expensesThisMonth = expenses.Where(e => e.Date >= thisMonthStart).ToList();
        var expensesLastMonth = expenses.Where(e => e.Date >= lastMonthStart && e.Date < thisMonthStart).ToList();

        model.TotalThisMonth = expensesThisMonth.Sum(e => e.Amount);
        model.TotalLastMonth = expensesLastMonth.Sum(e => e.Amount);

        if (model.TotalLastMonth == 0)
        {
            model.MonthOverMonthChangePercentage = model.TotalThisMonth > 0 ? 100 : 0;
        }
        else
        {
            model.MonthOverMonthChangePercentage = (double)((model.TotalThisMonth - model.TotalLastMonth) / model.TotalLastMonth * 100);
        }

        // By Category
        model.ExpensesByCategory = expenses
            .Where(e => e.Amount > 0)
            .GroupBy(e => string.IsNullOrWhiteSpace(e.CategoryName) ? "Uncategorized" : e.CategoryName)
            .ToDictionary(g => g.Key, g => g.Sum(e => e.Amount));

        // By Month
        model.ExpensesByMonth = expenses
            .Where(e => e.Amount > 0)
            .GroupBy(e => new { e.Date.Year, e.Date.Month })
            .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
            .ToDictionary(
                g => new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"), 
                g => g.Sum(e => e.Amount)
            );

        // Detailed Breakdown
        model.MonthlyDetails = expenses
            .GroupBy(e => new { e.Date.Year, e.Date.Month })
            .OrderByDescending(g => g.Key.Year).ThenByDescending(g => g.Key.Month)
            .Select(g => new ExpenseMonthGroup
            {
                MonthName = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMMM"),
                Year = g.Key.Year.ToString(),
                TotalAmount = g.Sum(e => e.Amount),
                Expenses = g.ToList()
            })
            .ToList();

        return View(model);
    }
}
