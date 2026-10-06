import os

file = "src/AzureTest.MVC/Program.cs"
with open(file, "r") as f:
    content = f.read()

if "AddSignalR" not in content:
    content = content.replace(
        "builder.Services.AddProblemDetails();",
        "builder.Services.AddProblemDetails();\nbuilder.Services.AddSignalR();"
    )
    with open(file, "w") as f:
        f.write(content)
print("SignalR Fixed")
