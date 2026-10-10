using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.SampleNotes;
using AzureTest.Application.SampleNotes.Commands;
using AzureTest.Application.SampleNotes.Queries;
using AzureTest.Contracts.DTOs.SampleNotes;
using AzureTest.MVC.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;

namespace AzureTest.MVC.Controllers;

[Authorize]
[DisplayName("Sample Note")]
[Description("Manage and control Sample Note operations.")]
    public class SampleNoteController : Controller
{
    private readonly IValidationService _validationService;
    private readonly ISampleNoteCommandUnit _commands;
    private readonly ISampleNoteQueryUnit _queries;

    public SampleNoteController(
        IValidationService validationService,
        ISampleNoteCommandUnit commands,
        ISampleNoteQueryUnit queries)
    {
        _validationService = validationService;
        _commands = commands;
        _queries = queries;
    }

    // GET /SampleNote
    [DisplayName("Index")]
    [Description("View a list of Sample Note.")]
    public async Task<IActionResult> Index([FromQuery] SampleNoteFilter filter, CancellationToken ct)
    {
        var notes = await _queries.GetAll.HandleAsync(new GetAllSampleNotesQuery(filter), ct);
        ViewBag.Filter = filter;
        return View(notes);
    }

    // GET /SampleNote/Create
    [DisplayName("Create")]
    [Description("Create a new Sample Note.")]
    public IActionResult Create() => View(new CreateSampleNoteDto());

    // POST /SampleNote/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Create")]
    [Description("Create a new Sample Note.")]
    public async Task<IActionResult> Create(CreateSampleNoteDto dto, CancellationToken ct)
    {
        // Step 1: Validate the DTO using IValidationService (auto-finds CreateSampleNoteDtoValidator via DI)
        var validation = await _validationService.ValidateAsync(dto, ct);
        if (validation.IsFailure)
            return validation.ToFormResult(this, dto, nameof(Index));

        // Step 2: Map to command and execute
        var res=await _commands.Create.HandleAsync(new CreateSampleNoteCommand(dto), ct);
        TempData["Success"] = "Note created successfully!";
        TempData.SetToast(validation);
        return RedirectToAction(nameof(Index));
    }

    // GET /SampleNote/Edit/{id}
    [DisplayName("Edit")]
    [Description("Modify an existing Sample Note.")]
    public async Task<IActionResult> Edit(Guid id, CancellationToken ct)
    {
        var note = await _queries.GetById.HandleAsync(new GetSampleNoteByIdQuery(id), ct);
        if (note is null) return NotFound();

        
        var dto = new UpdateSampleNoteDto
        {
            Id = note.Id,
            Title = note.Title,
            Content = note.Content,
            Category = note.Category,
            Priority = note.Priority,
            IsPinned = note.IsPinned
        };
        return View(dto);
    }

    // POST /SampleNote/Edit
    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Edit")]
    [Description("Modify an existing Sample Note.")]
    public async Task<IActionResult> Edit(UpdateSampleNoteDto dto, CancellationToken ct)
    {
        // Step 1: Validate the DTO
        var validation = await _validationService.ValidateAsync(dto, ct);
        if (validation.IsFailure)
            return validation.ToFormResult(this, dto, nameof(Index));

        // Step 2: Execute update command
        var updated = await _commands.Update.HandleAsync(new UpdateSampleNoteCommand(dto), ct);
        if (updated.IsFailure)
        {
            ModelState.AddModelError(string.Empty, "Note not found or could not be updated.");
            return updated.ToFormResult(this, dto, nameof(Index));
            
            
        }

        TempData["Success"] = "Note updated successfully!";
        return updated.ToFormResult(this, dto, nameof(Index));
    }

    // POST /SampleNote/Delete/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Delete")]
    [Description("Delete a Sample Note.")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _commands.Delete.HandleAsync(new DeleteSampleNoteCommand(id), ct);
        TempData["Success"] = "Note deleted.";
        return RedirectToAction(nameof(Index));
    }
}
