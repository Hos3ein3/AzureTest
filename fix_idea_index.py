with open('/Users/mohammadhosein/Development/AzureTest/src/AzureTest.MVC/Views/IdeaTracker/Index.cshtml', 'r') as f:
    content = f.read()

filter_block = """
    <!-- Search/Filter Bar -->
    <div class="bg-white p-5 rounded-xl border border-slate-200 shadow-sm mb-6">
        <div class="flex items-center justify-between mb-4">
            <h3 class="text-lg font-semibold text-slate-800">Filter Ideas</h3>
        </div>
        <form method="get">
            <div class="grid grid-cols-1 md:grid-cols-6 gap-4">
                <div class="md:col-span-2">
                    <label class="block text-sm font-medium text-slate-700 mb-1">Search</label>
                    <div class="relative">
                        <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                            <i class="fa-solid fa-magnifying-glass text-slate-400"></i>
                        </div>
                        <input name="search" type="text" value="@ViewBag.Search" class="block w-full pl-10 pr-3 py-2 border border-slate-300 rounded-lg leading-5 bg-white placeholder-slate-500 focus:outline-none focus:ring-1 focus:ring-brand-500 focus:border-brand-500 sm:text-sm" placeholder="Title, content..." />
                    </div>
                </div>
                
                <div class="md:col-span-2">
                    <label class="block text-sm font-medium text-slate-700 mb-1">Status</label>
                    <select name="status" onchange="this.form.submit()" class="block w-full pl-3 pr-10 py-2 text-base border border-slate-300 focus:outline-none focus:ring-brand-500 focus:border-brand-500 sm:text-sm rounded-lg">
                        <option value="">All Statuses</option>
                        @foreach (var s in Enum.GetValues<IdeaStatus>())
                        {
                            var selected = ViewBag.Status != null && (int)ViewBag.Status == (int)s ? "selected" : "";
                            <option value="@((int)s)" selected="@selected">@s</option>
                        }
                    </select>
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

import re
content = re.sub(r'<!-- Search/Filter Bar -->.*?</div>\s*<!-- Notion-Style Grid -->', filter_block + '\n    <!-- Notion-Style Grid -->', content, flags=re.DOTALL)

with open('/Users/mohammadhosein/Development/AzureTest/src/AzureTest.MVC/Views/IdeaTracker/Index.cshtml', 'w') as f:
    f.write(content)
