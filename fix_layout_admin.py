import re

file = "src/AzureTest.MVC/Views/Shared/_Layout.cshtml"
with open(file, "r") as f:
    content = f.read()

bad_block = """                <li class="pt-4 mt-4 border-t border-slate-800">
                    <p class="px-3 text-xs font-semibold text-slate-500 uppercase tracking-wider mb-2">System</p>
                    @if (_CurrentUser.Roles.Contains("Admin"))
                    {
                        
                            
                        
                        <a asp-area="" asp-controller="User" asp-action="Index" data-title="User Management" class="nav-item-link flex items-center px-3 py-2.5 rounded-lg transition-colors group @GetActiveClasses("User")">
                            <i class="fa-solid fa-users w-6 @GetActiveIconClasses("User")"></i>
                            <span class="ml-2 font-medium sidebar-text overflow-hidden whitespace-nowrap transition-all duration-300">User Management</span>
                        </a>
                        <a asp-area="" asp-controller="Role" asp-action="Index" data-title="Roles & Permissions" class="nav-item-link flex items-center px-3 py-2.5 rounded-lg transition-colors group @GetActiveClasses("Role")">
                            <i class="fa-solid fa-user-shield w-6 @GetActiveIconClasses("Role")"></i>
                            <span class="ml-2 font-medium sidebar-text overflow-hidden whitespace-nowrap transition-all duration-300">Roles & Permissions</span>
                        </a>
                        <a asp-area="" asp-controller="Permission" asp-action="Index" class="flex items-center px-3 py-2.5 rounded-lg transition-colors group @GetActiveClasses("Permission")">
                            <i class="fa-solid fa-user-lock w-6 @GetActiveIconClasses("Permission")"></i>
                            <span class="ml-2 font-medium">Permissions</span>
                        </a>
                        <a asp-area="" asp-controller="Component" asp-action="Index" data-title="Dashboard Components" class="nav-item-link flex items-center px-3 py-2.5 rounded-lg transition-colors group @GetActiveClasses("Component")">
                            <i class="fa-solid fa-cubes w-6 @GetActiveIconClasses("Component")"></i>
                            <span class="ml-2 font-medium sidebar-text overflow-hidden whitespace-nowrap transition-all duration-300">Dashboard Components</span>
                        </a>
                    }
                    <a href="/healthchecks-ui" target="_blank" class="flex items-center px-3 py-2.5 text-slate-300 hover:bg-slate-800 hover:text-white rounded-lg transition-colors group">
                        <i class="fa-solid fa-heart-pulse w-6 text-slate-400 group-hover:text-white"></i>
                        <span class="ml-2 font-medium sidebar-text overflow-hidden whitespace-nowrap transition-all duration-300">Health Status</span>
                    </a>
                    <a href="/hangfire" target="_blank" class="flex items-center px-3 py-2.5 text-slate-300 hover:bg-slate-800 hover:text-white rounded-lg transition-colors group">
                        <i class="fa-solid fa-server w-6 text-slate-400 group-hover:text-white"></i>
                        <span class="ml-2 font-medium sidebar-text overflow-hidden whitespace-nowrap transition-all duration-300">Background Jobs</span>
                    </a>
                </li>"""

good_block = """                @{
                    bool isAdmin = _CurrentUser.Roles.Contains("Admin");
                    bool canViewUser = isAdmin || _CurrentUser.Permissions.Contains("User/Index");
                    bool canViewRole = isAdmin || _CurrentUser.Permissions.Contains("Role/Index");
                    bool canViewPermission = isAdmin || _CurrentUser.Permissions.Contains("Permission/Index");
                    bool canViewComponent = isAdmin || _CurrentUser.Permissions.Contains("Component/Index");
                    bool hasSystemAccess = canViewUser || canViewRole || canViewPermission || canViewComponent || isAdmin;
                }
                @if (hasSystemAccess)
                {
                    <li class="pt-4 mt-4 border-t border-slate-800">
                        <p class="px-3 text-xs font-semibold text-slate-500 uppercase tracking-wider mb-2">System</p>
                        
                        @if (canViewUser)
                        {
                            <a asp-area="" asp-controller="User" asp-action="Index" data-title="User Management" class="nav-item-link flex items-center px-3 py-2.5 rounded-lg transition-colors group @GetActiveClasses("User")">
                                <i class="fa-solid fa-users w-6 @GetActiveIconClasses("User")"></i>
                                <span class="ml-2 font-medium sidebar-text overflow-hidden whitespace-nowrap transition-all duration-300">User Management</span>
                            </a>
                        }
                        
                        @if (canViewRole)
                        {
                            <a asp-area="" asp-controller="Role" asp-action="Index" data-title="Roles & Permissions" class="nav-item-link flex items-center px-3 py-2.5 rounded-lg transition-colors group @GetActiveClasses("Role")">
                                <i class="fa-solid fa-user-shield w-6 @GetActiveIconClasses("Role")"></i>
                                <span class="ml-2 font-medium sidebar-text overflow-hidden whitespace-nowrap transition-all duration-300">Roles & Permissions</span>
                            </a>
                        }
                        
                        @if (canViewPermission)
                        {
                            <a asp-area="" asp-controller="Permission" asp-action="Index" class="flex items-center px-3 py-2.5 rounded-lg transition-colors group @GetActiveClasses("Permission")">
                                <i class="fa-solid fa-user-lock w-6 @GetActiveIconClasses("Permission")"></i>
                                <span class="ml-2 font-medium">Permissions</span>
                            </a>
                        }
                        
                        @if (canViewComponent)
                        {
                            <a asp-area="" asp-controller="Component" asp-action="Index" data-title="Dashboard Components" class="nav-item-link flex items-center px-3 py-2.5 rounded-lg transition-colors group @GetActiveClasses("Component")">
                                <i class="fa-solid fa-cubes w-6 @GetActiveIconClasses("Component")"></i>
                                <span class="ml-2 font-medium sidebar-text overflow-hidden whitespace-nowrap transition-all duration-300">Dashboard Components</span>
                            </a>
                        }
                        
                        @if (isAdmin)
                        {
                            <a href="/healthchecks-ui" target="_blank" class="flex items-center px-3 py-2.5 text-slate-300 hover:bg-slate-800 hover:text-white rounded-lg transition-colors group">
                                <i class="fa-solid fa-heart-pulse w-6 text-slate-400 group-hover:text-white"></i>
                                <span class="ml-2 font-medium sidebar-text overflow-hidden whitespace-nowrap transition-all duration-300">Health Status</span>
                            </a>
                            <a href="/hangfire" target="_blank" class="flex items-center px-3 py-2.5 text-slate-300 hover:bg-slate-800 hover:text-white rounded-lg transition-colors group">
                                <i class="fa-solid fa-server w-6 text-slate-400 group-hover:text-white"></i>
                                <span class="ml-2 font-medium sidebar-text overflow-hidden whitespace-nowrap transition-all duration-300">Background Jobs</span>
                            </a>
                        }
                    </li>
                }"""

if bad_block in content:
    content = content.replace(bad_block, good_block)
    with open(file, "w") as f:
        f.write(content)
    print("Replaced successfully")
else:
    print("Could not find the block to replace")
