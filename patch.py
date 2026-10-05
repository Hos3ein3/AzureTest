file = "src/AzureTest.MVC/Views/Shared/_Layout.cshtml"
with open(file, "r") as f:
    content = f.read()

target = """                        @if (_CurrentUser.HasAccess("Permissions/Index"))
                        {"""

replacement = """                        @if (_CurrentUser.HasAccess("FileExplorer/Index"))
                        {
                            <a asp-area="" asp-controller="FileExplorer" asp-action="Index" data-title="File Explorer" class="nav-item-link flex items-center px-3 py-2.5 rounded-lg transition-colors group @GetActiveClasses("FileExplorer")">
                                <i class="fa-solid fa-folder-tree w-6 @GetActiveIconClasses("FileExplorer")"></i>
                                <span class="ml-2 font-medium sidebar-text overflow-hidden whitespace-nowrap transition-all duration-300">File Explorer</span>
                            </a>
                        }

                        @if (_CurrentUser.HasAccess("Permissions/Index"))
                        {"""

if target in content:
    content = content.replace(target, replacement)
    with open(file, "w") as f:
        f.write(content)
    print("Replaced")
else:
    print("Failed")
