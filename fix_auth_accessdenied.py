import re

file = "src/AzureTest.MVC/Controllers/AuthController.cs"
with open(file, "r") as f:
    content = f.read()

new_method = """    [HttpGet]
    [AllowAnonymous]
    [DisplayName("Access Denied")]
    [Description("Show access denied page.")]
    public IActionResult AccessDenied()
    {
        return View();
    }
"""
content = content.replace("public async Task<IActionResult> Logout", new_method + "\n    [HttpPost]\n    public async Task<IActionResult> Logout")

with open(file, "w") as f:
    f.write(content)
