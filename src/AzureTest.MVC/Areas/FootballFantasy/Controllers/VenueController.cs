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

[Area("FootballFantasy")]
[Authorize]
[DisplayName("Venue")]
[Description("Manage real-world football stadiums and venues.")]
public class VenueController : Controller
{
    private readonly IFootballFantasyQueryUnit _queries;
    private readonly IFootballFantasyCommandUnit _commands;
    private readonly IValidationService _validator;

    public VenueController(IFootballFantasyQueryUnit queries, IFootballFantasyCommandUnit commands, IValidationService validator)
    {
        _queries = queries;
        _commands = commands;
        _validator = validator;
    }

    private async Task PopulateCountriesViewBagAsync(CancellationToken cancellationToken)
    {
        var countriesResult = await _queries.GetCountries.HandleAsync(new GetCountriesQuery(new CountryFilter { PageSize = 250 }), cancellationToken);
        ViewBag.Countries = countriesResult.Items;
    }

    [HttpGet]
    [DisplayName("Index")]
    [Description("List all football stadiums/venues with country association and capacity.")]
    public async Task<IActionResult> Index(VenueFilter filter, CancellationToken cancellationToken)
    {
        await PopulateCountriesViewBagAsync(cancellationToken);
        var result = await _queries.GetVenues.HandleAsync(new GetVenuesQuery(filter), cancellationToken);
        ViewBag.Filter = filter;
        return View(result);
    }

    [HttpGet]
    [DisplayName("Details")]
    [Description("View stadium details, host country, and resident teams.")]
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var result = await _queries.GetVenueById.HandleAsync(new GetVenueByIdQuery(id), cancellationToken);
        if (result.IsFailure) return NotFound();

        var teamsResult = await _queries.GetTeams.HandleAsync(new GetTeamsQuery(new TeamFilter { VenueId = id, PageSize = 50 }), cancellationToken);
        ViewBag.Teams = teamsResult.Items;

        return View(result.Value);
    }

    [HttpGet]
    [DisplayName("Create")]
    [Description("Create a new football stadium or venue.")]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        await PopulateCountriesViewBagAsync(cancellationToken);
        return View(new CreateVenueCommand(string.Empty, Guid.Empty));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Create")]
    [Description("Create a new football stadium or venue.")]
    public async Task<IActionResult> Create(CreateVenueCommand model, CancellationToken cancellationToken)
    {
        var validation = await _validator.ValidateAsync(model, cancellationToken);
        if (validation.IsFailure)
        {
            await PopulateCountriesViewBagAsync(cancellationToken);
            return validation.ToFormResult(this, model, nameof(Index));
        }

        var result = await _commands.CreateVenue.HandleAsync(model, cancellationToken);
        if (result.IsFailure) await PopulateCountriesViewBagAsync(cancellationToken);
        return result.ToFormResult(this, model, nameof(Index));
    }

    [HttpGet]
    [DisplayName("Edit")]
    [Description("Edit an existing football venue.")]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var result = await _queries.GetVenueById.HandleAsync(new GetVenueByIdQuery(id), cancellationToken);
        if (result.IsFailure) return NotFound();

        await PopulateCountriesViewBagAsync(cancellationToken);
        var model = new UpdateVenueCommand(
            result.Value.Id,
            result.Value.Name,
            result.Value.CountryId,
            result.Value.City,
            result.Value.Address,
            result.Value.Capacity,
            result.Value.Surface,
            result.Value.ImageUrl
        );
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Edit")]
    [Description("Update an existing football venue.")]
    public async Task<IActionResult> Edit(UpdateVenueCommand model, CancellationToken cancellationToken)
    {
        var validation = await _validator.ValidateAsync(model, cancellationToken);
        if (validation.IsFailure)
        {
            await PopulateCountriesViewBagAsync(cancellationToken);
            return validation.ToFormResult(this, model, nameof(Index));
        }

        var result = await _commands.UpdateVenue.HandleAsync(model, cancellationToken);
        if (result.IsFailure) await PopulateCountriesViewBagAsync(cancellationToken);
        return result.ToFormResult(this, model, nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Delete")]
    [Description("Delete a football venue.")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _commands.DeleteVenue.HandleAsync(new DeleteVenueCommand(id), cancellationToken);
        return RedirectToAction(nameof(Index));
    }
}
