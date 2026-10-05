import re

file = "src/AzureTest.MVC/Controllers/RawComponentController.cs"
with open(file, "r") as f:
    content = f.read()

# Fix Create
old_create = """        var command = new CreateRawComponentCommand(
            model.Name,
            model.Type,
            model.HtmlContent,
            model.JsonConfiguration,
            model.ViewName,
            model.Category,
            model.RequiredRole ?? "",
            model.MinWidth,
            model.MaxWidth,
            model.MinHeight,
            model.MaxHeight,
            model.IsResizable
        );"""
new_create = """        var command = new CreateRawComponentCommand(
            model.Name,
            model.Type,
            model.HtmlContent,
            model.JsonConfiguration,
            model.ViewName,
            model.Category,
            model.RequiredRole ?? "",
            model.MinWidth,
            model.MaxWidth,
            model.MinHeight,
            model.MaxHeight,
            model.IsResizable
        );"""

# Wait, the error is because I previously replaced:
#         var command = new CreateRawComponentCommand(
#             model.Name,
#             model.Type,
#             model.HtmlContent,
#             model.JsonConfiguration,
#             model.ViewName,
#             model.Category,
#             model.RequiredRole ?? "",
#             model.MinWidth,

# Let's just manually replace the exact string from the file
