import re
import os

files = [
    '/Users/mohammadhosein/Development/AzureTest/src/AzureTest.MVC/Views/Debt/Index.cshtml',
    '/Users/mohammadhosein/Development/AzureTest/src/AzureTest.MVC/Views/Expense/Index.cshtml',
    '/Users/mohammadhosein/Development/AzureTest/src/AzureTest.MVC/Views/DebtPerson/Index.cshtml'
]

filter_box = """
<!-- Search/Filter Bar -->
<div class="bg-white p-5 rounded-xl border border-slate-200 shadow-sm mb-6">
    <div class="flex items-center justify-between mb-4">
        <h3 class="text-lg font-semibold text-slate-800">Filter Records</h3>
    </div>
    <form asp-action="Index" method="get">
        <div class="grid grid-cols-1 md:grid-cols-6 gap-4">
            <div class="md:col-span-2">
                <label class="block text-sm font-medium text-slate-700 mb-1">Search</label>
                <div class="relative">
                    <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                        <i class="fa-solid fa-magnifying-glass text-slate-400"></i>
                    </div>
                    <input name="searchString" type="text" value="@ViewData["CurrentFilter"]" class="block w-full pl-10 pr-3 py-2 border border-slate-300 rounded-lg leading-5 bg-white placeholder-slate-500 focus:outline-none focus:ring-1 focus:ring-brand-500 focus:border-brand-500 sm:text-sm" placeholder="Search..." />
                </div>
            </div>
            
            <div class="md:col-span-2 flex items-end justify-start gap-2">
                <button type="submit" class="inline-flex justify-center items-center px-4 py-2 border border-transparent shadow-sm text-sm font-medium rounded-lg text-white bg-brand-600 hover:bg-brand-700 focus:outline-none transition-colors">
                    Filter
                </button>
                <a asp-action="Index" class="inline-flex justify-center items-center px-4 py-2 border border-slate-300 shadow-sm text-sm font-medium rounded-lg text-slate-700 bg-white hover:bg-slate-50 focus:outline-none transition-colors" title="Clear Filters">
                    <i class="fa-solid fa-rotate-left"></i>
                </a>
            </div>
        </div>
    </form>
</div>
"""

pagination_html = """
    <!-- Pagination -->
    <div class="bg-white px-4 py-3 border-t border-slate-200 sm:px-6 flex items-center justify-between">
        <div class="hidden sm:flex-1 sm:flex sm:items-center sm:justify-between">
            <div>
                <p class="text-sm text-slate-700">
                    Showing <span class="font-medium">@(((Model.PageNumber - 1) * Model.PageSize) + 1)</span>
                    to <span class="font-medium">@(Math.Min(Model.PageNumber * Model.PageSize, Model.TotalCount))</span>
                    of <span class="font-medium">@Model.TotalCount</span> results
                </p>
            </div>
            <div>
                <nav class="relative z-0 inline-flex rounded-md shadow-sm -space-x-px" aria-label="Pagination">
                    @{
                        var prevDisabled = !Model.HasPreviousPage ? "pointer-events-none text-slate-300" : "text-slate-500 hover:bg-slate-50";
                        var nextDisabled = !Model.HasNextPage ? "pointer-events-none text-slate-300" : "text-slate-500 hover:bg-slate-50";
                    }
                    <a asp-action="Index" asp-route-pageNumber="@(Model.PageNumber - 1)" asp-route-sortOrder="@ViewData["CurrentSort"]" asp-route-searchString="@ViewData["CurrentFilter"]" class="relative inline-flex items-center px-2 py-2 rounded-l-md border border-slate-300 bg-white text-sm font-medium @prevDisabled">
                        <span class="sr-only">Previous</span>
                        <i class="fa-solid fa-chevron-left"></i>
                    </a>
                    
                    @{
                        int window = 2;
                        int startPage = Math.Max(1, Model.PageNumber - window);
                        int endPage = Math.Min(Model.TotalPages, Model.PageNumber + window);
                        
                        if (startPage > 1)
                        {
                            <a asp-action="Index" asp-route-pageNumber="1" asp-route-sortOrder="@ViewData["CurrentSort"]" asp-route-searchString="@ViewData["CurrentFilter"]" class="relative inline-flex items-center px-4 py-2 border border-slate-300 bg-white text-sm font-medium text-slate-700 hover:bg-slate-50">1</a>
                            if (startPage > 2)
                            {
                                <span class="relative inline-flex items-center px-4 py-2 border border-slate-300 bg-white text-sm font-medium text-slate-700">...</span>
                            }
                        }
                        
                        for (int i = startPage; i <= endPage; i++)
                        {
                            if (i == Model.PageNumber)
                            {
                                <span class="relative inline-flex items-center px-4 py-2 border border-brand-500 bg-brand-50 text-sm font-medium text-brand-600 z-10">@i</span>
                            }
                            else
                            {
                                <a asp-action="Index" asp-route-pageNumber="@i" asp-route-sortOrder="@ViewData["CurrentSort"]" asp-route-searchString="@ViewData["CurrentFilter"]" class="relative inline-flex items-center px-4 py-2 border border-slate-300 bg-white text-sm font-medium text-slate-700 hover:bg-slate-50">@i</a>
                            }
                        }
                        
                        if (endPage < Model.TotalPages)
                        {
                            if (endPage < Model.TotalPages - 1)
                            {
                                <span class="relative inline-flex items-center px-4 py-2 border border-slate-300 bg-white text-sm font-medium text-slate-700">...</span>
                            }
                            <a asp-action="Index" asp-route-pageNumber="@Model.TotalPages" asp-route-sortOrder="@ViewData["CurrentSort"]" asp-route-searchString="@ViewData["CurrentFilter"]" class="relative inline-flex items-center px-4 py-2 border border-slate-300 bg-white text-sm font-medium text-slate-700 hover:bg-slate-50">@Model.TotalPages</a>
                        }
                    }
                    
                    <a asp-action="Index" asp-route-pageNumber="@(Model.PageNumber + 1)" asp-route-sortOrder="@ViewData["CurrentSort"]" asp-route-searchString="@ViewData["CurrentFilter"]" class="relative inline-flex items-center px-2 py-2 rounded-r-md border border-slate-300 bg-white text-sm font-medium @nextDisabled">
                        <span class="sr-only">Next</span>
                        <i class="fa-solid fa-chevron-right"></i>
                    </a>
                </nav>
            </div>
        </div>
    </div>
"""

for path in files:
    if not os.path.exists(path): continue
    with open(path, 'r') as f:
        content = f.read()

    # 1. Remove old small inline form if exists
    # Look for: <form asp-action="Index" method="get" class="flex gap-2"> ... </form>
    content = re.sub(r'<form asp-action="Index" method="get" class="flex gap-2">.*?</form>', '', content, flags=re.DOTALL)
    
    # 2. Insert new Filter Box before the main table block
    if filter_box not in content:
        content = re.sub(r'(<div class="bg-white rounded-xl shadow-sm border border-slate-100 overflow-hidden mb-6">)',
                         filter_box + r'\n\1', content)
                         
    # 3. Replace Old Pagination with new Pagination
    # The old pagination usually starts with <!-- Pagination --> or <div class="flex items-center justify-between">
    # Let's completely replace everything after </table></div> with the new pagination if it has old pagination
    content = re.sub(r'</table>\s*</div>\s*</div>\s*(<!-- Pagination -->)?\s*<div class="flex items-center justify-between">.*',
                     '</table>\n    </div>\n' + pagination_html + '\n</div>\n', content, flags=re.DOTALL)

    with open(path, 'w') as f:
        f.write(content)

