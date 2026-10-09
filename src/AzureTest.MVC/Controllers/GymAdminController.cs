
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
   
    public async Task<IActionResult> Index(string search = "", string sort = "Name", string dir = "asc", int page = 1)
    {
        var result = await _getAllTrainingsQuery.HandleAsync(new GetAllTrainingsQuery());
        var list = result.IsSuccess ? result.Value : new List<Training>();

        if (!string.IsNullOrWhiteSpace(search))
        {
            list = list.Where(x => x.Name.Contains(search, StringComparison.OrdinalIgnoreCase) || 
                                   (x.Description != null && x.Description.Contains(search, StringComparison.OrdinalIgnoreCase))).ToList();
        }

        if (dir == "asc")
        {
            list = sort switch {
                "Type" => list.OrderBy(x => x.Type.ToString()).ToList(),
                _ => list.OrderBy(x => x.Name).ToList()
            };
        }
        else
        {
            list = sort switch {
                "Type" => list.OrderByDescending(x => x.Type.ToString()).ToList(),
                _ => list.OrderByDescending(x => x.Name).ToList()
            };
        }

        int pageSize = 10;
        int totalItems = list.Count;
        int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
        page = Math.Max(1, Math.Min(page, totalPages > 0 ? totalPages : 1));

        var pagedList = list.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        ViewBag.Search = search;
        ViewBag.Sort = sort;
        ViewBag.Dir = dir;
        ViewBag.Page = page;
        ViewBag.TotalPages = totalPages;

        return View(pagedList);
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
    public async Task<IActionResult> CreateTraining(Training model, string EquipmentsStr)
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
    public async Task<IActionResult> EditTraining(Guid id, Training model, string EquipmentsStr)
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

    private List<string> ParseTags(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return new List<string>();
        try {
            var tags = System.Text.Json.JsonSerializer.Deserialize<List<Dictionary<string, string>>>(input);
            return tags?.Select(t => t["value"]).ToList() ?? new List<string>();
        } catch {
            return input.Split(',').Select(x => x.Trim()).ToList();
        }
    }
}
