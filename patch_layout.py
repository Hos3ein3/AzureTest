path = "src/AzureTest.MVC/Views/Shared/_Layout.cshtml"
with open(path, "r") as f: c = f.read()

admin_pages = """
        <li>
            <a asp-area="" asp-controller="Pages" asp-action="Index" class="flex items-center px-3 py-2.5 rounded-lg transition-colors group @GetActiveClasses("Pages")">
                <i class="fa-solid fa-file-lines w-6 text-center @GetActiveIconClasses("Pages")"></i>
                <span class="ml-2 font-medium">Pages (Admin)</span>
            </a>
        </li>
"""
c = c.replace('asp-controller="DashboardComponent"', 'asp-controller="Pages" asp-action="Index" class="flex items-center px-3 py-2.5 rounded-lg transition-colors group @GetActiveClasses("Pages")">\n                <i class="fa-solid fa-file-lines w-6 text-center @GetActiveIconClasses("Pages")"></i>\n                <span class="ml-2 font-medium">Pages</span>\n            </a>\n        </li>\n        <li>\n            <a asp-controller="DashboardComponent"')

user_pages = """
        <li>
            @await Component.InvokeAsync("PagesMenu")
        </li>
"""
# Insert after My Workouts or somewhere in the main menu
c = c.replace('<!-- Gym Area -->', f'{user_pages}\n        <!-- Gym Area -->')

with open(path, "w") as f: f.write(c)
print("Layout patched")
