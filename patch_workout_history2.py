import re

path = "src/AzureTest.MVC/Controllers/WorkoutController.cs"
with open(path, "r") as f: c = f.read()

c = c.replace(
    "s.Logs.Select(l => new {",
    "s.Logs.Select(l => (object)new {"
)

with open(path, "w") as f: f.write(c)
