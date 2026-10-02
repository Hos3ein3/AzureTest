using AzureTest.Application.Abstractions;


using System;
using System.Threading;
using System.Threading.Tasks;

using AzureTest.Application.Companies;
using AzureTest.Application.Companies.Commands.CrawlIndCompanies;
using AzureTest.Application.Companies.Commands.CreateCompany;
using AzureTest.Application.Companies.Commands.DeleteCompany;
using AzureTest.Application.Companies.Commands.UpdateCompany;
using AzureTest.Application.Companies.Queries.GetAllCompanies;
using AzureTest.Application.Companies.Queries.GetCompanyById;
using AzureTest.Contracts.DTOs.Companies;

using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using Microsoft.Extensions.Logging;

using Microsoft.AspNetCore.Authorization;

namespace AzureTest.MVC.Controllers;

[Authorize]
[DisplayName("Company")]
[Description("Manage and control Company operations.")]
    public class CompanyController : Controller
{
    private readonly ICompanyCommandUnit _companyCommandUnit;
    private readonly ICompanyQueryUnit _companyQueryUnit;
    private readonly ILogger<CompanyController> _logger;

    public CompanyController(ILogger<CompanyController> logger, ICompanyCommandUnit companyCommandUnit,
        ICompanyQueryUnit companyQueryUnit)
    {
        _logger = logger;
        _companyCommandUnit = companyCommandUnit;
        _companyQueryUnit = companyQueryUnit;
    }

    // GET: /Company
    [DisplayName("Index")]
    [Description("View a list of Company.")]
    public async Task<IActionResult> Index(GetAllCompaniesFilters filters,CancellationToken cancellationToken)
    {
        var pagedCompanies = await _companyQueryUnit.GetAll.HandleAsync(new GetAllCompaniesQuery(filters), cancellationToken);
        ViewBag.Filters = filters;
        return View(pagedCompanies);
    }

    // POST: /Company/Crawl
    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Crawl")]
    [Description("Execute Crawl operation for Company.")]
    public async Task<IActionResult> Crawl(CancellationToken cancellationToken)
    {
        var result =
            await _companyCommandUnit.CrawlCompanies.HandleAsync(new CrawlIndCompaniesCommand(), cancellationToken);

        if (result.Value.TotalProcessed> 0)
        {
            TempData["SuccessMessage"] =
                $"Crawl completed successfully! Processed {result.Value.TotalProcessed} companies. Added {result.Value.NewlyAdded} new companies.";
        }
        else
        {
            TempData["ErrorMessage"] = $"Crawl failed: {result.Value.TotalProcessed}";
        }

        return RedirectToAction(nameof(Index));
    }

    // GET: /Company/Details/5
    [DisplayName("Details")]
    [Description("View details of a Company.")]
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var company = await _companyQueryUnit.GetById.HandleAsync(new GetCompanyByIdQuery(id), cancellationToken);
        if (company == null) return NotFound();

        return View(company);
    }

    // GET: /Company/Create
    [DisplayName("Create")]
    [Description("Create a new Company.")]
    public IActionResult Create()
    {
        return View();
    }

    // POST: /Company/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Create")]
    [Description("Create a new Company.")]
    public async Task<IActionResult> Create(CreateCompanyDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            //ModelState.AddModelError(string.Empty, result.Error);
            return View(dto);
        }

        var result =
            await _companyCommandUnit.CreateCompany.HandleAsync(new CreateCompanyCommand(dto), cancellationToken);
        TempData["Success"] = "Resume created successfully.";
        return RedirectToAction(nameof(Details), new { result});
    }

    // GET: /Company/Edit/5
    [DisplayName("Edit")]
    [Description("Modify an existing Company.")]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var company = await _companyQueryUnit.GetById.HandleAsync(new GetCompanyByIdQuery(id), cancellationToken);
        if (company == null) return NotFound();

        var dto = new UpdateCompanyDto(company.Id, company.CompanyName, company.KvkNumber, company.IsINDMember,company.Location);
        return View(dto);
    }

    // POST: /Company/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Edit")]
    [Description("Modify an existing Company.")]
    public async Task<IActionResult> Edit(Guid id, UpdateCompanyDto dto, CancellationToken cancellationToken)
    {
        if (id != dto.Id) return BadRequest();

        if (ModelState.IsValid)
        {
            var result = await _companyCommandUnit.UpdateCompany.HandleAsync(new UpdateCompanyCommand(dto), cancellationToken);
            if (result.IsSuccess)
                return RedirectToAction(nameof(Index));

            //ModelState.AddModelError(string.Empty, result.Error);
        }

        return View(dto);
    }

    // GET: /Company/Delete/5
    [DisplayName("Delete")]
    [Description("Delete a Company.")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var company = await _companyQueryUnit.GetById.HandleAsync(new GetCompanyByIdQuery(id), cancellationToken);
        if (company == null) return NotFound();

        return View(company);
    }

    // POST: /Company/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [DisplayName("Delete Confirmed")]
    [Description("Delete a Company.")]
    public async Task<IActionResult> DeleteConfirmed(Guid id, CancellationToken cancellationToken)
    {
        var result = await _companyCommandUnit.DeleteCompany.HandleAsync(new DeleteCompanyCommand(id), cancellationToken);
        if (result.IsFailure)
        {
            //TempData["ErrorMessage"] = result.Error;
            return RedirectToAction(nameof(Delete), new { id });
        }

        return RedirectToAction(nameof(Index));
    }
}