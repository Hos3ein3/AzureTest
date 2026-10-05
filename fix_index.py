import re

file = "src/AzureTest.MVC/Views/Component/Index.cshtml"
with open(file, "r") as f:
    content = f.read()

content = content.replace(
"""            <div class="w-full sm:w-auto">
                <label class="block text-xs font-medium text-slate-500 mb-1">Sort By</label>""",
"""            <div class="w-full sm:w-auto">
                <label class="block text-xs font-medium text-slate-500 mb-1">Category</label>
                <select name="category" class="rounded-md border-slate-300 shadow-sm focus:border-brand-500 focus:ring-brand-500 sm:text-sm">
                    <option value="">All Categories</option>
                    @foreach (var cat in categories)
                    {
                        <option value="@cat" selected="@(ViewBag.Category == cat ? "selected" : null)">@cat</option>
                    }
                </select>
            </div>
            
            <div class="w-full sm:w-auto">
                <label class="block text-xs font-medium text-slate-500 mb-1">Sort By</label>"""
)

with open(file, "w") as f:
    f.write(content)
