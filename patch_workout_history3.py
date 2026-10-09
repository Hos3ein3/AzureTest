import re

path = "src/AzureTest.MVC/Controllers/WorkoutController.cs"
with open(path, "r") as f: c = f.read()

# Add the class at the end of the namespace but before the end of the file if needed, or just outside the controller.
class_def = "\npublic class HistoryLogItem { public TrainingLog Log { get; set; } public DateTime SessionDate { get; set; } }\n"
if "class HistoryLogItem" not in c:
    c = c.replace("public class WorkoutController : Controller {", class_def + "public class WorkoutController : Controller {")

# Fix the Select to use the class
c = c.replace(
    "s.Logs.Select(l => (object)new {",
    "s.Logs.Select(l => new HistoryLogItem {"
).replace(
    "s.Logs.Select(l => new {",
    "s.Logs.Select(l => new HistoryLogItem {"
)

with open(path, "w") as f: f.write(c)

path2 = "src/AzureTest.MVC/Views/Workout/History.cshtml"
with open(path2, "r") as f: c2 = f.read()
c2 = c2.replace("@model Dictionary<Guid, List<object>>", "@model Dictionary<Guid, List<AzureTest.MVC.Controllers.HistoryLogItem>>")
c2 = c2.replace("var dItem = (dynamic)item;", "")
c2 = c2.replace("var log = dItem.Log as AzureTest.Domain.Documents.Gym.TrainingLog;", "var log = item.Log;")
c2 = c2.replace("var date = (DateTime)dItem.SessionDate;", "var date = item.SessionDate;")
with open(path2, "w") as f: f.write(c2)

