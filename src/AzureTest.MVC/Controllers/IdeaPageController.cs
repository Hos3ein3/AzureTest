using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.IdeaTracker;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Contracts.DTOs.IdeaTracker;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AzureTest.Application.Messaging;
using AzureTest.Application.Abstractions;

namespace AzureTest.MVC.Controllers;

[Authorize]
public class IdeaPageController : Controller
{
    private readonly ICurrentUser _currentUser;
    private readonly IIdeaPageCommandUnit _commands;
    private readonly IIdeaPageQueryUnit _queries;
    private readonly IIdeaQueryUnit _ideaQueries;

    public IdeaPageController(ICurrentUser currentUser, IIdeaPageCommandUnit commands, IIdeaPageQueryUnit queries, IIdeaQueryUnit ideaQueries)
    {
        _currentUser = currentUser;
        _commands = commands;
        _queries = queries;
        _ideaQueries = ideaQueries;
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid id, CancellationToken ct)
    {
        var result = await _queries.GetIdeaPageById.HandleAsync(new GetIdeaPageByIdQuery(_currentUser.Id, id), ct);
        if (result.IsFailure) return NotFound();

        var ideaResult = await _ideaQueries.GetIdeaById.HandleAsync(new GetIdeaByIdQuery(_currentUser.Id, result.Value.IdeaId), ct);
        if (ideaResult.IsSuccess) ViewBag.Idea = ideaResult.Value;

        var pagesResult = await _queries.GetIdeaPages.HandleAsync(new GetIdeaPagesQuery(_currentUser.Id, result.Value.IdeaId), ct);
        if (pagesResult.IsSuccess) ViewBag.Pages = pagesResult.Value;

        var sectionQueries = HttpContext.RequestServices.GetService(typeof(IIdeaSectionQueryUnit)) as IIdeaSectionQueryUnit;
        if (sectionQueries != null) {
            var sectionsResult = await sectionQueries.GetIdeaSections.HandleAsync(new GetIdeaSectionsQuery(_currentUser.Id, id), ct);
            if (sectionsResult.IsSuccess) ViewBag.Sections = sectionsResult.Value;
        }

        return View(result.Value);
    }

    [HttpGet]
    public IActionResult Create(Guid ideaId)
    {
        return View(new CreateIdeaPageDto { IdeaId = ideaId });
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateIdeaPageDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(dto);

        var result = await _commands.Create.HandleAsync(new CreateIdeaPageCommand(_currentUser.Id, dto), ct);
        if (result.IsFailure)
        {
            ModelState.AddModelError("", result.Message ?? "Error creating page.");
            return View(dto);
        }

        return RedirectToAction("Details", "IdeaTracker", new { id = dto.IdeaId });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id, CancellationToken ct)
    {
        var result = await _queries.GetIdeaPageById.HandleAsync(new GetIdeaPageByIdQuery(_currentUser.Id, id), ct);
        if (result.IsFailure) return NotFound();

        var dto = new UpdateIdeaPageDto
        {
            Id = result.Value.Id,
            IdeaId = result.Value.IdeaId,
            Title = result.Value.Title,
            Content = result.Value.Content
        };
        return View(dto);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(UpdateIdeaPageDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(dto);

        var result = await _commands.Update.HandleAsync(new UpdateIdeaPageCommand(_currentUser.Id, dto), ct);
        if (result.IsFailure)
        {
            ModelState.AddModelError("", result.Message ?? "Error updating page.");
            return View(dto);
        }

        return RedirectToAction("Details", "IdeaPage", new { id = dto.Id });
    }

    [HttpPost]
    public async Task<IActionResult> Delete(Guid id, Guid ideaId, CancellationToken ct)
    {
        await _commands.Delete.HandleAsync(new DeleteIdeaPageCommand(_currentUser.Id, id), ct);
        return RedirectToAction("Details", "IdeaTracker", new { id = ideaId });
    }

    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> UpdateLayout([FromBody] UpdateIdeaPageLayoutDto dto, [FromServices] ICommandHandler<UpdateIdeaPageLayoutCommand, Result> handler, CancellationToken ct)
    {
        var result = await handler.HandleAsync(new UpdateIdeaPageLayoutCommand(_currentUser.Id, dto), ct);
        if (result.IsFailure) return BadRequest(result.Message);
        return Ok();
    }
}
