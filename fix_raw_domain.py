import re

file = "src/AzureTest.Domain/Documents/DashboardRawComponent.cs"
with open(file, "r") as f:
    content = f.read()

content = content.replace(
    "public string? JsonConfiguration { get; set; }",
    "public string? JsonConfiguration { get; set; }\n    public string? ViewName { get; set; }"
)

with open(file, "w") as f:
    f.write(content)
