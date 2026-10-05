import re
with open('./src/AzureTest.MVC/Controllers/HomeController.cs', 'r') as f:
    content = f.read()

replacement = """
    [DisplayName("Error")]
    [Description("Execute Error operation for Home.")]
    public IActionResult Error()
    {
        var exceptionHandlerPathFeature = HttpContext.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();
        var exception = exceptionHandlerPathFeature?.Error;
        
        var model = new ErrorViewModel 
        { 
            RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier,
            ExceptionMessage = exception?.Message,
            InnerExceptionMessage = exception?.InnerException?.Message,
            StackTrace = exception?.StackTrace
        };
        return View(model);
    }
"""

content = re.sub(r'\[DisplayName\("Error"\)\].*?public IActionResult Error\(\)\s*\{.*?\}', replacement, content, flags=re.DOTALL)

with open('./src/AzureTest.MVC/Controllers/HomeController.cs', 'w') as f:
    f.write(content)
