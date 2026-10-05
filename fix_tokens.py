import re

file1 = "src/AzureTest.MVC/Views/Error/Error.cshtml"
with open(file1, "r") as f:
    content1 = f.read()
content1 = content1.replace('<form asp-controller="Ticket" asp-action="Create" method="post" class="inline">', '<form asp-controller="Ticket" asp-action="Create" method="post" class="inline">\n            @Html.AntiForgeryToken()')
with open(file1, "w") as f:
    f.write(content1)

file2 = "src/AzureTest.MVC/Views/Auth/AccessDenied.cshtml"
with open(file2, "r") as f:
    content2 = f.read()
content2 = content2.replace('<form asp-controller="Ticket" asp-action="Create" method="post">', '<form asp-controller="Ticket" asp-action="Create" method="post">\n                @Html.AntiForgeryToken()')
with open(file2, "w") as f:
    f.write(content2)

