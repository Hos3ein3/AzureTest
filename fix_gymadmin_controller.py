path = "src/AzureTest.MVC/Controllers/GymAdminController.cs"
with open(path, "r") as f: c = f.read()

if "private List<string> ParseTags" not in c:
    # We will replace the last } with the method and then the }
    last_brace_index = c.rfind("}")
    if last_brace_index != -1:
        c = c[:last_brace_index] + """
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
}""" + c[last_brace_index+1:]

c = c.replace("public async Task<IActionResult> EditTraining(Guid id, Training model)", "public async Task<IActionResult> EditTraining(Guid id, Training model, string EquipmentsStr)")
c = c.replace("model.BaseCaloriesBurn ?? 0, model.Equipments ?? new());", "model.BaseCaloriesBurn ?? 0, ParseTags(EquipmentsStr));")

with open(path, "w") as f: f.write(c)

# Fix @yaireo
for view in ["CreateTraining.cshtml", "EditTraining.cshtml"]:
    p = f"src/AzureTest.MVC/Views/GymAdmin/{view}"
    with open(p, "r") as f: cv = f.read()
    cv = cv.replace("@yaireo", "@@yaireo")
    with open(p, "w") as f: f.write(cv)

print("Fixed")
