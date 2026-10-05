import re

file = "src/AzureTest.MVC/Controllers/RawComponentController.cs"
with open(file, "r") as f:
    content = f.read()

content = content.replace(
"""        var command = new CreateRawComponentCommand(
            model.Name,
            model.Type,
            model.HtmlContent,
            model.JsonConfiguration,
            model.Category,
            model.RequiredRole
        );""",
"""        var command = new CreateRawComponentCommand(
            model.Name,
            model.Type,
            model.HtmlContent,
            model.JsonConfiguration,
            model.Category,
            model.RequiredRole,
            model.MinWidth,
            model.MaxWidth,
            model.MinHeight,
            model.MaxHeight,
            model.IsResizable
        );"""
)

content = content.replace(
"""        var command = new UpdateRawComponentCommand(
            model.Id,
            model.Name,
            model.Type,
            model.HtmlContent,
            model.JsonConfiguration,
            model.Category,
            model.RequiredRole
        );""",
"""        var command = new UpdateRawComponentCommand(
            model.Id,
            model.Name,
            model.Type,
            model.HtmlContent,
            model.JsonConfiguration,
            model.Category,
            model.RequiredRole,
            model.MinWidth,
            model.MaxWidth,
            model.MinHeight,
            model.MaxHeight,
            model.IsResizable
        );"""
)

with open(file, "w") as f:
    f.write(content)
