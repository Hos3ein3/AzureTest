using AzureTest.MVC.Extensions;

using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.IdeaTracker;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Contracts.DTOs.IdeaTracker;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace AzureTest.MVC.Controllers;

[Authorize]
[DisplayName("Idea Tracker")]
[Description("Manage and control Idea Tracker operations.")]
    public class IdeaTrackerController : Controller
{
    private readonly ICurrentUser _currentUser;
    private readonly IIdeaCommandUnit _commands;
    private readonly IIdeaQueryUnit _queries;
    private readonly IValidationService _validation;

    private readonly IWebHostEnvironment _env;

    public IdeaTrackerController(ICurrentUser currentUser, IIdeaCommandUnit commands, IIdeaQueryUnit queries, IValidationService validation, IWebHostEnvironment env)
    {
        _currentUser = currentUser;
        _commands = commands;
        _queries = queries;
        _validation = validation;
        _env = env;
    }

    [DisplayName("Index")]
    [Description("View a list of Idea Tracker.")]
    public async Task<IActionResult> Index(string search, int? status, CancellationToken ct)
    {
        var filter = new IdeaFilter(_currentUser.Id, search, status);
        var result = await _queries.GetIdeas.HandleAsync(new GetIdeasQuery(filter), ct);
        
        ViewBag.Search = search;
        ViewBag.Status = status;
        
        return View(result.Value);
    }

    [DisplayName("Details")]
    [Description("View details of a Idea Tracker.")]
    public async Task<IActionResult> Details(Guid id, CancellationToken ct)
    {
        var result = await _queries.GetIdeaById.HandleAsync(new GetIdeaByIdQuery(_currentUser.Id, id), ct);
        if (result.IsFailure) return NotFound();
        
        // Fetch Sub-Pages (Ideas where ParentId == id)
        var filter = new IdeaFilter(_currentUser.Id);
        var allIdeasResult = await _queries.GetIdeas.HandleAsync(new GetIdeasQuery(filter), ct);
        if (allIdeasResult.IsSuccess)
        {
            ViewBag.SubPages = allIdeasResult.Value.Where(x => x.ParentId == id).ToList();
        }
        else
        {
            ViewBag.SubPages = new List<IdeaDto>();
        }

        var ideaPageQueryUnit = HttpContext.RequestServices.GetService(typeof(IIdeaPageQueryUnit)) as IIdeaPageQueryUnit;
        if (ideaPageQueryUnit != null)
        {
            var pagesResult = await ideaPageQueryUnit.GetIdeaPages.HandleAsync(new GetIdeaPagesQuery(_currentUser.Id, id), ct);
            ViewBag.Pages = pagesResult.IsSuccess ? pagesResult.Value : new List<IdeaPageDto>();
        }
        else
        {
            ViewBag.Pages = new List<IdeaPageDto>();
        }

        return View(result.Value);
    }

    [HttpGet]
    [DisplayName("Create")]
    [Description("Create a new Idea Tracker.")]
    public IActionResult Create(Guid? parentId)
    {
        return View(new CreateIdeaDto { ParentId = parentId });
    }

    [HttpPost]
    [DisplayName("Create")]
    [Description("Create a new Idea Tracker.")]
    public async Task<IActionResult> Create(CreateIdeaDto dto, CancellationToken ct)
    {
        var validation = await _validation.ValidateAsync(dto, ct);
        if (validation.IsFailure)
        {
            validation.AddToModelState(ModelState);
            return View(dto);
        }

        var result = await _commands.Create.HandleAsync(new CreateIdeaCommand(_currentUser.Id, dto), ct);
        if (result.IsFailure)
        {
            ModelState.AddModelError("", result.Message ?? "Error creating idea.");
            return View(dto);
        }

        return RedirectToAction(nameof(Details), new { id = result.Value });
    }

    [HttpGet]
    [DisplayName("Edit")]
    [Description("Modify an existing Idea Tracker.")]
    public async Task<IActionResult> Edit(Guid id, CancellationToken ct)
    {
        var result = await _queries.GetIdeaById.HandleAsync(new GetIdeaByIdQuery(_currentUser.Id, id), ct);
        if (result.IsFailure) return NotFound();

        var dto = new UpdateIdeaDto
        {
            Id = result.Value.Id,
            ParentId = result.Value.ParentId,
            Title = result.Value.Title,
            Content = result.Value.Content,
            Icon = result.Value.Icon,
            CoverImage = result.Value.CoverImage,
            ThemeColor = result.Value.ThemeColor,
            Status = result.Value.Status,
            Priority = result.Value.Priority,
            Tags = result.Value.Tags
        };
        return View(dto);
    }

    [HttpPost]
    [DisplayName("Edit")]
    [Description("Modify an existing Idea Tracker.")]
    public async Task<IActionResult> Edit(UpdateIdeaDto dto, CancellationToken ct)
    {
        var validation = await _validation.ValidateAsync(dto, ct);
        if (validation.IsFailure)
        {
            validation.AddToModelState(ModelState);
            return View(dto);
        }

        var result = await _commands.Update.HandleAsync(new UpdateIdeaCommand(_currentUser.Id, dto), ct);
        if (result.IsFailure)
        {
            ModelState.AddModelError("", result.Message ?? "Error updating idea.");
            return View(dto);
        }

        return RedirectToAction(nameof(Details), new { id = dto.Id });
    }

    [HttpPost]
    [IgnoreAntiforgeryToken]
    [DisplayName("Upload Image")]
    [Description("Execute Upload Image operation for Idea Tracker.")]
    public async Task<IActionResult> UploadImage(IFormFile? file, IFormFile? image)
    {
        var uploadedFile = file ?? image;
        if (uploadedFile == null || uploadedFile.Length == 0) return BadRequest(new { error = "No file uploaded." });
        
        var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "ideas");
        Directory.CreateDirectory(uploadsFolder);
        
        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(uploadedFile.FileName);
        var filePath = Path.Combine(uploadsFolder, fileName);
        
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await uploadedFile.CopyToAsync(stream);
        }
        
        return Json(new { location = $"/uploads/ideas/{fileName}" });
    }
    [DisplayName("Delete")]
    [Description("Delete a Idea Tracker.")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _commands.Delete.HandleAsync(new DeleteIdeaCommand(_currentUser.Id, id), ct);
        return RedirectToAction(nameof(Index));
    }
}
