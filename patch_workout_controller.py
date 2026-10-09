import re

path = "src/AzureTest.MVC/Controllers/WorkoutController.cs"
with open(path, "r") as f: c = f.read()

dep_injection = "private readonly RegisterUserGymCourseCommandHandler _registerUserGymCourse;"
if "UpdateUserGymCourseDayCommandHandler" not in c:
    c = c.replace(dep_injection, dep_injection + "\n    private readonly UpdateUserGymCourseDayCommandHandler _updateUserGymCourseDay;")
    c = c.replace("RegisterUserGymCourseCommandHandler registerUserGymCourse,", "RegisterUserGymCourseCommandHandler registerUserGymCourse, UpdateUserGymCourseDayCommandHandler updateUserGymCourseDay,")
    c = c.replace("_registerUserGymCourse = registerUserGymCourse;", "_registerUserGymCourse = registerUserGymCourse;\n        _updateUserGymCourseDay = updateUserGymCourseDay;")

new_method = """
    [HttpPost]
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
"""

if "public async Task<IActionResult> UpdateDay(" not in c:
    c = c.replace("public async Task<IActionResult> Day(", new_method + "\n    public async Task<IActionResult> Day(")

with open(path, "w") as f: f.write(c)
print("Workout controller patched")
