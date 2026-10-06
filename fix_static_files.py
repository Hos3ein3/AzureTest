import os

file = "src/AzureTest.MVC/Program.cs"
with open(file, "r") as f:
    content = f.read()

if "app.UseStaticFiles()" not in content:
    content = content.replace("app.MapStaticAssets();", "app.UseStaticFiles();\napp.MapStaticAssets();")
    with open(file, "w") as f:
        f.write(content)
print("Static Files Fixed")
