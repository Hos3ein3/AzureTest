import re

for view in ["src/AzureTest.MVC/Views/RawComponent/Create.cshtml", "src/AzureTest.MVC/Views/RawComponent/Edit.cshtml"]:
    with open(view, "r") as f:
        content = f.read()
    
    # We will insert the constraints block right before the closing </div> of the form grid
    constraints_html = """
            <div class="md:col-span-1 pt-4 border-t border-slate-100">
                <h3 class="text-lg font-medium text-slate-800 mb-3">Grid Constraints</h3>
                <div class="grid grid-cols-2 gap-4">
                    <div>
                        <label asp-for="MinWidth" class="block text-sm font-medium text-slate-700">Min Width</label>
                        <input asp-for="MinWidth" type="number" class="mt-1 block w-full rounded-md border-slate-300 shadow-sm focus:border-brand-500 focus:ring-brand-500 sm:text-sm" />
                    </div>
                    <div>
                        <label asp-for="MaxWidth" class="block text-sm font-medium text-slate-700">Max Width</label>
                        <input asp-for="MaxWidth" type="number" class="mt-1 block w-full rounded-md border-slate-300 shadow-sm focus:border-brand-500 focus:ring-brand-500 sm:text-sm" />
                    </div>
                    <div>
                        <label asp-for="MinHeight" class="block text-sm font-medium text-slate-700">Min Height</label>
                        <input asp-for="MinHeight" type="number" class="mt-1 block w-full rounded-md border-slate-300 shadow-sm focus:border-brand-500 focus:ring-brand-500 sm:text-sm" />
                    </div>
                    <div>
                        <label asp-for="MaxHeight" class="block text-sm font-medium text-slate-700">Max Height</label>
                        <input asp-for="MaxHeight" type="number" class="mt-1 block w-full rounded-md border-slate-300 shadow-sm focus:border-brand-500 focus:ring-brand-500 sm:text-sm" />
                    </div>
                </div>
                <div class="mt-4 flex items-center">
                    <input asp-for="IsResizable" type="checkbox" class="h-4 w-4 rounded border-slate-300 text-brand-600 focus:ring-brand-500" />
                    <label asp-for="IsResizable" class="ml-2 block text-sm text-slate-700">Is Resizable</label>
                </div>
            </div>
        </div>

        <div class="flex justify-end pt-5 border-t border-slate-100 gap-3">"""

    content = content.replace('        </div>\n\n        <div class="flex justify-end pt-5 border-t border-slate-100 gap-3">', constraints_html)
    
    with open(view, "w") as f:
        f.write(content)
