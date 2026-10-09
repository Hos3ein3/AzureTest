import re

controllers = [
    ("src/AzureTest.MVC/Controllers/GymAdminController.cs", "Gym Administration", "Manage global gym courses, trainings, and templates."),
    ("src/AzureTest.MVC/Controllers/IdentitySettingController.cs", "Identity Settings", "Manage authentication and identity configurations."),
    ("src/AzureTest.MVC/Controllers/PagesController.cs", "Page Management", "Manage application pages and user comments.")
]

for path, dname, ddesc in controllers:
    try:
        with open(path, "r") as f: c = f.read()
    except FileNotFoundError:
        continue
    
    if "using System.ComponentModel;" not in c:
        c = "using System.ComponentModel;\n" + c
    
    # Class attribute
    if '[DisplayName("' not in c.split("public class")[0]:
        c = c.replace(f"public class", f'[DisplayName("{dname}")]\n[Description("{ddesc}")]\npublic class', 1)
    
    # Actions (methods returning IActionResult or Task<IActionResult>)
    def replacer(match):
        method_sig = match.group(0)
        if "[DisplayName" in method_sig:
            return method_sig
        
        # Simple extraction of action name
        # public async Task<IActionResult> Index(...)
        m = re.search(r'(IActionResult>|IActionResult)\s+([A-Za-z0-9_]+)\(', method_sig)
        if m:
            action_name = m.group(2)
            # Camel case split
            display = re.sub("([a-z])([A-Z])","\g<1> \g<2>", action_name)
            attr = f'[DisplayName("{display}")]\n    [Description("Action: {display}")]\n    '
            return method_sig.replace("public ", attr + "public ")
        return method_sig

    # Find all public methods returning IActionResult
    c = re.sub(r'public\s+(async\s+)?(Task<IActionResult>|IActionResult)\s+[A-Za-z0-9_]+\s*\(', replacer, c)
    
    with open(path, "w") as f: f.write(c)

print("Attributes patched.")
