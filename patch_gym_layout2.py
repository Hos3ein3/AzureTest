import os

file = "src/AzureTest.MVC/Views/Shared/_Layout.cshtml"
with open(file, "r") as f:
    content = f.read()

nav_item = """
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

target = 'asp-controller="Habit"'

if "fa-dumbbell" not in content:
    idx = content.find(target)
    if idx != -1:
        # Find the preceding <li>
        start_li = content.rfind("<li>", 0, idx)
        content = content[:start_li] + nav_item + content[start_li:]
        with open(file, "w") as f:
            f.write(content)
        print("Patched correctly")
    else:
        print("Target not found")
