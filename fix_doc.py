import re

file = "src/AzureTest.Domain/Documents/DashboardComponent.cs"
with open(file, "r") as f:
    content = f.read()

content = content.replace("public string Description { get; set; } = string.Empty;", "public string? Description { get; set; }")
content = content.replace("public string ImagePath { get; set; } = string.Empty;", "public string? ImagePath { get; set; }")
content = content.replace("public string RequiredRole { get; set; } = string.Empty;", "public string? RequiredRole { get; set; }")

with open(file, "w") as f:
    f.write(content)
