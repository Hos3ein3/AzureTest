import re

path = "src/AzureTest.MVC/Views/Workout/History.cshtml"
with open(path, "r") as f: c = f.read()

c = c.replace(
    "var log = item.Log as AzureTest.Domain.Documents.Gym.TrainingLog;",
    "var dItem = (dynamic)item;\n                                    var log = dItem.Log as AzureTest.Domain.Documents.Gym.TrainingLog;"
).replace(
    "var date = (DateTime)item.SessionDate;",
    "var date = (DateTime)dItem.SessionDate;"
)

with open(path, "w") as f: f.write(c)
