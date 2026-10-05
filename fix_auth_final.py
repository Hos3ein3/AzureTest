import re

file = "src/AzureTest.MVC/Controllers/AuthController.cs"
with open(file, "r") as f:
    content = f.read()

# Remove the broken lines completely and replace with the correct structure
bad_block = """    [HttpPost]
    [DisplayName("Logout")]
    [Description("Execute Logout operation for Auth.")]
        [HttpGet]
    [AllowAnonymous]
    [DisplayName("Access Denied")]
    [Description("Show access denied page.")]
    public IActionResult AccessDenied()
    {
        return View();
    }

    [HttpPost]
    [DisplayName("Logout")]
    [Description("Execute Logout operation for Auth.")]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)"""

good_block = """    [HttpGet]
    [AllowAnonymous]
    [DisplayName("Access Denied")]
    [Description("Show access denied page.")]
    public IActionResult AccessDenied()
    {
        return View();
    }

    [HttpPost]
    [DisplayName("Logout")]
    [Description("Execute Logout operation for Auth.")]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)"""

content = content.replace(bad_block, good_block)

with open(file, "w") as f:
    f.write(content)
