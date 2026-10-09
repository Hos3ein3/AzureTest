import re

path = "src/AzureTest.MVC/Controllers/WorkoutController.cs"
with open(path, "r") as f: c = f.read()

history_action = """
    [HttpGet]
    [DisplayName("Workout History")]
    [Description("View logs grouped by training exercise.")]
    public async Task<IActionResult> History()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if(string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId)) return Unauthorized();

        var sessionsRes = await _getUserSessions.HandleAsync(new GetUserTrainingSessionsQuery(userId));
        var sessions = sessionsRes.IsSuccess ? sessionsRes.Value : new List<UserTrainingSession>();

        var allTrainingsRes = await _getAllTrainings.HandleAsync(new GetAllTrainingsQuery());
        var allTrainings = allTrainingsRes.IsSuccess ? allTrainingsRes.Value : new List<Training>();
        var trainingsDict = allTrainings.ToDictionary(t => t.Id, t => t);

        // Flatten all logs and enrich with Date from the Session, then group by TrainingId
        var allLogs = sessions.SelectMany(s => s.Logs.Select(l => new {
            Log = l,
            SessionDate = s.StartTime
        })).ToList();

        var grouped = allLogs.GroupBy(x => x.Log.TrainingId)
                             .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.SessionDate).ToList());

        ViewBag.TrainingsDict = trainingsDict;
        return View(grouped);
    }
"""

if "IActionResult> History()" not in c:
    c = c.replace("public class WorkoutController : Controller {", "public class WorkoutController : Controller {" + history_action)
    with open(path, "w") as f: f.write(c)
    print("WorkoutController patched with History")
else:
    print("History already exists")
