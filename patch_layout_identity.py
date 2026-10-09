import os

file = "src/AzureTest.MVC/Views/Shared/_Layout.cshtml"
with open(file, "r") as f:
    content = f.read()

new_link = """
                        @if (_CurrentUser.HasAccess("IdentitySetting/Edit") || User.IsInRole("Admin"))
                        {
                            <a asp-area="" asp-controller="IdentitySetting" asp-action="Edit" data-title="Identity Settings" class="nav-item-link flex items-center px-3 py-2.5 rounded-lg transition-colors group @GetActiveClasses("IdentitySetting")">
                                <i class="fa-solid fa-user-lock w-6 @GetActiveIconClasses("IdentitySetting")"></i>
                                <span class="ml-2 font-medium sidebar-text overflow-hidden whitespace-nowrap transition-all duration-300">Identity Settings</span>
                            </a>
                        }
"""

target = '                                <span class="ml-2 font-medium sidebar-text overflow-hidden whitespace-nowrap transition-all duration-300">Roles & Permissions</span>\n                            </a>\n                        }'

if target in content and "Identity Settings" not in content:
    content = content.replace(target, target + "\n" + new_link)
    with open(file, "w") as f:
        f.write(content)
    print("Layout patched")
else:
    print("Failed to patch layout or already patched")
