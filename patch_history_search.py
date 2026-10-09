import re

path = "src/AzureTest.MVC/Views/Workout/History.cshtml"
with open(path, "r") as f: c = f.read()

header = """        <div>
            <a href="@Url.Action("Index")" class="inline-flex items-center justify-center rounded-lg border border-slate-300 bg-white px-4 py-2 text-sm font-medium text-slate-700 shadow-sm hover:bg-slate-50 transition-colors">
                <i class="fa-solid fa-arrow-left mr-2"></i> Back to Dashboard
            </a>
        </div>
    </div>"""

new_header = """        <div class="flex items-center gap-4">
            <div class="relative">
                <i class="fa-solid fa-magnifying-glass absolute left-3 top-1/2 transform -translate-y-1/2 text-slate-400"></i>
                <input type="text" id="historySearch" placeholder="Search exercises..." class="pl-9 pr-4 py-2 border border-slate-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-brand-500 w-64 shadow-sm" oninput="filterHistory()" />
            </div>
            <a href="@Url.Action("Index")" class="inline-flex items-center justify-center rounded-lg border border-slate-300 bg-white px-4 py-2 text-sm font-medium text-slate-700 shadow-sm hover:bg-slate-50 transition-colors">
                <i class="fa-solid fa-arrow-left mr-2"></i> Back
            </a>
        </div>
    </div>"""
c = c.replace(header, new_header)

card_class = """<div class="bg-white rounded-xl shadow-sm border border-slate-200 overflow-hidden flex flex-col h-full">"""
c = c.replace(card_class, """<div class="workout-history-card bg-white rounded-xl shadow-sm border border-slate-200 overflow-hidden flex flex-col h-full">""")

js = """<script>
    function filterHistory() {
        var query = document.getElementById('historySearch').value.toLowerCase();
        var cards = document.querySelectorAll('.workout-history-card');
        cards.forEach(card => {
            var title = card.querySelector('h4').innerText.toLowerCase();
            if (title.includes(query)) card.style.display = '';
            else card.style.display = 'none';
        });
    }
</script>
"""

c += "\n" + js

with open(path, "w") as f: f.write(c)
