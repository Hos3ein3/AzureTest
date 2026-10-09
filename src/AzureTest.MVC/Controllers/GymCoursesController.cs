using AzureTest.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;

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
    private readonly IBlobService _blobService;

    public GymCoursesController(
        GetAllTrainingCoursesQueryHandler getAllTrainingCoursesQuery,
        GetTrainingCourseByIdQueryHandler getTrainingCourseByIdQuery,
        CreateTrainingCourseCommandHandler createTrainingCourseCommand,
        UpdateTrainingCourseCommandHandler updateTrainingCourseCommand,
        GetAllTrainingsQueryHandler getAllTrainingsQuery, IBlobService blobService)
    {
        _getAllTrainingCoursesQuery = getAllTrainingCoursesQuery;
        _getTrainingCourseByIdQuery = getTrainingCourseByIdQuery;
        _createTrainingCourseCommand = createTrainingCourseCommand;
        _updateTrainingCourseCommand = updateTrainingCourseCommand;
        _getAllTrainingsQuery = getAllTrainingsQuery;
        _blobService = blobService;
    }

    [HttpGet]
    [DisplayName("Courses Index")]
    [Description("List all training courses.")]
    public async Task<IActionResult> Index(TrainingCourseFilter? filter, string search = "", string sort = "Name", string dir = "asc", int page = 1)
    {
        filter ??= new TrainingCourseFilter();
        if (!string.IsNullOrWhiteSpace(search)) filter.SearchTerm = search;
        if (!string.IsNullOrWhiteSpace(sort)) filter.SortBy = sort;
        if (!string.IsNullOrWhiteSpace(dir)) filter.SortDescending = string.Equals(dir, "desc", StringComparison.OrdinalIgnoreCase);
        if (page > 0) filter.PageNumber = page;

        var result = await _getAllTrainingCoursesQuery.HandleAsync(new GetAllTrainingCoursesQuery(filter));
        var paged = result.IsSuccess
            ? result.Value
            : new AzureTest.Contracts.DTOs.Common.PagedResult<TrainingCourse, TrainingCourseFilter>(new List<TrainingCourse>(), 0, 1, 10, filter);

        ViewBag.Search = filter.SearchTerm ?? "";
        ViewBag.Sort = filter.SortBy;
        ViewBag.Dir = filter.SortDescending ? "desc" : "asc";
        ViewBag.Page = paged.PageNumber;
        ViewBag.TotalPages = paged.TotalPages;
        ViewBag.Filters = filter;

        return View(paged.Items);
    }

    [HttpGet]
    [DisplayName("Create Course")]
    public async Task<IActionResult> Create()
    {
        var trainingsResult = await _getAllTrainingsQuery.HandleAsync(new GetAllTrainingsQuery(new TrainingFilter { PageSize = -1 }));
        ViewBag.Trainings = trainingsResult.IsSuccess ? trainingsResult.Value.Items : new List<Training>();
        return View(new TrainingCourse { Weeks = new List<CourseWeek>() });
    }

    [HttpPost]
    
    [HttpPost]
    public async Task<IActionResult> UploadMedia(IFormFile file)
    {
        if (file == null || file.Length == 0) return BadRequest("No file uploaded");
        var ext = System.IO.Path.GetExtension(file.FileName);
        var blobName = $"{Guid.NewGuid()}{ext}";
        using var stream = file.OpenReadStream();
        var url = await _blobService.UploadFileAsync("gym", blobName, stream, file.ContentType);
        return Json(new { success = true, url = url });
    }

    public async Task<IActionResult> Create([FromBody] TrainingCourse model)
    {
        if (ModelState.IsValid)
        {
            var cmd = new CreateTrainingCourseCommand(model.Name, model.Description ?? "", model.Tags ?? new List<string>(), model.Weeks ?? new List<CourseWeek>(), model.ImagePath, model.VideoPath);
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
        
        var trainingsResult = await _getAllTrainingsQuery.HandleAsync(new GetAllTrainingsQuery(new TrainingFilter { PageSize = -1 }));
        ViewBag.Trainings = trainingsResult.IsSuccess ? trainingsResult.Value.Items : new List<Training>();
        
        return View(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(Guid id, [FromBody] TrainingCourse model)
    {
        if (id != model.Id) return BadRequest();

        if (ModelState.IsValid)
        {
            var cmd = new UpdateTrainingCourseCommand(id, model.Name, model.Description ?? "", model.Tags ?? new List<string>(), model.Weeks ?? new List<CourseWeek>(), model.ImagePath, model.VideoPath);
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
