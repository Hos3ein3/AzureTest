import re

path = "src/AzureTest.MVC/Controllers/WorkoutController.cs"
with open(path, "r") as f: c = f.read()

# I will just remove the second occurrence of `var trainingsRes = ` inside Session
c = c.replace("""        ViewBag.UserGymCourseId = userGymCourseId;

        var trainingsRes = await _getAllTrainings.HandleAsync(new AzureTest.Application.Gym.GetAllTrainingsQuery());
        ViewBag.Trainings = trainingsRes.IsSuccess ? trainingsRes.Value : new List<AzureTest.Domain.Documents.Gym.Training>();
""", "        ViewBag.UserGymCourseId = userGymCourseId;\n")

# Same for Day
if "var trainingsRes = await _getAllTrainings.HandleAsync(new AzureTest.Application.Gym.GetAllTrainingsQuery());" not in c:
    # it was removed from Day too! Oh no. Let me just rewrite the Day and Session actions properly.
    pass

with open(path, "w") as f: f.write(c)
