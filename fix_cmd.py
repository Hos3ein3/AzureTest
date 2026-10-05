import re

file = "src/AzureTest.Application/Dashboards/Components/Commands/ComponentCommandUnits.cs"
with open(file, "r") as f:
    content = f.read()

content = content.replace(
"""public record CreateDashboardComponentCommand(
    string Name,
    string Description,
    string ImagePath,
    string Category,
    string RequiredRole,""",
"""public record CreateDashboardComponentCommand(
    string Name,
    string? Description,
    string? ImagePath,
    string Category,
    string? RequiredRole,""")

content = content.replace(
"""public record UpdateDashboardComponentCommand(
    Guid Id,
    string Name,
    string Description,
    string ImagePath,
    string Category,
    string RequiredRole,""",
"""public record UpdateDashboardComponentCommand(
    Guid Id,
    string Name,
    string? Description,
    string? ImagePath,
    string Category,
    string? RequiredRole,""")

with open(file, "w") as f:
    f.write(content)
