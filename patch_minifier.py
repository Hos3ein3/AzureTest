import re

with open('/Users/mohammadhosein/Development/AzureTest/src/AzureTest.MVC/Program.cs', 'r') as f:
    content = f.read()

# Add usings at the top if they don't exist
if 'using WebMarkupMin.AspNetCore8;' not in content:
    content = "using WebMarkupMin.AspNetCore8;\nusing WebMarkupMin.Core;\n" + content

# 1. Add services block for Minification
services_block = """
// --- HTML, CSS, JS Minification (Only enabled in Production) ---
if (builder.Environment.IsProduction())
{
    // WebOptimizer for CSS & JS minification
    builder.Services.AddWebOptimizer(pipeline =>
    {
        pipeline.MinifyJsFiles("js/**/*.js");
        pipeline.MinifyCssFiles("css/**/*.css");
    });

    // WebMarkupMin for HTML minification
    builder.Services.AddWebMarkupMin(options =>
    {
        options.AllowMinificationInDevelopmentEnvironment = false;
        options.AllowCompressionInDevelopmentEnvironment = false;
    })
    .AddHtmlMinification(options =>
    {
        options.MinificationSettings.RemoveRedundantAttributes = true;
        options.MinificationSettings.RemoveHttpProtocolFromAttributes = true;
        options.MinificationSettings.RemoveHttpsProtocolFromAttributes = true;
        options.MinificationSettings.RemoveHtmlComments = true;
    })
    .AddHttpCompression();
}
"""

if 'AddWebOptimizer' not in content:
    content = content.replace("builder.Services.AddProblemDetails();", "builder.Services.AddProblemDetails();\n" + services_block)

# 2. Add middleware pipeline block
middleware_block = """
if (app.Environment.IsProduction())
{
    app.UseWebOptimizer();
    app.UseWebMarkupMin();
}
"""

if 'UseWebOptimizer' not in content:
    content = content.replace("app.UseRouting();", middleware_block + "\napp.UseRouting();")

with open('/Users/mohammadhosein/Development/AzureTest/src/AzureTest.MVC/Program.cs', 'w') as f:
    f.write(content)
