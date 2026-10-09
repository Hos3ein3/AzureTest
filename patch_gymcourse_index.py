import re

path = "src/AzureTest.MVC/Views/GymCourses/Index.cshtml"
with open(path, "r") as f: c = f.read()

old_table = """    <div class="bg-white dark:bg-slate-800 rounded-xl shadow-sm border border-slate-100 dark:border-slate-700 overflow-hidden flex flex-col">
        <div class="overflow-x-auto">
            <table class="min-w-full divide-y divide-slate-200 dark:divide-slate-700">
                <thead class="bg-slate-50 dark:bg-slate-900/50">
                    <tr>
                        <th scope="col" class="px-6 py-3 text-left text-xs font-semibold text-slate-500 uppercase tracking-wider">@Html.Raw(GetSortLink("Name", "Name"))</th>
                        <th scope="col" class="px-6 py-3 text-left text-xs font-semibold text-slate-500 uppercase tracking-wider">Description</th>
                        <th scope="col" class="px-6 py-3 text-left text-xs font-semibold text-slate-500 uppercase tracking-wider">Tags</th>
                        <th scope="col" class="px-6 py-3 text-right text-xs font-semibold text-slate-500 uppercase tracking-wider">Length</th>
                        <th scope="col" class="px-6 py-3 text-right text-xs font-semibold text-slate-500 uppercase tracking-wider">Actions</th>
                    </tr>
                </thead>
                <tbody class="bg-white dark:bg-slate-800 divide-y divide-slate-200 dark:divide-slate-700">
                    @foreach (var item in Model)
                    {
                        <tr class="hover:bg-slate-50 dark:hover:bg-slate-700/50 transition-colors">
                            <td class="px-6 py-4">
                                <div class="text-sm font-bold text-slate-900 dark:text-white">@item.Name</div>
                            </td>
                            <td class="px-6 py-4">
                                <div class="text-sm text-slate-500 dark:text-slate-400 max-w-xs truncate" title="@item.Description">
                                    @(string.IsNullOrWhiteSpace(item.Description) ? "—" : item.Description)
                                </div>
                            </td>
                            <td class="px-6 py-4">
                                <div class="flex flex-wrap gap-1">
                                    @if(item.Tags != null && item.Tags.Any()) {
                                        foreach(var tag in item.Tags) {
                                            <span class="inline-flex items-center px-2 py-0.5 rounded text-xs font-medium bg-slate-100 text-slate-800 dark:bg-slate-700 dark:text-slate-300">@tag</span>
                                        }
                                    } else {
                                        <span class="text-slate-400 text-sm">—</span>
                                    }
                                </div>
                            </td>
                            <td class="px-6 py-4 whitespace-nowrap text-right text-sm font-medium text-slate-900 dark:text-white">
                                @(item.Weeks?.Count ?? 0) Weeks
                            </td>
                            <td class="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                                <a asp-action="Edit" asp-route-id="@item.Id" class="text-slate-600 hover:text-brand-600 dark:text-slate-400 dark:hover:text-brand-400 mx-2" title="Edit">
                                    <i class="fa-solid fa-pen-to-square"></i>
                                </a>
                            </td>
                        </tr>
                    }
                </tbody>
            </table>
        </div>"""

new_grid = """    <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6 mb-8">
        @foreach (var item in Model)
        {
            <div class="bg-white rounded-xl shadow-sm border border-slate-200 overflow-hidden flex flex-col hover:shadow-md transition-shadow">
                <div class="p-6 flex-1">
                    <div class="flex justify-between items-start mb-4">
                        <h3 class="text-lg font-bold text-slate-900 line-clamp-1" title="@item.Name">@item.Name</h3>
                        <span class="bg-slate-50 text-slate-600 border border-slate-200 text-xs font-bold px-2.5 py-1 rounded-full whitespace-nowrap">
                            @(item.Weeks?.Count ?? 0) Weeks
                        </span>
                    </div>
                    
                    <p class="text-sm text-slate-600 mb-4 line-clamp-2" title="@item.Description">
                        @(string.IsNullOrWhiteSpace(item.Description) ? "No description provided." : item.Description)
                    </p>
                    
                    <div class="flex flex-wrap gap-1.5 mt-auto">
                        @if(item.Tags != null && item.Tags.Any()) {
                            foreach(var tag in item.Tags.Take(4)) {
                                <span class="inline-flex items-center px-2 py-0.5 rounded text-xs font-medium bg-slate-100 text-slate-600 border border-slate-200">@tag</span>
                            }
                            if(item.Tags.Count > 4) {
                                <span class="inline-flex items-center px-2 py-0.5 rounded text-xs font-medium bg-slate-100 text-slate-500 border border-slate-200">+@(item.Tags.Count - 4)</span>
                            }
                        }
                    </div>
                </div>
                <div class="bg-slate-50 px-6 py-3 border-t border-slate-100 flex justify-end">
                    <a asp-action="Edit" asp-route-id="@item.Id" class="inline-flex items-center justify-center rounded-lg border border-slate-300 bg-white px-3 py-1.5 text-sm font-medium text-slate-700 shadow-sm hover:bg-slate-50 transition-colors">
                        <i class="fa-solid fa-pen-to-square mr-1.5 text-brand-600"></i> Edit Course
                    </a>
                </div>
            </div>
        }
    </div>
    
    <div>"""

c = c.replace(old_table, new_grid)

# Close the trailing div from the table replacement block
c = c.replace("""    </div>
}""", """    </div>
    </div>
}""")

# Remove dark mode classes from pagination while we're at it
c = c.replace('dark:bg-slate-800', '')
c = c.replace('dark:border-slate-700', '')
c = c.replace('dark:border-slate-600', '')
c = c.replace('dark:bg-slate-900/50', '')
c = c.replace('dark:hover:bg-slate-700/50', '')
c = c.replace('dark:hover:bg-slate-700', '')
c = c.replace('dark:text-slate-400', '')
c = c.replace('dark:text-white', '')
c = c.replace('dark:text-slate-300', '')

with open(path, "w") as f: f.write(c)
