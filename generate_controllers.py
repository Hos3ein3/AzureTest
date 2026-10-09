import os

gym_admin = """
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
    public IActionResult CreateTraining()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateTraining(Training model)
    {
        if (ModelState.IsValid)
        {
            var cmd = new CreateTrainingCommand(model.Name, model.Description ?? "", model.MediaUrl ?? "", model.Type, model.BaseCaloriesBurn);
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
    public async Task<IActionResult> EditTraining(Guid id)
    {
        var result = await _getTrainingByIdQuery.HandleAsync(new GetTrainingByIdQuery(id));
        if (!result.IsSuccess) return NotFound();
        return View(result.Value);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditTraining(Guid id, Training model)
    {
        if (id != model.Id) return BadRequest();
        
        if (ModelState.IsValid)
        {
            var cmd = new UpdateTrainingCommand(id, model.Name, model.Description ?? "", model.MediaUrl ?? "", model.Type, model.BaseCaloriesBurn);
            var result = await _updateTrainingCommand.HandleAsync(cmd);
            if (result.IsSuccess)
            {
                return RedirectToAction(nameof(Index));
            }
            ModelState.AddModelError("", result.Error ?? "Failed to update");
        }
        return View(model);
    }
}
"""

gym_courses = """
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using AzureTest.Application.Gym;
using AzureTest.Domain.Documents.Gym;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.MVC.Controllers;

[Authorize(Roles = "Admin")]
[DisplayName("Gym Courses")]
[Description("Manage Gym Courses.")]
public class GymCoursesController : Controller
{
    private readonly GetAllTrainingCoursesQueryHandler _getAllTrainingCoursesQuery;
    private readonly GetTrainingCourseByIdQueryHandler _getTrainingCourseByIdQuery;
    private readonly CreateTrainingCourseCommandHandler _createTrainingCourseCommand;
    private readonly UpdateTrainingCourseCommandHandler _updateTrainingCourseCommand;
    private readonly GetAllTrainingsQueryHandler _getAllTrainingsQuery;

    public GymCoursesController(
        GetAllTrainingCoursesQueryHandler getAllTrainingCoursesQuery,
        GetTrainingCourseByIdQueryHandler getTrainingCourseByIdQuery,
        CreateTrainingCourseCommandHandler createTrainingCourseCommand,
        UpdateTrainingCourseCommandHandler updateTrainingCourseCommand,
        GetAllTrainingsQueryHandler getAllTrainingsQuery)
    {
        _getAllTrainingCoursesQuery = getAllTrainingCoursesQuery;
        _getTrainingCourseByIdQuery = getTrainingCourseByIdQuery;
        _createTrainingCourseCommand = createTrainingCourseCommand;
        _updateTrainingCourseCommand = updateTrainingCourseCommand;
        _getAllTrainingsQuery = getAllTrainingsQuery;
    }

    [HttpGet]
    [DisplayName("Courses Index")]
    [Description("List all training courses.")]
    public async Task<IActionResult> Index(string search = "", string sort = "Name", string dir = "asc", int page = 1)
    {
        var result = await _getAllTrainingCoursesQuery.HandleAsync(new GetAllTrainingCoursesQuery());
        var list = result.IsSuccess ? result.Value : new List<TrainingCourse>();

        if (!string.IsNullOrWhiteSpace(search))
        {
            list = list.Where(x => x.Name.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        if (dir == "asc")
        {
            list = list.OrderBy(x => x.Name).ToList();
        }
        else
        {
            list = list.OrderByDescending(x => x.Name).ToList();
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
    [DisplayName("Create Course")]
    public async Task<IActionResult> Create()
    {
        var trainingsResult = await _getAllTrainingsQuery.HandleAsync(new GetAllTrainingsQuery());
        ViewBag.Trainings = trainingsResult.IsSuccess ? trainingsResult.Value : new List<Training>();
        return View(new TrainingCourse { Weeks = new List<CourseWeek>() });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] TrainingCourse model)
    {
        if (ModelState.IsValid)
        {
            var cmd = new CreateTrainingCourseCommand(model.Name, model.Description ?? "", model.Tags ?? new List<string>(), model.Weeks ?? new List<CourseWeek>());
            var result = await _createTrainingCourseCommand.HandleAsync(cmd);
            if (result.IsSuccess)
            {
                return Json(new { success = true });
            }
            return Json(new { success = false, message = result.Error });
        }
        return Json(new { success = false, message = "Invalid data" });
    }

    [HttpGet]
    [DisplayName("Edit Course")]
    public async Task<IActionResult> Edit(Guid id)
    {
        var result = await _getTrainingCourseByIdQuery.HandleAsync(new GetTrainingCourseByIdQuery(id));
        if (!result.IsSuccess) return NotFound();
        
        var trainingsResult = await _getAllTrainingsQuery.HandleAsync(new GetAllTrainingsQuery());
        ViewBag.Trainings = trainingsResult.IsSuccess ? trainingsResult.Value : new List<Training>();
        
        return View(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(Guid id, [FromBody] TrainingCourse model)
    {
        if (id != model.Id) return BadRequest();

        if (ModelState.IsValid)
        {
            var cmd = new UpdateTrainingCourseCommand(id, model.Name, model.Description ?? "", model.Tags ?? new List<string>(), model.Weeks ?? new List<CourseWeek>());
            var result = await _updateTrainingCourseCommand.HandleAsync(cmd);
            if (result.IsSuccess)
            {
                return Json(new { success = true });
            }
            return Json(new { success = false, message = result.Error });
        }
        return Json(new { success = false, message = "Invalid data" });
    }
}
"""

with open("src/AzureTest.MVC/Controllers/GymAdminController.cs", "w") as f:
    f.write(gym_admin)

with open("src/AzureTest.MVC/Controllers/GymCoursesController.cs", "w") as f:
    f.write(gym_courses)

print("Controllers Generated")
