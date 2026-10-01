using AzureTest.MVC.Extensions;

using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.IdeaTracker;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Contracts.DTOs.IdeaTracker;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace AzureTest.MVC.Controllers;

[Authorize]
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

    public async Task<IActionResult> Index(string search, int? status, CancellationToken ct)
    {
        var filter = new IdeaFilter(_currentUser.Id, search, status);
        var result = await _queries.GetIdeas.HandleAsync(new GetIdeasQuery(filter), ct);
        
        ViewBag.Search = search;
        ViewBag.Status = status;
        
        return View(result.Value);
    }

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

        return View(result.Value);
    }

    [HttpGet]
    public IActionResult Create(Guid? parentId)
    {
        return View(new CreateIdeaDto { ParentId = parentId });
    }

    [HttpPost]
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

    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> UploadImage(IFormFile image)
    {
        if (image == null || image.Length == 0) return BadRequest();
        
        var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "ideas");
        Directory.CreateDirectory(uploadsFolder);
        
        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
        var filePath = Path.Combine(uploadsFolder, fileName);
        
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await image.CopyToAsync(stream);
        }
        
        return Json(new { location = $"/uploads/ideas/{fileName}" });
    }
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _commands.Delete.HandleAsync(new DeleteIdeaCommand(_currentUser.Id, id), ct);
        return RedirectToAction(nameof(Index));
    }
}
