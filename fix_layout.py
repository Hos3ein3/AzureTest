import re

layout_file = "src/AzureTest.MVC/Views/Shared/_Layout.cshtml"
with open(layout_file, "r") as f:
    content = f.read()

if "HouseCrawler" not in content:
    new_link = """                <li>
                    <a asp-area="" asp-controller="HouseCrawler" asp-action="Index" data-title="House Crawler" class="nav-item-link flex items-center px-3 py-2.5 rounded-lg transition-colors group @GetActiveClasses("HouseCrawler")">
                        <i class="fa-solid fa-house-chimney w-6 @GetActiveIconClasses("HouseCrawler")"></i>
                        <span class="ml-2 font-medium sidebar-text overflow-hidden whitespace-nowrap transition-all duration-300">House Crawler</span>
                    </a>
                </li>"""
    
    content = content.replace(
        """                <li>
                    <a asp-area="" asp-controller="Company" asp-action="Index" data-title="Companies" class="nav-item-link flex items-center px-3 py-2.5 rounded-lg transition-colors group @GetActiveClasses("Company")">""",
        new_link + """\n                <li>
                    <a asp-area="" asp-controller="Company" asp-action="Index" data-title="Companies" class="nav-item-link flex items-center px-3 py-2.5 rounded-lg transition-colors group @GetActiveClasses("Company")">"""
    )
    with open(layout_file, "w") as f:
        f.write(content)
