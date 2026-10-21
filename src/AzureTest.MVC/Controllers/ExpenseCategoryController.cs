using AzureTest.Application.Abstractions;


using System;
using System.Linq;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Domain.Documents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;

namespace AzureTest.MVC.Controllers;

[Authorize]
[DisplayName("Expense Category")]
[Description("Manage and control Expense Category operations.")]
    public class ExpenseCategoryController : Controller
{
    private readonly INoSqlUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;
    private readonly ITenantProvider _tenantProvider;

    public ExpenseCategoryController(
        INoSqlUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        ITenantProvider tenantProvider)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _tenantProvider = tenantProvider;
    }

    private bool CanAccessCategory(ExpenseCategory category)
    {
        if (_tenantProvider.IsMultiTenancyEnabled && _tenantProvider.CurrentTenantId != Guid.Empty)
        {
            return category.TenantId == _tenantProvider.CurrentTenantId ||
                   category.UserId == _tenantProvider.CurrentTenantId;
        }

        return category.UserId == _currentUser.Id;
    }

    [HttpGet]
    [DisplayName("Index")]
    [Description("View a list of Expense Category.")]
    public async Task<IActionResult> Index()
    {
        var categories = await _unitOfWork.ExpenseCategories.GetAllAsync();
        var isMultiTenant = _tenantProvider.IsMultiTenancyEnabled && _tenantProvider.CurrentTenantId != Guid.Empty;
        var targetTenantId = _tenantProvider.CurrentTenantId;

        var userCategories = categories
            .Where(c => isMultiTenant
                ? (c.TenantId == targetTenantId || c.UserId == targetTenantId)
                : c.UserId == _currentUser.Id)
            .OrderBy(c => c.Name)
            .ToList();

        return View(userCategories);
    }

    [HttpGet]
    [DisplayName("Create")]
    [Description("Create a new Expense Category.")]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Create")]
    [Description("Create a new Expense Category.")]
    public async Task<IActionResult> Create(ExpenseCategory category)
    {
        if (!ModelState.IsValid)
            return View(category);

        var isMultiTenant = _tenantProvider.IsMultiTenancyEnabled && _tenantProvider.CurrentTenantId != Guid.Empty;
        var effectiveTenantId = isMultiTenant ? _tenantProvider.CurrentTenantId : Guid.Empty;
        var effectiveUserId = (_tenantProvider.IsAdminImpersonating && effectiveTenantId != Guid.Empty)
            ? effectiveTenantId
            : _currentUser.Id;

        category.UserId = effectiveUserId;
        if (effectiveTenantId != Guid.Empty)
        {
            category.TenantId = effectiveTenantId;
        }

        await _unitOfWork.ExpenseCategories.AddAsync(category);
        await _unitOfWork.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [DisplayName("Edit")]
    [Description("Modify an existing Expense Category.")]
    public async Task<IActionResult> Edit(Guid id)
    {
        var category = await _unitOfWork.ExpenseCategories.GetByIdAsync(id);
        if (category == null || !CanAccessCategory(category))
            return NotFound();

        return View(category);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Edit")]
    [Description("Modify an existing Expense Category.")]
    public async Task<IActionResult> Edit(Guid id, ExpenseCategory model)
    {
        if (id != model.Id) return BadRequest();

        if (!ModelState.IsValid)
            return View(model);

        var existingCategory = await _unitOfWork.ExpenseCategories.GetByIdAsync(id);
        if (existingCategory == null || !CanAccessCategory(existingCategory))
            return NotFound();

        existingCategory.Name = model.Name;
        await _unitOfWork.ExpenseCategories.UpdateAsync(id, existingCategory);
        await _unitOfWork.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}
