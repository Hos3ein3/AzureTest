import re

file = "src/AzureTest.MVC/Controllers/AuthController.cs"
with open(file, "r") as f:
    content = f.read()

content = content.replace("""    [DisplayName("Logout")]
    [Description("Execute Logout operation for Auth.")]
    [HttpGet]
    [AllowAnonymous]
    [DisplayName("Access Denied")]
    [Description("Show access denied page.")]
    public IActionResult AccessDenied()""", """    [HttpGet]
    [AllowAnonymous]
    [DisplayName("Access Denied")]
    [Description("Show access denied page.")]
    public IActionResult AccessDenied()""")

content = content.replace("""    [HttpPost]
    public async Task<IActionResult> Logout""", """    [HttpPost]
    [DisplayName("Logout")]
    [Description("Execute Logout operation for Auth.")]
    public async Task<IActionResult> Logout""")

with open(file, "w") as f:
    f.write(content)
