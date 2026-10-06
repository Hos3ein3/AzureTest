import os

file = "src/AzureTest.MVC/Views/Shared/_Layout.cshtml"
with open(file, "r") as f:
    content = f.read()

js_logic = """
<script>
    // Omnibar Logic
    const omnibarModal = document.getElementById('omnibarModal');
    const omnibarInput = document.getElementById('omnibarInput');
    const omnibarResults = document.getElementById('omnibarResults');
    const omnibarEmpty = document.getElementById('omnibarEmpty');
    let searchTimeout = null;

    function toggleOmnibar() {
        if (!omnibarModal) return;
        if (omnibarModal.classList.contains('hidden')) {
            omnibarModal.classList.remove('hidden');
            setTimeout(() => omnibarInput.focus(), 50);
        } else {
            omnibarModal.classList.add('hidden');
            omnibarInput.value = '';
            omnibarResults.innerHTML = '';
            omnibarResults.classList.add('hidden');
            omnibarEmpty.classList.add('hidden');
        }
    }

    document.addEventListener('keydown', (e) => {
        if ((e.metaKey || e.ctrlKey) && e.key === 'k') {
            e.preventDefault();
            toggleOmnibar();
        }
        if (e.key === 'Escape' && omnibarModal && !omnibarModal.classList.contains('hidden')) {
            toggleOmnibar();
        }
    });

    if(omnibarInput) {
        omnibarInput.addEventListener('input', (e) => {
            clearTimeout(searchTimeout);
            const q = e.target.value.trim();
            if (!q) {
                omnibarResults.innerHTML = '';
                omnibarResults.classList.add('hidden');
                omnibarEmpty.classList.add('hidden');
                return;
            }

            searchTimeout = setTimeout(() => {
                fetch('/Search/Omnibar?q=' + encodeURIComponent(q))
                    .then(res => res.json())
                    .then(data => {
                        omnibarResults.innerHTML = '';
                        if (data.length === 0) {
                            omnibarResults.classList.add('hidden');
                            omnibarEmpty.classList.remove('hidden');
                        } else {
                            omnibarEmpty.classList.add('hidden');
                            omnibarResults.classList.remove('hidden');
                            
                            data.forEach(item => {
                                const li = document.createElement('li');
                                li.className = "cursor-pointer select-none hover:bg-slate-100 dark:hover:bg-slate-700/50 transition border-b border-slate-50 dark:border-slate-800/50 last:border-0";
                                li.innerHTML = `
                                    <a href="${item.url}" class="flex items-center px-4 py-3 block w-full h-full">
                                        <div class="flex h-10 w-10 shrink-0 items-center justify-center rounded-lg bg-white dark:bg-slate-800 shadow-sm border border-slate-100 dark:border-slate-700 ${item.color}">
                                            <i class="${item.icon} text-lg"></i>
                                        </div>
                                        <div class="ml-4 flex-auto">
                                            <p class="text-sm font-semibold text-slate-900 dark:text-slate-100">${item.title}</p>
                                            <p class="text-xs text-slate-500 dark:text-slate-400 mt-0.5">${item.type} &middot; ${item.subtitle}</p>
                                        </div>
                                        <i class="fa-solid fa-chevron-right text-slate-300 dark:text-slate-600 text-xs"></i>
                                    </a>
                                `;
                                omnibarResults.appendChild(li);
                            });
                        }
                    });
            }, 250);
        });
    }
</script>
"""

if "function toggleOmnibar()" not in content:
    content = content.replace("</body>", js_logic + "\n</body>")

with open(file, "w") as f:
    f.write(content)
print("Omnibar JS fixed")
