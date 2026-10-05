import re

file = "src/AzureTest.MVC/Views/RawComponent/Edit.cshtml"
with open(file, "r") as f:
    content = f.read()

content = content.replace(
"""                <textarea asp-for="JsonConfiguration" rows="10" class="mt-1 block w-full font-mono rounded-md border-slate-300 shadow-sm focus:border-brand-500 focus:ring-brand-500 sm:text-sm bg-slate-50 text-slate-800 p-3" placeholder='{ "chartType": "bar" }'></textarea>
                <span asp-validation-for="JsonConfiguration" class="text-xs text-red-500"></span>
            </div>""",
"""                <textarea asp-for="JsonConfiguration" rows="10" class="mt-1 block w-full font-mono rounded-md border-slate-300 shadow-sm focus:border-brand-500 focus:ring-brand-500 sm:text-sm bg-slate-50 text-slate-800 p-3" placeholder='{ "chartType": "bar" }'></textarea>
                <span asp-validation-for="JsonConfiguration" class="text-xs text-red-500"></span>
                
                <div class="mt-4">
                    <label asp-for="ViewName" class="block text-sm font-medium text-slate-700">View Name (Optional)</label>
                    <p class="text-xs text-slate-500 mb-1">Specifies the Partial View that will render this JSON config (e.g. "Widgets/_ResumeStatsA").</p>
                    <input asp-for="ViewName" class="mt-1 block w-full rounded-md border-slate-300 shadow-sm focus:border-brand-500 focus:ring-brand-500 sm:text-sm" />
                </div>
            </div>"""
)

with open(file, "w") as f:
    f.write(content)
