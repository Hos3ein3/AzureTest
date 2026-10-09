import re

path = "src/AzureTest.MVC/Views/Workout/Index.cshtml"
with open(path, "r") as f: c = f.read()

# Replace the header div
old_header = """<div class="mb-8">
    <h1 class="text-3xl font-bold text-gray-900 dark:text-white mb-2">Workout Dashboard</h1>
    <p class="text-gray-600 dark:text-gray-400">Track your progress and discover new training courses.</p>
</div>"""

new_header = """<div class="mb-8 flex flex-col md:flex-row justify-between items-start md:items-center">
    <div>
        <h1 class="text-3xl font-bold text-gray-900 dark:text-white mb-2">Workout Dashboard</h1>
        <p class="text-gray-600 dark:text-gray-400">Track your progress and discover new training courses.</p>
    </div>
    <div class="mt-4 md:mt-0">
        <a href="@Url.Action("History")" class="inline-flex items-center justify-center rounded-lg border border-slate-300 bg-white px-4 py-2.5 text-sm font-medium text-slate-700 shadow-sm hover:bg-slate-50 transition-colors">
            <i class="fa-solid fa-clock-rotate-left mr-2"></i> View Exercise History
        </a>
    </div>
</div>"""

c = c.replace(old_header, new_header)

with open(path, "w") as f: f.write(c)
print("Workout Index patched")
