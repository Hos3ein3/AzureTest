
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using AzureTest.Application.Gym;
using AzureTest.Domain.Documents.Gym;
using AzureTest.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.MVC.Controllers;

[Authorize(Roles = "Admin")]
[DisplayName("Gym Admin")]
[Description("Manage Gym Trainings.")]
public class GymAdminController : Controller
{
    private readonly GetAllTrainingsQueryHandler _getAllTrainingsQuery;
    private readonly GetTrainingByIdQueryHandler _getTrainingByIdQuery;
    private readonly CreateTrainingCommandHandler _createTrainingCommand;
    private readonly UpdateTrainingCommandHandler _updateTrainingCommand;

    public GymAdminController(
        GetAllTrainingsQueryHandler getAllTrainingsQuery,
        GetTrainingByIdQueryHandler getTrainingByIdQuery,
        CreateTrainingCommandHandler createTrainingCommand,
        UpdateTrainingCommandHandler updateTrainingCommand)
    {
        _getAllTrainingsQuery = getAllTrainingsQuery;
        _getTrainingByIdQuery = getTrainingByIdQuery;
        _createTrainingCommand = createTrainingCommand;
        _updateTrainingCommand = updateTrainingCommand;
    }

    [HttpGet]
    [DisplayName("Trainings Index")]
    [Description("List all base trainings.")]
   
    public async Task<IActionResult> Index(TrainingFilter? filter, string search = "", string sort = "Name", string dir = "asc", int page = 1)
    {
        filter ??= new TrainingFilter();
        if (!string.IsNullOrWhiteSpace(search)) filter.SearchTerm = search;
        if (!string.IsNullOrWhiteSpace(sort)) filter.SortBy = sort;
        if (!string.IsNullOrWhiteSpace(dir)) filter.SortDescending = string.Equals(dir, "desc", StringComparison.OrdinalIgnoreCase);
        if (page > 0) filter.PageNumber = page;

        var result = await _getAllTrainingsQuery.HandleAsync(new GetAllTrainingsQuery(filter));
        var paged = result.IsSuccess 
            ? result.Value 
            : new AzureTest.Contracts.DTOs.Common.PagedResult<Training, TrainingFilter>(new List<Training>(), 0, 1, 10, filter);

        ViewBag.Search = filter.SearchTerm ?? "";
        ViewBag.Sort = filter.SortBy;
        ViewBag.Dir = filter.SortDescending ? "desc" : "asc";
        ViewBag.Page = paged.PageNumber;
        ViewBag.TotalPages = paged.TotalPages;
        ViewBag.Filters = filter;

        return View(paged.Items);
    }

    [HttpGet]
    [DisplayName("Create Training")]
    
    [Description("Action: Create Training")]
    public IActionResult CreateTraining()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Create Training")]
    [Description("Action: Create Training")]
    public async Task<IActionResult> CreateTraining(Training model, string? EquipmentsStr=null)
    {
        if (ModelState.IsValid)
        {
            var cmd = new CreateTrainingCommand(model.Name, model.Description ?? "", model.MediaUrl ?? "", model.Type, model.BaseCaloriesBurn ?? 0, ParseTags(EquipmentsStr));
            var result = await _createTrainingCommand.HandleAsync(cmd);
            if (result.IsSuccess)
            {
                return RedirectToAction(nameof(Index));
            }
            ModelState.AddModelError("", result.Error ?? "Failed to create");
        }
        return View(model);
    }

    [HttpGet]
    [DisplayName("Edit Training")]
    
    [Description("Action: Edit Training")]
    public async Task<IActionResult> EditTraining(Guid id)
    {
        var result = await _getTrainingByIdQuery.HandleAsync(new GetTrainingByIdQuery(id));
        if (!result.IsSuccess) return NotFound();
        return View(result.Value);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Edit Training")]
    [Description("Action: Edit Training")]
    public async Task<IActionResult> EditTraining(Guid id, Training model, string? EquipmentsStr=null)
    {
        if (id != model.Id) return BadRequest();
        
        if (ModelState.IsValid)
        {
            var cmd = new UpdateTrainingCommand(id, model.Name, model.Description ?? "", model.MediaUrl ?? "", model.Type, model.BaseCaloriesBurn ?? 0, ParseTags(EquipmentsStr));
            var result = await _updateTrainingCommand.HandleAsync(cmd);
            if (result.IsSuccess)
            {
                return RedirectToAction(nameof(Index));
            }
            ModelState.AddModelError("", result.Error ?? "Failed to update");
        }
        return View(model);
    }

    private List<string>? ParseTags(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return null;
        try {
            var tags = System.Text.Json.JsonSerializer.Deserialize<List<Dictionary<string, string>>>(input);
            return tags?.Select(t => t["value"]).ToList() ?? new List<string>();
        } catch {
            return input.Split(',').Select(x => x.Trim()).ToList();
        }
    }
}
