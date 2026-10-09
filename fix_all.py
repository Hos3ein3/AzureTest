import re

# 1. Fix Session.cshtml
path = "src/AzureTest.MVC/Views/Workout/Session.cshtml"
with open(path, "r") as f: c = f.read()
c = c.replace("${l.setsCompleted}x${l.repsCompleted} @ ${l.weightUsed}kg", "${l.setsCompleted}x${l.repsCompleted} @@ ${l.weightUsed}kg")
with open(path, "w") as f: f.write(c)

# 2. Fix WorkoutController
path = "src/AzureTest.MVC/Controllers/WorkoutController.cs"
with open(path, "r") as f: c = f.read()
if "using System.Security.Claims;" not in c:
    c = "using System.Security.Claims;\n" + c
with open(path, "w") as f: f.write(c)

# 3. Fix GymAdminController
path = "src/AzureTest.MVC/Controllers/GymAdminController.cs"
with open(path, "r") as f: c = f.read()
if "private List<string> ParseTags" not in c:
    # insert inside the class at the end
    c = c.replace("}", """
    private List<string> ParseTags(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return new List<string>();
        try {
            var tags = System.Text.Json.JsonSerializer.Deserialize<List<Dictionary<string, string>>>(input);
            return tags?.Select(t => t["value"]).ToList() ?? new List<string>();
        } catch {
            return input.Split(',').Select(x => x.Trim()).ToList();
        }
    }
}""")

# edit training signature
if "string EquipmentsStr" not in c.split("EditTraining(Training model")[1][:50]:
    c = c.replace("public async Task<IActionResult> EditTraining(Training model)", "public async Task<IActionResult> EditTraining(Training model, string EquipmentsStr)")
    c = c.replace("model.BaseCaloriesBurn ?? 0)", "model.BaseCaloriesBurn ?? 0, ParseTags(EquipmentsStr))")

with open(path, "w") as f: f.write(c)

# 4. Fix @yaireo
for view in ["CreateTraining.cshtml", "EditTraining.cshtml"]:
    path = f"src/AzureTest.MVC/Views/GymAdmin/{view}"
    with open(path, "r") as f: c = f.read()
    c = c.replace("@yaireo", "@@yaireo")
    with open(path, "w") as f: f.write(c)

print("Fixed all errors")
