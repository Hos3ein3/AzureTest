using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.FootballFantasy;
using AzureTest.Application.FootballFantasy.Commands;
using AzureTest.Application.FootballFantasy.Queries;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Contracts.DTOs.FootballFantasy;
using AzureTest.MVC.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.MVC.Controllers;

[Authorize]
[DisplayName("Country")]
[Description("Manage real-world football countries.")]
public class CountryController : Controller
{
    private readonly IFootballFantasyQueryUnit _queries;
    private readonly IFootballFantasyCommandUnit _commands;
    private readonly IValidationService _validator;

    public CountryController(IFootballFantasyQueryUnit queries, IFootballFantasyCommandUnit commands, IValidationService validator)
    {
        _queries = queries;
        _commands = commands;
        _validator = validator;
    }

    [HttpGet]
    [DisplayName("Index")]
    [Description("List all football countries with filtering and pagination.")]
    public async Task<IActionResult> Index(CountryFilter filter, CancellationToken cancellationToken)
    {
        var result = await _queries.GetCountries.HandleAsync(new GetCountriesQuery(filter), cancellationToken);
        ViewBag.Filter = filter;
        return View(result);
    }

    [HttpGet]
    [DisplayName("Details")]
    [Description("View country details and flag.")]
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var result = await _queries.GetCountryById.HandleAsync(new GetCountryByIdQuery(id), cancellationToken);
        return result.ToViewResult(this);
    }

    [HttpGet]
    [DisplayName("Create")]
    [Description("Create a new football country.")]
    public IActionResult Create()
    {
        return View(new CreateCountryCommand(string.Empty, string.Empty, "Europe", null));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Create")]
    [Description("Create a new football country.")]
    public async Task<IActionResult> Create(CreateCountryCommand model, CancellationToken cancellationToken)
    {
        var validation = await _validator.ValidateAsync(model, cancellationToken);
        if (validation.IsFailure)
        {
            return validation.ToFormResult(this, model, nameof(Index));
        }

        var result = await _commands.CreateCountry.HandleAsync(model, cancellationToken);
        return result.ToFormResult(this, model, nameof(Index));
    }

    [HttpGet]
    [DisplayName("Edit")]
    [Description("Edit an existing football country.")]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var result = await _queries.GetCountryById.HandleAsync(new GetCountryByIdQuery(id), cancellationToken);
        if (result.IsFailure) return NotFound();

        var model = new UpdateCountryCommand(result.Value.Id, result.Value.Name, result.Value.Continent, result.Value.FlagUrl);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Edit")]
    [Description("Update an existing football country.")]
    public async Task<IActionResult> Edit(UpdateCountryCommand model, CancellationToken cancellationToken)
    {
        var validation = await _validator.ValidateAsync(model, cancellationToken);
        if (validation.IsFailure)
        {
            return validation.ToFormResult(this, model, nameof(Index));
        }

        var result = await _commands.UpdateCountry.HandleAsync(model, cancellationToken);
        return result.ToFormResult(this, model, nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Delete")]
    [Description("Delete a football country.")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _commands.DeleteCountry.HandleAsync(new DeleteCountryCommand(id), cancellationToken);
        return RedirectToAction(nameof(Index));
    }
}
