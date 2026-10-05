import re

file = "src/AzureTest.Application/Dashboards/RawComponents/RawComponentCQRS.cs"
with open(file, "r") as f:
    content = f.read()

# Fix Create
old_create = """            HtmlContent = command.HtmlContent,
            JsonConfiguration = command.JsonConfiguration,
            Category = command.Category,"""
new_create = """            HtmlContent = command.HtmlContent,
            JsonConfiguration = command.JsonConfiguration,
            ViewName = command.ViewName,
            Category = command.Category,"""
content = content.replace(old_create, new_create)

# Fix Update
old_update = """        component.HtmlContent = command.HtmlContent;
        component.JsonConfiguration = command.JsonConfiguration;
        component.Category = command.Category;"""
new_update = """        component.HtmlContent = command.HtmlContent;
        component.JsonConfiguration = command.JsonConfiguration;
        component.ViewName = command.ViewName;
        component.Category = command.Category;"""
content = content.replace(old_update, new_update)

with open(file, "w") as f:
    f.write(content)
