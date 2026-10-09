import re

path = "src/AzureTest.MVC/Controllers/WorkoutController.cs"
with open(path, "r") as f: c = f.read()

# Add missing usings
if "using System.ComponentModel;" not in c:
    c = "using System.ComponentModel;\n" + c

# Add controller attribute
c = c.replace("public class WorkoutController : Controller", '[DisplayName("Workout Management")]\n[Description("Allows users to enroll in courses, customize days, and perform sessions.")]\npublic class WorkoutController : Controller')

# Inject GetTrainingCourseByIdQueryHandler
c = c.replace("private readonly GetAllTrainingCoursesQueryHandler _getAllCourses;", "private readonly GetAllTrainingCoursesQueryHandler _getAllCourses;\n    private readonly GetTrainingCourseByIdQueryHandler _getCourseById;")
c = c.replace("GetAllTrainingCoursesQueryHandler getAllCourses,", "GetAllTrainingCoursesQueryHandler getAllCourses, GetTrainingCourseByIdQueryHandler getCourseById,")
c = c.replace("_getAllCourses = getAllCourses;", "_getAllCourses = getAllCourses;\n        _getCourseById = getCourseById;")

# Add Details Action
details_action = """
    [DisplayName("Course Details")]
    [Description("View details of a global training course before enrolling.")]
    public async Task<IActionResult> Details(Guid id) {
        var res = await _getCourseById.HandleAsync(new GetTrainingCourseByIdQuery(id));
        if (!res.IsSuccess) return NotFound();

        var trainingsRes = await _getAllTrainings.HandleAsync(new GetAllTrainingsQuery());
        ViewBag.Trainings = trainingsRes.IsSuccess ? trainingsRes.Value.ToDictionary(t => t.Id) : new Dictionary<Guid, AzureTest.Domain.Documents.Gym.Training>();

        return View(res.Value);
    }
"""
if "public async Task<IActionResult> Details" not in c:
    c = c.replace("public async Task<IActionResult> Index()", details_action + "\n    [DisplayName(\"Workout Dashboard\")]\n    [Description(\"View enrolled courses and available global courses.\")]\n    public async Task<IActionResult> Index()")

# Add attributes to other actions
c = c.replace("public async Task<IActionResult> Enroll(Guid id)", "[DisplayName(\"Enroll Course\")]\n    [Description(\"Enroll the user into a specific gym course.\")]\n    public async Task<IActionResult> Enroll(Guid id)")
c = c.replace("public async Task<IActionResult> MyCourse(Guid id)", "[DisplayName(\"My Enrolled Course\")]\n    [Description(\"View the customized plan for an enrolled course.\")]\n    public async Task<IActionResult> MyCourse(Guid id)")
c = c.replace("public async Task<IActionResult> Day(Guid userGymCourseId", "[DisplayName(\"Day Workout View\")]\n    [Description(\"View and edit a specific day workout layout.\")]\n    public async Task<IActionResult> Day(Guid userGymCourseId")
c = c.replace("public async Task<IActionResult> UpdateDay([FromBody]", "[DisplayName(\"Update Day Layout\")]\n    [Description(\"Save modifications to a customized day layout.\")]\n    public async Task<IActionResult> UpdateDay([FromBody]")
c = c.replace("public async Task<IActionResult> Session(Guid userGymCourseId", "[DisplayName(\"Active Session\")]\n    [Description(\"Perform an active workout session.\")]\n    public async Task<IActionResult> Session(Guid userGymCourseId")
c = c.replace("public async Task<IActionResult> FinishSession([FromBody]", "[DisplayName(\"Finish Session\")]\n    [Description(\"Log completed sets, reps, and durations for a session.\")]\n    public async Task<IActionResult> FinishSession([FromBody]")

with open(path, "w") as f: f.write(c)

print("WorkoutController patched.")
