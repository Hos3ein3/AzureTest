import re

for view in ["CreateTraining.cshtml", "EditTraining.cshtml"]:
    path = f"src/AzureTest.MVC/Views/GymAdmin/{view}"
    with open(path, "r") as f: c = f.read()

    equipment_block = """
                    <div>
                        <label class="block text-sm font-medium text-gray-700">Equipments</label>
                        <input name="EquipmentsStr" id="EquipmentsStr" class="mt-1 block w-full border-gray-300 rounded-md shadow-sm" placeholder="e.g. Dumbbell, Barbell" value="@(Model?.Equipments != null ? string.Join(",", Model.Equipments) : "")" />
                    </div>
"""
    c = c.replace('<div>\n                        <label asp-for="BaseCaloriesBurn"', equipment_block + '                    <div>\n                        <label asp-for="BaseCaloriesBurn"')

    if "Tagify" not in c:
        script_block = """
    <link href="https://cdn.jsdelivr.net/npm/@yaireo/tagify/dist/tagify.css" rel="stylesheet" type="text/css" />
    <script src="https://cdn.jsdelivr.net/npm/@yaireo/tagify"></script>
    <script>
        document.addEventListener('DOMContentLoaded', () => {
            var input = document.querySelector('#EquipmentsStr');
            new Tagify(input);
        });
    </script>
"""
        c = c.replace('</script>\n}', f'</script>\n{script_block}}}')

    with open(path, "w") as f: f.write(c)

# Now modify the controller to parse EquipmentsStr
controller_path = "src/AzureTest.MVC/Controllers/GymAdminController.cs"
with open(controller_path, "r") as f: c = f.read()
c = c.replace('public async Task<IActionResult> CreateTraining(Training model)', 'public async Task<IActionResult> CreateTraining(Training model, string EquipmentsStr)')
c = c.replace('public async Task<IActionResult> EditTraining(Training model)', 'public async Task<IActionResult> EditTraining(Training model, string EquipmentsStr)')
c = c.replace('model.Equipments ?? new()', 'ParseTags(EquipmentsStr)')

# Add ParseTags method if it doesn't exist
if "ParseTags" not in c:
    c = c.replace('public IActionResult Index', '''
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

    public IActionResult Index''')

with open(controller_path, "w") as f: f.write(c)
print("Fixed training views and controller")
