import re

with open('/Users/mohammadhosein/Development/AzureTest/src/AzureTest.MVC/Views/Debt/Index.cshtml', 'r') as f:
    content = f.read()

filter_box = """
<!-- Search/Filter Bar -->
<div class="bg-white p-5 rounded-xl border border-slate-200 shadow-sm mb-6">
    <div class="flex items-center justify-between mb-4">
        <h3 class="text-lg font-semibold text-slate-800">Filter Debts</h3>
    </div>
    <form asp-action="Index" method="get">
        <div class="grid grid-cols-1 md:grid-cols-6 gap-4">
            <div class="md:col-span-2">
                <label class="block text-sm font-medium text-slate-700 mb-1">Search</label>
                <div class="relative">
                    <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                        <i class="fa-solid fa-magnifying-glass text-slate-400"></i>
                    </div>
                    <input name="search" type="text" value="@ViewBag.CurrentSearch" class="block w-full pl-10 pr-3 py-2 border border-slate-300 rounded-lg leading-5 bg-white placeholder-slate-500 focus:outline-none focus:ring-1 focus:ring-brand-500 focus:border-brand-500 sm:text-sm" placeholder="Description or Person..." />
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
                    Showing Page <span class="font-medium">@ViewBag.CurrentPage</span>
                    of <span class="font-medium">@ViewBag.TotalPages</span>
                </p>
            </div>
            <div>
                <nav class="relative z-0 inline-flex rounded-md shadow-sm -space-x-px" aria-label="Pagination">
                    @{
                        var prevDisabled = ViewBag.CurrentPage <= 1 ? "pointer-events-none text-slate-300" : "text-slate-500 hover:bg-slate-50";
                        var nextDisabled = ViewBag.CurrentPage >= ViewBag.TotalPages ? "pointer-events-none text-slate-300" : "text-slate-500 hover:bg-slate-50";
                    }
                    <a asp-action="Index" asp-route-page="@(ViewBag.CurrentPage - 1)" asp-route-sort="@ViewBag.CurrentSort" asp-route-dir="@ViewBag.CurrentDir" asp-route-search="@ViewBag.CurrentSearch" class="relative inline-flex items-center px-2 py-2 rounded-l-md border border-slate-300 bg-white text-sm font-medium @prevDisabled">
                        <span class="sr-only">Previous</span>
                        <i class="fa-solid fa-chevron-left"></i>
                    </a>
                    
                    @{
                        int window = 2;
                        int startPage = Math.Max(1, ViewBag.CurrentPage - window);
                        int endPage = Math.Min(ViewBag.TotalPages, ViewBag.CurrentPage + window);
                        
                        if (startPage > 1)
                        {
                            <a asp-action="Index" asp-route-page="1" asp-route-sort="@ViewBag.CurrentSort" asp-route-dir="@ViewBag.CurrentDir" asp-route-search="@ViewBag.CurrentSearch" class="relative inline-flex items-center px-4 py-2 border border-slate-300 bg-white text-sm font-medium text-slate-700 hover:bg-slate-50">1</a>
                            if (startPage > 2)
                            {
                                <span class="relative inline-flex items-center px-4 py-2 border border-slate-300 bg-white text-sm font-medium text-slate-700">...</span>
                            }
                        }
                        
                        for (int i = startPage; i <= endPage; i++)
                        {
                            if (i == ViewBag.CurrentPage)
                            {
                                <span class="relative inline-flex items-center px-4 py-2 border border-brand-500 bg-brand-50 text-sm font-medium text-brand-600 z-10">@i</span>
                            }
                            else
                            {
                                <a asp-action="Index" asp-route-page="@i" asp-route-sort="@ViewBag.CurrentSort" asp-route-dir="@ViewBag.CurrentDir" asp-route-search="@ViewBag.CurrentSearch" class="relative inline-flex items-center px-4 py-2 border border-slate-300 bg-white text-sm font-medium text-slate-700 hover:bg-slate-50">@i</a>
                            }
                        }
                        
                        if (endPage < ViewBag.TotalPages)
                        {
                            if (endPage < ViewBag.TotalPages - 1)
                            {
                                <span class="relative inline-flex items-center px-4 py-2 border border-slate-300 bg-white text-sm font-medium text-slate-700">...</span>
                            }
                            <a asp-action="Index" asp-route-page="@ViewBag.TotalPages" asp-route-sort="@ViewBag.CurrentSort" asp-route-dir="@ViewBag.CurrentDir" asp-route-search="@ViewBag.CurrentSearch" class="relative inline-flex items-center px-4 py-2 border border-slate-300 bg-white text-sm font-medium text-slate-700 hover:bg-slate-50">@ViewBag.TotalPages</a>
                        }
                    }
                    
                    <a asp-action="Index" asp-route-page="@(ViewBag.CurrentPage + 1)" asp-route-sort="@ViewBag.CurrentSort" asp-route-dir="@ViewBag.CurrentDir" asp-route-search="@ViewBag.CurrentSearch" class="relative inline-flex items-center px-2 py-2 rounded-r-md border border-slate-300 bg-white text-sm font-medium @nextDisabled">
                        <span class="sr-only">Next</span>
                        <i class="fa-solid fa-chevron-right"></i>
                    </a>
                </nav>
            </div>
        </div>
    </div>
"""

# Insert Filter Box before table
if "<!-- Search/Filter Bar -->" not in content:
    content = re.sub(r'(<div class="bg-white rounded-xl shadow-sm border border-slate-200 overflow-hidden">)', filter_box + r'\n\1', content)

# Replace existing pagination
content = re.sub(r'@if \(ViewBag\.TotalPages > 1\)\s*\{.*?</div>\s*\}', pagination_html, content, flags=re.DOTALL)

with open('/Users/mohammadhosein/Development/AzureTest/src/AzureTest.MVC/Views/Debt/Index.cshtml', 'w') as f:
    f.write(content)
