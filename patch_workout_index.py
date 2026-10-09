import re

path = "src/AzureTest.MVC/Views/Workout/Index.cshtml"
with open(path, "r") as f: c = f.read()

# I want to add a Details button next to Enroll, or wrap the Course Name in a link.
# Let's add a View Details button.
btn = """<div class="flex gap-2">
                                <a href="@Url.Action("Details", new { id = course.Id })" class="bg-slate-700/50 hover:bg-slate-600 text-slate-300 hover:text-white px-6 py-2 rounded-lg font-semibold transition-colors border border-slate-600">
                                    <i class="fa-solid fa-circle-info mr-2"></i> Details
                                </a>
                                <form method="post" action="@Url.Action("Enroll", new { id = course.Id })">
                                    <button type="submit" class="bg-brand-500 hover:bg-brand-600 text-white px-6 py-2 rounded-lg font-semibold transition-all hover:scale-105 shadow-lg shadow-brand-500/20">
                                        <i class="fa-solid fa-user-plus mr-2"></i> Enroll Now
                                    </button>
                                </form>
                            </div>"""

# Find the Enroll form and replace it.
c = re.sub(r'<form method="post" action="@Url\.Action\("Enroll", new \{ id = course\.Id \}\)">\s*<button.*?</form>', btn, c, flags=re.DOTALL)

with open(path, "w") as f: f.write(c)

print("Index.cshtml patched")
