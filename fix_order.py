import os

file = "src/AzureTest.MVC/Program.cs"
with open(file, "r") as f:
    content = f.read()

content = content.replace("app.UseStaticFiles();\n", "")
content = content.replace("app.UseRouting();", "app.UseStaticFiles();\napp.UseRouting();")

with open(file, "w") as f:
    f.write(content)
print("Order fixed")
