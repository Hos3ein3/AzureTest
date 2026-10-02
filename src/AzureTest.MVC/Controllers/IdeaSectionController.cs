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
public class IdeaSectionController : Controller
{
    private readonly ICurrentUser _currentUser;
    private readonly IIdeaSectionCommandUnit _commands;
    private readonly IIdeaSectionQueryUnit _queries;
    private readonly IIdeaPageQueryUnit _pageQueries;

    public IdeaSectionController(ICurrentUser currentUser, IIdeaSectionCommandUnit commands, IIdeaSectionQueryUnit queries, IIdeaPageQueryUnit pageQueries)
    {
        _currentUser = currentUser;
        _commands = commands;
        _queries = queries;
        _pageQueries = pageQueries;
    }

    [HttpGet]
    public IActionResult Create(Guid ideaPageId)
    {
        return View(new CreateIdeaSectionDto { IdeaPageId = ideaPageId });
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateIdeaSectionDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(dto);

        var result = await _commands.Create.HandleAsync(new CreateIdeaSectionCommand(_currentUser.Id, dto), ct);
        if (result.IsFailure)
        {
            ModelState.AddModelError("", result.Message ?? "Error creating section.");
            return View(dto);
        }

        return RedirectToAction("Details", "IdeaPage", new { id = dto.IdeaPageId });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id, CancellationToken ct)
    {
        var result = await _queries.GetIdeaSectionById.HandleAsync(new GetIdeaSectionByIdQuery(_currentUser.Id, id), ct);
        if (result.IsFailure) return NotFound();

        var dto = new UpdateIdeaSectionDto
        {
            Id = result.Value.Id,
            IdeaPageId = result.Value.IdeaPageId,
            Title = result.Value.Title,
            Content = result.Value.Content,
            OrderIndex = result.Value.OrderIndex
        };
        return View(dto);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(UpdateIdeaSectionDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(dto);

        var result = await _commands.Update.HandleAsync(new UpdateIdeaSectionCommand(_currentUser.Id, dto), ct);
        if (result.IsFailure)
        {
            ModelState.AddModelError("", result.Message ?? "Error updating section.");
            return View(dto);
        }

        return RedirectToAction("Details", "IdeaPage", new { id = dto.IdeaPageId });
    }

    [HttpPost]
    public async Task<IActionResult> Delete(Guid id, Guid ideaPageId, CancellationToken ct)
    {
        await _commands.Delete.HandleAsync(new DeleteIdeaSectionCommand(_currentUser.Id, id), ct);
        return RedirectToAction("Details", "IdeaPage", new { id = ideaPageId });
    }

    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> UpdateLayout([FromBody] UpdateIdeaSectionLayoutDto dto, [FromServices] ICommandHandler<UpdateIdeaSectionLayoutCommand, Result> handler, CancellationToken ct)
    {
        var result = await handler.HandleAsync(new UpdateIdeaSectionLayoutCommand(_currentUser.Id, dto), ct);
        if (result.IsFailure) return BadRequest(result.Message);
        return Ok();
    }
}
