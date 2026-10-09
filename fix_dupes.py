import re

# Fix GymAdminController duplicates - remove the auto-added ones that clash with existing ones
path = "src/AzureTest.MVC/Controllers/GymAdminController.cs"
with open(path, "r") as f: c = f.read()

# The problem is that there are blocks like:
# [DisplayName("Trainings Index")]  <-- existing
# [Description("List all base trainings.")]  <-- existing
# [DisplayName("Action: Index")]  <-- inserted by script
# [Description("Action: Index")]  <-- inserted by script
# public async Task<IActionResult> Index(

# Remove the duplicated pattern: if two [DisplayName] in a row exist
# Strategy: find every occurrence of [DisplayName(...)] right after existing [DisplayName] within 3 lines
import re

# Remove lines that are [DisplayName("Action: X")] right before a public IActionResult
c = re.sub(r'\[DisplayName\("Action: [^"]+"\)\]\s*\n\s*\[Description\("Action: [^"]+"\)\]\s*\n\s*public ', 'public ', c)

with open(path, "w") as f: f.write(c)
print("Fixed duplicates in GymAdminController")
