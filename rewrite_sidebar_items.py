import re

path = "src/AzureTest.MVC/Views/Shared/_Sidebar.cshtml"

content = """@using AzureTest.Application.Interfaces.Services
@inject ICurrentUser _CurrentUser
@{
    var currentController = ViewContext.RouteData.Values["controller"]?.ToString();
    
    string GetActiveClasses(string controller)
    {
        return currentController == controller 
            ? "bg-brand-50 text-brand-700" 
            : "text-slate-600 hover:bg-slate-50 hover:text-slate-900";
    }
    
    string GetActiveIconClasses(string controller)
    {
        return currentController == controller 
            ? "text-brand-600" 
            : "text-slate-400 group-hover:text-slate-600";
    }
}

<style>
    /* Mobile Overlay */
    #mobileSidebarOverlay { display: none; }
    #mobileSidebarOverlay.active { display: block; }
    
    /* Desktop Sidebar Animations */
    #sidebar { transition: width 0.3s ease; }
    #sidebar.collapsed { width: 4.5rem; }
    #sidebar.collapsed .sidebar-text { opacity: 0; width: 0; display: none; }
    #sidebar.collapsed .logo-brand { display: none; }
    #sidebar.collapsed .logo-icon { margin-right: 0; font-size: 1.75rem; }
    #sidebar.collapsed .nav-item-link { justify-content: center; padding-left: 0; padding-right: 0; }
    #sidebar.collapsed .nav-item-link i { margin-left: 0; margin-right: 0; }
    
    /* Desktop Tooltip for Collapsed State */
    #sidebar.collapsed .nav-item-link { position: relative; }
    #sidebar.collapsed .nav-item-link:hover::after {
        content: attr(data-title);
        position: absolute;
        left: calc(100% + 8px);
        background-color: #1e293b;
        color: #fff;
        padding: 6px 10px;
        border-radius: 6px;
        font-size: 0.75rem;
        font-weight: 600;
        white-space: nowrap;
        z-index: 50;
        pointer-events: none;
        box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1);
    }
    
    /* Dropdown overrides for collapsed state */
    #sidebar.collapsed details summary .sidebar-text,
    #sidebar.collapsed details summary i.fa-chevron-down {
        display: none;
    }
    #sidebar.collapsed details ul {
        display: none;
    }
</style>

<!-- Mobile Sidebar Backdrop -->
<div id="mobileSidebarOverlay" class="fixed inset-0 bg-slate-900/50 backdrop-blur-sm z-40 md:hidden transition-opacity opacity-0"></div>

<!-- Sidebar Container -->
<aside id="sidebar" class="fixed md:relative inset-y-0 left-0 z-50 w-64 bg-white border-r border-slate-200 text-slate-800 flex flex-col shadow-2xl md:shadow-none transform -translate-x-full md:translate-x-0 transition-transform duration-300 ease-in-out h-full">
    
    <div class="h-16 flex items-center px-5 border-b border-slate-100 flex-shrink-0">
        <div class="flex items-center flex-1 overflow-hidden">
            <i class="fa-solid fa-briefcase text-brand-600 text-2xl mr-3 logo-icon transition-all"></i>
            <span class="logo-brand text-xl font-black tracking-tight text-slate-800 truncate">Tracker<span class="text-brand-600">Pro</span></span>
        </div>
        <button id="sidebarToggleDesktop" class="hidden md:flex text-slate-400 hover:text-brand-600 transition-colors p-2 rounded-lg hover:bg-slate-50 focus:outline-none">
            <i class="fa-solid fa-bars"></i>
        </button>
        <button id="sidebarToggleMobileClose" class="md:hidden text-slate-400 hover:text-slate-600 transition-colors p-2 focus:outline-none">
            <i class="fa-solid fa-xmark text-xl"></i>
        </button>
    </div>
    
    <nav class="flex-1 overflow-y-auto py-4 scrollbar-hide px-3">
        <ul class="space-y-1">
            <li>
                <a asp-area="" asp-controller="Home" asp-action="Index" data-title="Dashboard" class="nav-item-link flex items-center px-3 py-2.5 rounded-lg transition-colors group @GetActiveClasses("Home")">
                    <i class="fa-solid fa-gauge-high w-6 text-center text-lg transition-colors @GetActiveIconClasses("Home")"></i>
                    <span class="ml-2 font-medium sidebar-text whitespace-nowrap transition-all duration-300">Dashboard</span>
                </a>
            </li>
            <li>
                <a asp-area="" asp-controller="Resume" asp-action="Index" data-title="Resumes" class="nav-item-link flex items-center px-3 py-2.5 rounded-lg transition-colors group @GetActiveClasses("Resume")">
                    <i class="fa-solid fa-file-lines w-6 text-center text-lg transition-colors @GetActiveIconClasses("Resume")"></i>
                    <span class="ml-2 font-medium sidebar-text whitespace-nowrap transition-all duration-300">Resumes</span>
                </a>
            </li>
            <li>
                <a asp-area="" asp-controller="IdeaTracker" asp-action="Index" data-title="Idea Tracker" class="nav-item-link flex items-center px-3 py-2.5 rounded-lg transition-colors group @GetActiveClasses("IdeaTracker")">
                    <i class="fa-solid fa-brain w-6 text-center text-lg transition-colors @GetActiveIconClasses("IdeaTracker")"></i>
                    <span class="ml-2 font-medium sidebar-text whitespace-nowrap transition-all duration-300">Idea Tracker</span>
                </a>
            </li>
            <li>
                <a asp-area="" asp-controller="JobCrawler" asp-action="Index" data-title="Job Crawler" class="nav-item-link flex items-center px-3 py-2.5 rounded-lg transition-colors group @GetActiveClasses("JobCrawler")">
                    <i class="fa-solid fa-spider w-6 text-center text-lg transition-colors @GetActiveIconClasses("JobCrawler")"></i>
                    <span class="ml-2 font-medium sidebar-text whitespace-nowrap transition-all duration-300">Job Crawler</span>
                </a>
            </li>
            <li>
                <a asp-area="" asp-controller="HouseCrawler" asp-action="Index" data-title="House Crawler" class="nav-item-link flex items-center px-3 py-2.5 rounded-lg transition-colors group @GetActiveClasses("HouseCrawler")">
                    <i class="fa-solid fa-house-chimney w-6 text-center text-lg transition-colors @GetActiveIconClasses("HouseCrawler")"></i>
                    <span class="ml-2 font-medium sidebar-text whitespace-nowrap transition-all duration-300">House Crawler</span>
                </a>
            </li>
            <li>
                <a asp-area="" asp-controller="Company" asp-action="Index" data-title="Companies" class="nav-item-link flex items-center px-3 py-2.5 rounded-lg transition-colors group @GetActiveClasses("Company")">
                    <i class="fa-solid fa-building w-6 text-center text-lg transition-colors @GetActiveIconClasses("Company")"></i>
                    <span class="ml-2 font-medium sidebar-text whitespace-nowrap transition-all duration-300">Companies</span>
                </a>
            </li>
            <li>
                <a asp-area="" asp-controller="ResumeFile" asp-action="Index" data-title="Resume Files" class="nav-item-link flex items-center px-3 py-2.5 rounded-lg transition-colors group @GetActiveClasses("ResumeFile")">
                    <i class="fa-solid fa-file-pdf w-6 text-center text-lg transition-colors @GetActiveIconClasses("ResumeFile")"></i>
                    <span class="ml-2 font-medium sidebar-text whitespace-nowrap transition-all duration-300">Resume Files</span>
                </a>
            </li>
            <li>
                <a asp-area="" asp-controller="Calendar" asp-action="Index" data-title="Calendar" class="nav-item-link flex items-center px-3 py-2.5 rounded-lg transition-colors group @GetActiveClasses("Calendar")">
                    <i class="fa-solid fa-calendar-days w-6 text-center text-lg transition-colors @GetActiveIconClasses("Calendar")"></i>
                    <span class="ml-2 font-medium sidebar-text whitespace-nowrap transition-all duration-300">Calendar</span>
                </a>
            </li>
            <li>
                <a asp-area="" asp-controller="Task" asp-action="Index" data-title="Tasks" class="nav-item-link flex items-center px-3 py-2.5 rounded-lg transition-colors group @GetActiveClasses("Task")">
                    <i class="fa-solid fa-list-check w-6 text-center text-lg transition-colors @GetActiveIconClasses("Task")"></i>
                    <span class="ml-2 font-medium sidebar-text whitespace-nowrap transition-all duration-300">Tasks</span>
                </a>
            </li>
            <li>
                <a asp-area="" asp-controller="Event" asp-action="Index" data-title="Events" class="nav-item-link flex items-center px-3 py-2.5 rounded-lg transition-colors group @GetActiveClasses("Event")">
                    <i class="fa-solid fa-calendar-plus w-6 text-center text-lg transition-colors @GetActiveIconClasses("Event")"></i>
                    <span class="ml-2 font-medium sidebar-text whitespace-nowrap transition-all duration-300">Events</span>
                </a>
            </li>
            @if (_HostEnv.IsDevelopment())
            {
                <li>
                    <a asp-area="" asp-controller="SampleNote" asp-action="Index" data-title="Sample Notes (Dev)" class="nav-item-link flex items-center px-3 py-2.5 rounded-lg transition-colors group @GetActiveClasses("SampleNote")">
                        <i class="fa-solid fa-flask w-6 text-center text-lg transition-colors @GetActiveIconClasses("SampleNote")"></i>
                        <span class="ml-2 font-medium sidebar-text whitespace-nowrap transition-all duration-300">Sample Notes <span class="text-xs text-brand-500">(Dev)</span></span>
                    </a>
                </li>
            }

            <!-- Gym Section -->
            <li>
                <details class="group [&_summary::-webkit-details-marker]:hidden">
                    <summary class="flex items-center px-3 py-2.5 rounded-lg transition-colors cursor-pointer text-slate-600 hover:bg-slate-50 hover:text-slate-900">
                        <i class="fa-solid fa-dumbbell w-6 text-center text-lg text-slate-400 group-hover:text-slate-600"></i>
                        <span class="ml-2 font-medium flex-1 sidebar-text whitespace-nowrap transition-all duration-300">Gym</span>
                        <i class="fa-solid fa-chevron-down text-xs transition-transform duration-200 group-open:rotate-180"></i>
                    </summary>
                    <ul class="mt-1 ml-6 space-y-1 border-l border-slate-200 pl-3">
                        @if (User.IsInRole("Admin") || _CurrentUser.HasAccess(@Url.Action("GymCourses", "Index", new {})))
                        {
                            <li>
                                <a asp-controller="GymCourses" asp-action="Index" class="flex items-center px-3 py-2 rounded-lg text-sm text-slate-500 hover:text-slate-800 hover:bg-slate-50 transition-colors">
                                    Courses
                                </a>
                            </li>
                        }
                        @if (User.IsInRole("Admin") || _CurrentUser.HasAccess(@Url.Action("GymCourses", "Index", new {})))
                        {
                            <li>
                                <a asp-controller="GymAdmin" asp-action="Index" class="flex items-center px-3 py-2 rounded-lg text-sm text-slate-500 hover:text-slate-800 hover:bg-slate-50 transition-colors">
                                    Trainings
                                </a>
                            </li>
                        }
                        <li>
                            <a asp-controller="Workout" asp-action="Index" class="flex items-center px-3 py-2 rounded-lg text-sm text-slate-500 hover:text-slate-800 hover:bg-slate-50 transition-colors">
                                Workout
                            </a>
                        </li>
                    </ul>
                </details>
            </li>
            
            <li>
                @await Component.InvokeAsync("PagesMenu")
            </li>

            <li>
                <a asp-area="" asp-controller="Habit" asp-action="Index" data-title="Habits" class="nav-item-link flex items-center px-3 py-2.5 rounded-lg transition-colors group @GetActiveClasses("Habit")">
                    <i class="fa-solid fa-seedling w-6 text-center text-lg transition-colors @GetActiveIconClasses("Habit")"></i>
                    <span class="ml-2 font-medium sidebar-text whitespace-nowrap transition-all duration-300">Habits</span>
                </a>
            </li>
            <li>
                <a asp-area="" asp-controller="Ticket" asp-action="Index" data-title="Support Tickets" class="nav-item-link flex items-center px-3 py-2.5 rounded-lg transition-colors group @GetActiveClasses("Ticket")">
                    <i class="fa-solid fa-ticket w-6 text-center text-lg transition-colors @GetActiveIconClasses("Ticket")"></i>
                    <span class="ml-2 font-medium sidebar-text whitespace-nowrap transition-all duration-300">Support Tickets</span>
                </a>
            </li>

            <!-- Financials Section -->
            <li>
                <details class="group [&_summary::-webkit-details-marker]:hidden">
                    <summary class="flex items-center px-3 py-2.5 rounded-lg transition-colors cursor-pointer text-slate-600 hover:bg-slate-50 hover:text-slate-900">
                        <i class="fa-solid fa-wallet w-6 text-center text-lg text-slate-400 group-hover:text-slate-600"></i>
                        <span class="ml-2 font-medium flex-1 sidebar-text whitespace-nowrap transition-all duration-300">Financials</span>
                        <i class="fa-solid fa-chevron-down text-xs transition-transform duration-200 group-open:rotate-180"></i>
                    </summary>
                    <ul class="mt-1 ml-6 space-y-1 border-l border-slate-200 pl-3">
                        <li>
                            <a asp-controller="Expense" asp-action="Index" class="flex items-center px-3 py-2 rounded-lg text-sm text-slate-500 hover:text-slate-800 hover:bg-slate-50 transition-colors">
                                Expenses
                            </a>
                        </li>
                        <li>
                            <a asp-controller="ExpenseCategory" asp-action="Index" class="flex items-center px-3 py-2 rounded-lg text-sm text-slate-500 hover:text-slate-800 hover:bg-slate-50 transition-colors">
                                Categories
                            </a>
                        </li>
                        <li>
                            <a asp-controller="Debt" asp-action="Index" class="flex items-center px-3 py-2 rounded-lg text-sm text-slate-500 hover:text-slate-800 hover:bg-slate-50 transition-colors">
                                Debts
                            </a>
                        </li>
                        <li>
                            <a asp-controller="DebtPerson" asp-action="Index" class="flex items-center px-3 py-2 rounded-lg text-sm text-slate-500 hover:text-slate-800 hover:bg-slate-50 transition-colors">
                                Persons
                            </a>
                        </li>
                    </ul>
                </details>
            </li>

            <!-- System Section -->
            <li class="pt-4 mt-4 border-t border-slate-100">
                <p class="px-3 text-xs font-semibold text-slate-400 uppercase tracking-wider mb-2 sidebar-text transition-all duration-300">System</p>
                
                @if (_CurrentUser.HasAccess("User/Index"))
                {
                    <a asp-area="" asp-controller="User" asp-action="Index" data-title="User Management" class="nav-item-link flex items-center px-3 py-2.5 rounded-lg transition-colors group @GetActiveClasses("User")">
                        <i class="fa-solid fa-users w-6 text-center text-lg transition-colors @GetActiveIconClasses("User")"></i>
                        <span class="ml-2 font-medium sidebar-text whitespace-nowrap transition-all duration-300">User Management</span>
                    </a>
                }
                
                @if (_CurrentUser.HasAccess("Role/Index"))
                {
                    <a asp-area="" asp-controller="Role" asp-action="Index" data-title="Roles & Permissions" class="nav-item-link flex items-center px-3 py-2.5 rounded-lg transition-colors group @GetActiveClasses("Role")">
                        <i class="fa-solid fa-user-shield w-6 text-center text-lg transition-colors @GetActiveIconClasses("Role")"></i>
                        <span class="ml-2 font-medium sidebar-text whitespace-nowrap transition-all duration-300">Roles & Permissions</span>
                    </a>
                }
                
                @if (_CurrentUser.HasAccess("IdentitySetting/Edit"))
                {
                    <a asp-area="" asp-controller="IdentitySetting" asp-action="Edit" data-title="Identity Setting" class="nav-item-link flex items-center px-3 py-2.5 rounded-lg transition-colors group @GetActiveClasses("IdentitySetting")">
                        <i class="fa-solid fa-gears w-6 text-center text-lg transition-colors @GetActiveIconClasses("IdentitySetting")"></i>
                        <span class="ml-2 font-medium sidebar-text whitespace-nowrap transition-all duration-300">Identity Setting</span>
                    </a>
                }

                @if (_CurrentUser.HasAccess("Permissions/Index"))
                {
                    <a asp-area="" asp-controller="Permission" asp-action="Index" data-title="Permissions" class="nav-item-link flex items-center px-3 py-2.5 rounded-lg transition-colors group @GetActiveClasses("Permission")">
                        <i class="fa-solid fa-user-lock w-6 text-center text-lg transition-colors @GetActiveIconClasses("Permission")"></i>
                        <span class="ml-2 font-medium sidebar-text whitespace-nowrap transition-all duration-300">Permissions</span>
                    </a>
                }
                
                @if (_CurrentUser.HasAccess("FileExplorer/Index"))
                {
                    <a asp-area="" asp-controller="FileExplorer" asp-action="Index" data-title="File Explorer" class="nav-item-link flex items-center px-3 py-2.5 rounded-lg transition-colors group @GetActiveClasses("FileExplorer")">
                        <i class="fa-solid fa-folder-tree w-6 text-center text-lg transition-colors @GetActiveIconClasses("FileExplorer")"></i>
                        <span class="ml-2 font-medium sidebar-text whitespace-nowrap transition-all duration-300">File Explorer</span>
                    </a>
                }
                
                @if (_CurrentUser.HasAccess("Pages/Index"))
                {
                    <a asp-area="" asp-controller="Pages" asp-action="Index" data-title="Pages (Admin)" class="nav-item-link flex items-center px-3 py-2.5 rounded-lg transition-colors group @GetActiveClasses("Pages")">
                        <i class="fa-solid fa-book-open w-6 text-center text-lg transition-colors @GetActiveIconClasses("Pages")"></i>
                        <span class="ml-2 font-medium sidebar-text whitespace-nowrap transition-all duration-300">Pages (Admin)</span>
                    </a>
                }
                
                @if (_CurrentUser.HasAccess("Component/Index"))
                {
                    <a asp-area="" asp-controller="Component" asp-action="Index" data-title="Components" class="nav-item-link flex items-center px-3 py-2.5 rounded-lg transition-colors group @GetActiveClasses("Component")">
                        <i class="fa-solid fa-cubes w-6 text-center text-lg transition-colors @GetActiveIconClasses("Component")"></i>
                        <span class="ml-2 font-medium sidebar-text whitespace-nowrap transition-all duration-300">Components</span>
                    </a>
                }
                
                @if (_CurrentUser.IsAdmin)
                {
                    <a href="/healthchecks-ui" target="_blank" data-title="Health Status" class="nav-item-link flex items-center px-3 py-2.5 rounded-lg transition-colors group text-slate-600 hover:bg-slate-50 hover:text-slate-900">
                        <i class="fa-solid fa-heart-pulse w-6 text-center text-lg text-slate-400 group-hover:text-slate-600"></i>
                        <span class="ml-2 font-medium sidebar-text whitespace-nowrap transition-all duration-300">Health Status</span>
                    </a>
                    <a href="/hangfire" target="_blank" data-title="Background Jobs" class="nav-item-link flex items-center px-3 py-2.5 rounded-lg transition-colors group text-slate-600 hover:bg-slate-50 hover:text-slate-900">
                        <i class="fa-solid fa-server w-6 text-center text-lg text-slate-400 group-hover:text-slate-600"></i>
                        <span class="ml-2 font-medium sidebar-text whitespace-nowrap transition-all duration-300">Background Jobs</span>
                    </a>
                }
            </li>
        </ul>
    </nav>
</aside>

<script>
    document.addEventListener("DOMContentLoaded", function() {
        const sidebar = document.getElementById("sidebar");
        const desktopToggle = document.getElementById("sidebarToggleDesktop");
        const mobileMenuBtn = document.getElementById("mobileMenuBtn");
        const mobileCloseBtn = document.getElementById("sidebarToggleMobileClose");
        const mobileOverlay = document.getElementById("mobileSidebarOverlay");

        // Desktop Toggle Logic
        if (localStorage.getItem('sidebarCollapsed') === 'true' && window.innerWidth >= 768) {
            sidebar.classList.add("collapsed");
        }

        desktopToggle?.addEventListener("click", function() {
            sidebar.classList.toggle("collapsed");
            localStorage.setItem('sidebarCollapsed', sidebar.classList.contains("collapsed"));
        });

        // Mobile Toggle Logic
        function openMobileSidebar() {
            sidebar.classList.remove("-translate-x-full");
            mobileOverlay.classList.add("active");
            // small delay for opacity transition
            setTimeout(() => mobileOverlay.classList.add("opacity-100"), 10);
        }

        function closeMobileSidebar() {
            sidebar.classList.add("-translate-x-full");
            mobileOverlay.classList.remove("opacity-100");
            setTimeout(() => mobileOverlay.classList.remove("active"), 300);
        }

        mobileMenuBtn?.addEventListener("click", openMobileSidebar);
        mobileCloseBtn?.addEventListener("click", closeMobileSidebar);
        mobileOverlay?.addEventListener("click", closeMobileSidebar);
    });
</script>
"""

with open(path, "w") as f:
    f.write(content)
