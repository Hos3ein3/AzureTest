import os

file = "src/AzureTest.MVC/Views/Shared/_Layout.cshtml"
with open(file, "r") as f:
    content = f.read()

nav_item = """
                        <!-- Gym Navigation -->
                        <li>
                            <a asp-area="" asp-controller="Workout" asp-action="Index" class="flex items-center px-3 py-2.5 rounded-lg transition-colors group @GetActiveClasses("Workout")">
                                <i class="fa-solid fa-dumbbell w-6 @GetActiveIconClasses("Workout")"></i>
                                <span class="ml-2 font-medium">My Workouts</span>
                            </a>
                        </li>
                        @if (User.IsInRole("Admin"))
                        {
                            <li>
                                <a asp-area="" asp-controller="GymAdmin" asp-action="Index" class="flex items-center px-3 py-2.5 rounded-lg transition-colors group @GetActiveClasses("GymAdmin")">
                                    <i class="fa-solid fa-gears w-6 @GetActiveIconClasses("GymAdmin")"></i>
                                    <span class="ml-2 font-medium">Gym Admin</span>
                                </a>
                            </li>
                        }
"""

if "fa-dumbbell" not in content:
    target = 'class="fa-solid fa-flask w-6 @GetActiveIconClasses("SampleNote")"></i>\n                                <span class="ml-2 font-medium">Sample Notes <span class="text-xs text-yellow-400">(Dev)</span></span>\n                            </a>\n                        </li>\n                    }'
    
    if target in content:
        content = content.replace(target, target + "\n" + nav_item)
        with open(file, "w") as f:
            f.write(content)
        print("Patched correctly")
    else:
        print("Target not found")
