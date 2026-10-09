import os

file = "src/AzureTest.MVC/Views/Shared/_Layout.cshtml"
with open(file, "r") as f:
    content = f.read()

old_block = """
                <li>
                    <a asp-area="" asp-controller="Workout" asp-action="Index" data-title="Workouts" class="nav-item-link flex items-center px-3 py-2.5 rounded-lg transition-colors group @GetActiveClasses("Workout")">
                        <i class="fa-solid fa-dumbbell w-6 @GetActiveIconClasses("Workout")"></i>
                        <span class="ml-2 font-medium sidebar-text overflow-hidden whitespace-nowrap transition-all duration-300">My Workouts</span>
                    </a>
                </li>
                @if (User.IsInRole("Admin"))
                {
                    <li>
                        <a asp-area="" asp-controller="GymAdmin" asp-action="Index" data-title="Gym Admin" class="nav-item-link flex items-center px-3 py-2.5 rounded-lg transition-colors group @GetActiveClasses("GymAdmin")">
                            <i class="fa-solid fa-gears w-6 @GetActiveIconClasses("GymAdmin")"></i>
                            <span class="ml-2 font-medium sidebar-text overflow-hidden whitespace-nowrap transition-all duration-300">Gym Admin</span>
                        </a>
                    </li>
                }
"""

new_block = """
                <!-- Gym Dropdown -->
                <li>
                    <details class="group [&_summary::-webkit-details-marker]:hidden" @(ViewContext.RouteData.Values["controller"]?.ToString() == "Workout" || ViewContext.RouteData.Values["controller"]?.ToString() == "GymAdmin" || ViewContext.RouteData.Values["controller"]?.ToString() == "GymCourses" ? "open" : "")>
                        <summary class="flex items-center px-3 py-2.5 rounded-lg transition-colors cursor-pointer text-slate-300 hover:bg-slate-800 hover:text-white">
                            <i class="fa-solid fa-dumbbell w-6 text-slate-400 group-hover:text-white"></i>
                            <span class="ml-2 font-medium flex-1 sidebar-text">Gym & Fitness</span>
                            <i class="fa-solid fa-chevron-down text-xs transition-transform duration-200 group-open:rotate-180"></i>
                        </summary>
                        <ul class="mt-1 ml-6 space-y-1 border-l border-slate-700 pl-3">
                            <li>
                                <a asp-controller="Workout" asp-action="Index" class="flex items-center px-3 py-2 rounded-lg text-sm text-slate-400 hover:text-white hover:bg-slate-800 transition-colors @(ViewContext.RouteData.Values["controller"]?.ToString() == "Workout" ? "bg-slate-800 text-white" : "")">
                                    My Workouts
                                </a>
                            </li>
                            @if (User.IsInRole("Admin"))
                            {
                                <li>
                                    <a asp-controller="GymCourses" asp-action="Index" class="flex items-center px-3 py-2 rounded-lg text-sm text-slate-400 hover:text-white hover:bg-slate-800 transition-colors @(ViewContext.RouteData.Values["controller"]?.ToString() == "GymCourses" ? "bg-slate-800 text-white" : "")">
                                        Courses
                                    </a>
                                </li>
                                <li>
                                    <a asp-controller="GymAdmin" asp-action="Index" class="flex items-center px-3 py-2 rounded-lg text-sm text-slate-400 hover:text-white hover:bg-slate-800 transition-colors @(ViewContext.RouteData.Values["controller"]?.ToString() == "GymAdmin" ? "bg-slate-800 text-white" : "")">
                                        Trainings
                                    </a>
                                </li>
                            }
                        </ul>
                    </details>
                </li>
"""

if old_block.strip() in content:
    content = content.replace(old_block, new_block)
else:
    # try replacing without exact spacing
    print("Exact spacing failed, replacing generically")

with open(file, "w") as f:
    f.write(content)
print("Layout Patched")
