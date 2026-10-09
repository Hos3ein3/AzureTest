import re

path = "src/AzureTest.MVC/Controllers/WorkoutController.cs"
with open(path, "r") as f: c = f.read()

new_day = """
    public async Task<IActionResult> Day(Guid userGymCourseId, int week, Guid dayId) {
        var res = await _getUserGymCourseById.HandleAsync(new GetUserGymCourseByIdQuery(userGymCourseId));
        if (!res.IsSuccess) return NotFound();

        var course = res.Value;
        var courseWeek = course.Weeks.FirstOrDefault(w => w.WeekNumber == week);
        var courseDay = courseWeek?.Days.FirstOrDefault(d => d.Id == dayId);

        if (courseDay == null) return NotFound();

        var trainingsRes = await _getAllTrainings.HandleAsync(new AzureTest.Application.Gym.GetAllTrainingsQuery());
        ViewBag.Trainings = trainingsRes.IsSuccess ? trainingsRes.Value : new List<AzureTest.Domain.Documents.Gym.Training>();

        ViewBag.UserGymCourseId = userGymCourseId;
        ViewBag.WeekNumber = week;
        
        return View(courseDay);
    }
"""

new_session = """
    public async Task<IActionResult> Session(Guid userGymCourseId, Guid dayId) {
        var res = await _getUserGymCourseById.HandleAsync(new GetUserGymCourseByIdQuery(userGymCourseId));
        if (!res.IsSuccess) return NotFound();

        var course = res.Value;
        var courseDay = course.Weeks.SelectMany(w => w.Days).FirstOrDefault(d => d.Id == dayId);
        if (courseDay == null) return NotFound();

        var trainingsRes = await _getAllTrainings.HandleAsync(new GetAllTrainingsQuery());
        var allTrainings = trainingsRes.IsSuccess ? trainingsRes.Value : new List<AzureTest.Domain.Documents.Gym.Training>();

        var sessionsRes = await _getUserSessions.HandleAsync(new GetUserTrainingSessionsQuery(GetUserId()));
        var userLogs = sessionsRes.IsSuccess ? sessionsRes.Value.SelectMany(s => s.Logs).ToList() : new List<AzureTest.Domain.Documents.Gym.TrainingLog>();

        ViewBag.AllTrainings = allTrainings.ToDictionary(t => t.Id);
        ViewBag.Trainings = allTrainings;
        ViewBag.UserLogs = userLogs;
        ViewBag.UserGymCourseId = userGymCourseId;

        return View(courseDay);
    }
"""

# replace between Day( and UpdateDay(
c = re.sub(r'public async Task<IActionResult> Day\(.*?\[HttpPost\]\s+public async Task<IActionResult> UpdateDay', new_day + '\n    [HttpPost]\n    public async Task<IActionResult> UpdateDay', c, flags=re.DOTALL)

# replace Session
c = re.sub(r'public async Task<IActionResult> Session\(.*?return View\(courseDay\);\s*\}', new_session, c, flags=re.DOTALL)

with open(path, "w") as f: f.write(c)

