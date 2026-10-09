using System.ComponentModel;
using System.Security.Claims;
using System;
using System.Linq;
using System.Threading.Tasks;
using AzureTest.Application.Gym;
using AzureTest.Domain.Documents.Gym;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.MVC.Controllers;

[Authorize]
[DisplayName("Workout Management")]
[Description("Allows users to enroll in courses, customize days, and perform sessions.")]

public class HistoryLogItem { public TrainingLog Log { get; set; } public DateTime SessionDate { get; set; } }
public class WorkoutController : Controller {
    [HttpGet]
    [DisplayName("Workout History")]
    [Description("View logs grouped by training exercise.")]
    public async Task<IActionResult> History()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if(string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId)) return Unauthorized();

        var sessionsRes = await _getUserSessions.HandleAsync(new GetUserTrainingSessionsQuery(userId));
        var sessions = sessionsRes.IsSuccess ? sessionsRes.Value : new List<UserTrainingSession>();

        var allTrainingsRes = await _getAllTrainings.HandleAsync(new GetAllTrainingsQuery(new TrainingFilter { PageSize = -1 }));
        var allTrainings = allTrainingsRes.IsSuccess ? allTrainingsRes.Value.Items : new List<Training>();
        var trainingsDict = allTrainings.ToDictionary(t => t.Id, t => t);

        // Flatten all logs and enrich with Date from the Session, then group by TrainingId
        var allLogs = sessions.SelectMany(s => s.Logs.Select(l => new HistoryLogItem {
            Log = l,
            SessionDate = s.StartTime
        })).ToList();

        var grouped = allLogs.GroupBy(x => x.Log.TrainingId)
                             .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.SessionDate).ToList());

        ViewBag.TrainingsDict = trainingsDict;
        return View(grouped);
    }

    private readonly GetAllTrainingCoursesQueryHandler _getAllCourses;
    private readonly GetTrainingCourseByIdQueryHandler _getCourseById;
    private readonly GetUserGymCoursesQueryHandler _getUserGymCourses;
    private readonly RegisterUserGymCourseCommandHandler _registerUserGymCourse;
    private readonly UpdateUserGymCourseDayCommandHandler _updateUserGymCourseDay;
    private readonly GetUserGymCourseByIdQueryHandler _getUserGymCourseById;
    private readonly GetAllTrainingsQueryHandler _getAllTrainings;
    private readonly GetUserTrainingSessionsQueryHandler _getUserSessions;
    private readonly SaveUserTrainingSessionCommandHandler _saveSession;
    
    

    public WorkoutController(
        GetAllTrainingCoursesQueryHandler getAllCourses, GetTrainingCourseByIdQueryHandler getCourseById,
        GetUserGymCoursesQueryHandler getUserGymCourses,
        RegisterUserGymCourseCommandHandler registerUserGymCourse, UpdateUserGymCourseDayCommandHandler updateUserGymCourseDay,
        GetUserGymCourseByIdQueryHandler getUserGymCourseById,
        GetAllTrainingsQueryHandler getAllTrainings,
        GetUserTrainingSessionsQueryHandler getUserSessions,
        SaveUserTrainingSessionCommandHandler saveSession) 
    {
        _getAllCourses = getAllCourses;
        _getCourseById = getCourseById;
        _getUserGymCourses = getUserGymCourses;
        _registerUserGymCourse = registerUserGymCourse;
        _updateUserGymCourseDay = updateUserGymCourseDay;
        _getUserGymCourseById = getUserGymCourseById;
        _getAllTrainings = getAllTrainings;
        _getUserSessions = getUserSessions;
        _saveSession = saveSession;
    }

    
    private Guid GetUserId()
    {
        var userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
        return Guid.TryParse(userIdString, out var userId) ? userId : Guid.Empty;
    }

    
    [DisplayName("Course Details")]
    [Description("View details of a global training course before enrolling.")]
    public async Task<IActionResult> Details(Guid id) {
        var res = await _getCourseById.HandleAsync(new GetTrainingCourseByIdQuery(id));
        if (!res.IsSuccess) return NotFound();

        var trainingsRes = await _getAllTrainings.HandleAsync(new GetAllTrainingsQuery(new TrainingFilter { PageSize = -1 }));
        ViewBag.Trainings = trainingsRes.IsSuccess ? trainingsRes.Value.Items.ToDictionary(t => t.Id) : new Dictionary<Guid, AzureTest.Domain.Documents.Gym.Training>();

        return View(res.Value);
    }

    [DisplayName("Workout Dashboard")]
    [Description("View enrolled courses and available global courses.")]
    public async Task<IActionResult> Index() {
        var coursesRes = await _getAllCourses.HandleAsync(new GetAllTrainingCoursesQuery(new TrainingCourseFilter { PageSize = -1 }));
        var userGymCoursesRes = await _getUserGymCourses.HandleAsync(new GetUserGymCoursesQuery(GetUserId()));

        ViewBag.AvailableCourses = coursesRes.IsSuccess ? coursesRes.Value.Items : new List<AzureTest.Domain.Documents.Gym.TrainingCourse>();
        ViewBag.UserGymCourses = userGymCoursesRes.IsSuccess ? userGymCoursesRes.Value : new();

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(Guid courseId) {
        var res = await _registerUserGymCourse.HandleAsync(new RegisterUserGymCourseCommand(GetUserId(), courseId));
        if (res.IsSuccess) {
            return RedirectToAction(nameof(MyCourse), new { id = res.Value });
        }
        return RedirectToAction(nameof(Index));
    }

    [DisplayName("My Enrolled Course")]
    [Description("View the customized plan for an enrolled course.")]
    public async Task<IActionResult> MyCourse(Guid id) {
        var res = await _getUserGymCourseById.HandleAsync(new GetUserGymCourseByIdQuery(id));
        if (!res.IsSuccess) return NotFound();
        
        return View(res.Value);
    }

    
    [HttpPost]
    [DisplayName("Update Day Layout")]
    [Description("Save modifications to a customized day layout.")]
    public async Task<IActionResult> UpdateDay([FromBody] UpdateDayRequest req) {
        if(req.Day == null) return BadRequest("Day data is missing");
        var cmd = new UpdateUserGymCourseDayCommand(Guid.Parse(User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier)), req.UserGymCourseId, req.WeekNumber, req.Day);
        var res = await _updateUserGymCourseDay.HandleAsync(cmd);
        if (res.IsSuccess) return Ok(new { success = true });
        return BadRequest(new { success = false, message = res.Message });
    }

    public class UpdateDayRequest {
        public Guid UserGymCourseId { get; set; }
        public int WeekNumber { get; set; }
        public AzureTest.Domain.Documents.Gym.CourseDay Day { get; set; }
    }

    [DisplayName("Day Workout View")]
    [Description("View and edit a specific day workout layout.")]
    public async Task<IActionResult> Day(Guid userGymCourseId, int week, Guid dayId) {
        var res = await _getUserGymCourseById.HandleAsync(new GetUserGymCourseByIdQuery(userGymCourseId));
        if (!res.IsSuccess) return NotFound();

        var course = res.Value;
        var courseWeek = course.Weeks.FirstOrDefault(w => w.WeekNumber == week);
        var courseDay = courseWeek?.Days.FirstOrDefault(d => d.Id == dayId);

        if (courseDay == null) return NotFound();

        ViewBag.UserGymCourseId = userGymCourseId;
        ViewBag.WeekNumber = week;
        
        var trainingsRes = await _getAllTrainings.HandleAsync(new AzureTest.Application.Gym.GetAllTrainingsQuery(new TrainingFilter { PageSize = -1 }));
        ViewBag.Trainings = trainingsRes.IsSuccess ? trainingsRes.Value.Items : new List<AzureTest.Domain.Documents.Gym.Training>();

        return View(courseDay);
    }

    
    [DisplayName("Active Session")]
    [Description("Perform an active workout session.")]
    public async Task<IActionResult> Session(Guid userGymCourseId, Guid dayId) {
        var res = await _getUserGymCourseById.HandleAsync(new GetUserGymCourseByIdQuery(userGymCourseId));
        if (!res.IsSuccess) return NotFound();

        var course = res.Value;
        var courseDay = course.Weeks.SelectMany(w => w.Days).FirstOrDefault(d => d.Id == dayId);
        if (courseDay == null) return NotFound();

        var trainingsRes = await _getAllTrainings.HandleAsync(new GetAllTrainingsQuery(new TrainingFilter { PageSize = -1 }));
        var allTrainings = trainingsRes.IsSuccess ? trainingsRes.Value.Items : new List<AzureTest.Domain.Documents.Gym.Training>();

        var sessionsRes = await _getUserSessions.HandleAsync(new GetUserTrainingSessionsQuery(GetUserId()));
        var userLogs = sessionsRes.IsSuccess ? sessionsRes.Value.SelectMany(s => s.Logs).ToList() : new List<AzureTest.Domain.Documents.Gym.TrainingLog>();

        ViewBag.AllTrainings = allTrainings.ToDictionary(t => t.Id);
        ViewBag.Trainings = allTrainings;
        ViewBag.UserLogs = userLogs;
        ViewBag.UserGymCourseId = userGymCourseId;

        return View(courseDay);
    }


    [HttpPost]
    [DisplayName("Finish Session")]
    [Description("Log completed sets, reps, and durations for a session.")]
    public async Task<IActionResult> FinishSession([FromBody] SaveUserTrainingSessionCommand command) {
        var cmdWithUser = command with { UserId = GetUserId() };
        var res = await _saveSession.HandleAsync(cmdWithUser);
        if (res.IsSuccess) {
            return Json(new { success = true, redirectUrl = Url.Action(nameof(MyCourse), new { id = command.UserGymCourseId }) });
        }
        return Json(new { success = false, message = res.Error });
    }
}
