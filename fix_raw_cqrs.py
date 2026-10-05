import re

file = "src/AzureTest.Application/Dashboards/RawComponents/RawComponentCQRS.cs"
with open(file, "r") as f:
    content = f.read()

# Update Create command
content = content.replace(
"""public record CreateRawComponentCommand(
    string Name,
    string Type,
    string? HtmlContent,
    string? JsonConfiguration,
    string? Category,
    string RequiredRole,""",
"""public record CreateRawComponentCommand(
    string Name,
    string Type,
    string? HtmlContent,
    string? JsonConfiguration,
    string? ViewName,
    string? Category,
    string RequiredRole,"""
)

# Update Create handler
content = content.replace(
"""        var component = new DashboardRawComponent
        {
            Name = command.Name,
            Type = command.Type,
            HtmlContent = command.HtmlContent,
            JsonConfiguration = command.JsonConfiguration,
            Category = command.Category,
            RequiredRole = command.RequiredRole,""",
"""        var component = new DashboardRawComponent
        {
            Name = command.Name,
            Type = command.Type,
            HtmlContent = command.HtmlContent,
            JsonConfiguration = command.JsonConfiguration,
            ViewName = command.ViewName,
            Category = command.Category,
            RequiredRole = command.RequiredRole,"""
)

# Update Update command
content = content.replace(
"""public record UpdateRawComponentCommand(
    Guid Id,
    string Name,
    string Type,
    string? HtmlContent,
    string? JsonConfiguration,
    string? Category,
    string RequiredRole,""",
"""public record UpdateRawComponentCommand(
    Guid Id,
    string Name,
    string Type,
    string? HtmlContent,
    string? JsonConfiguration,
    string? ViewName,
    string? Category,
    string RequiredRole,"""
)

# Update Update handler
content = content.replace(
"""        component.Name = command.Name;
        component.Type = command.Type;
        component.HtmlContent = command.HtmlContent;
        component.JsonConfiguration = command.JsonConfiguration;
        component.Category = command.Category;
        component.RequiredRole = command.RequiredRole;""",
"""        component.Name = command.Name;
        component.Type = command.Type;
        component.HtmlContent = command.HtmlContent;
        component.JsonConfiguration = command.JsonConfiguration;
        component.ViewName = command.ViewName;
        component.Category = command.Category;
        component.RequiredRole = command.RequiredRole;"""
)

with open(file, "w") as f:
    f.write(content)
