import re

path = "src/AzureTest.MVC/Views/Shared/_Sidebar.cshtml"
with open(path, "r") as f: c = f.read()

# Replace Gym Section
gym_old = """            <!-- Gym Section -->
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
            </li>"""

gym_new = """            <!-- Gym Section -->
            <li class="pt-4 mt-4 border-t border-slate-100">
                <p class="px-3 text-xs font-semibold text-slate-400 uppercase tracking-wider mb-2 sidebar-text transition-all duration-300">Gym</p>
                
                @if (User.IsInRole("Admin") || _CurrentUser.HasAccess(@Url.Action("GymCourses", "Index", new {})))
                {
                    <a asp-area="" asp-controller="GymCourses" asp-action="Index" data-title="Courses" class="nav-item-link flex items-center px-3 py-2.5 rounded-lg transition-colors group @GetActiveClasses("GymCourses")">
                        <i class="fa-solid fa-chalkboard-user w-6 text-center text-lg transition-colors @GetActiveIconClasses("GymCourses")"></i>
                        <span class="ml-2 font-medium sidebar-text whitespace-nowrap transition-all duration-300">Courses</span>
                    </a>
                }
                
                @if (User.IsInRole("Admin") || _CurrentUser.HasAccess(@Url.Action("GymCourses", "Index", new {})))
                {
                    <a asp-area="" asp-controller="GymAdmin" asp-action="Index" data-title="Trainings" class="nav-item-link flex items-center px-3 py-2.5 rounded-lg transition-colors group @GetActiveClasses("GymAdmin")">
                        <i class="fa-solid fa-dumbbell w-6 text-center text-lg transition-colors @GetActiveIconClasses("GymAdmin")"></i>
                        <span class="ml-2 font-medium sidebar-text whitespace-nowrap transition-all duration-300">Trainings</span>
                    </a>
                }
                
                <a asp-area="" asp-controller="Workout" asp-action="Index" data-title="Workout" class="nav-item-link flex items-center px-3 py-2.5 rounded-lg transition-colors group @GetActiveClasses("Workout")">
                    <i class="fa-solid fa-person-running w-6 text-center text-lg transition-colors @GetActiveIconClasses("Workout")"></i>
                    <span class="ml-2 font-medium sidebar-text whitespace-nowrap transition-all duration-300">Workout</span>
                </a>
            </li>"""

# Replace Financials Section
fin_old = """            <!-- Financials Section -->
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
            </li>"""

fin_new = """            <!-- Financials Section -->
            <li class="pt-4 mt-4 border-t border-slate-100">
                <p class="px-3 text-xs font-semibold text-slate-400 uppercase tracking-wider mb-2 sidebar-text transition-all duration-300">Financials</p>
                
                <a asp-area="" asp-controller="Expense" asp-action="Index" data-title="Expenses" class="nav-item-link flex items-center px-3 py-2.5 rounded-lg transition-colors group @GetActiveClasses("Expense")">
                    <i class="fa-solid fa-money-bill-wave w-6 text-center text-lg transition-colors @GetActiveIconClasses("Expense")"></i>
                    <span class="ml-2 font-medium sidebar-text whitespace-nowrap transition-all duration-300">Expenses</span>
                </a>
                
                <a asp-area="" asp-controller="ExpenseCategory" asp-action="Index" data-title="Expense Categories" class="nav-item-link flex items-center px-3 py-2.5 rounded-lg transition-colors group @GetActiveClasses("ExpenseCategory")">
                    <i class="fa-solid fa-tags w-6 text-center text-lg transition-colors @GetActiveIconClasses("ExpenseCategory")"></i>
                    <span class="ml-2 font-medium sidebar-text whitespace-nowrap transition-all duration-300">Categories</span>
                </a>
                
                <a asp-area="" asp-controller="Debt" asp-action="Index" data-title="Debts" class="nav-item-link flex items-center px-3 py-2.5 rounded-lg transition-colors group @GetActiveClasses("Debt")">
                    <i class="fa-solid fa-handshake-angle w-6 text-center text-lg transition-colors @GetActiveIconClasses("Debt")"></i>
                    <span class="ml-2 font-medium sidebar-text whitespace-nowrap transition-all duration-300">Debts</span>
                </a>
                
                <a asp-area="" asp-controller="DebtPerson" asp-action="Index" data-title="Debt Persons" class="nav-item-link flex items-center px-3 py-2.5 rounded-lg transition-colors group @GetActiveClasses("DebtPerson")">
                    <i class="fa-solid fa-users-line w-6 text-center text-lg transition-colors @GetActiveIconClasses("DebtPerson")"></i>
                    <span class="ml-2 font-medium sidebar-text whitespace-nowrap transition-all duration-300">Persons</span>
                </a>
            </li>"""

c = c.replace(gym_old, gym_new).replace(fin_old, fin_new)

with open(path, "w") as f: f.write(c)
