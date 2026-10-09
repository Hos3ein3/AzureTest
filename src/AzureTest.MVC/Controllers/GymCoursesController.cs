
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
