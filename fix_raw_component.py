import re

domain_file = "src/AzureTest.Domain/Documents/DashboardRawComponent.cs"
with open(domain_file, "r") as f:
    content = f.read()
content = content.replace("public string RequiredRole { get; set; } = string.Empty;", "public string? RequiredRole { get; set; }")
with open(domain_file, "w") as f:
    f.write(content)

cqrs_file = "src/AzureTest.Application/Dashboards/RawComponents/RawComponentCQRS.cs"
with open(cqrs_file, "r") as f:
    content = f.read()

content = content.replace(
    "string RequiredRole,", 
    "string? RequiredRole,"
)

with open(cqrs_file, "w") as f:
    f.write(content)

