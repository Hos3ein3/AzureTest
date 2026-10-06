import os

file = "src/AzureTest.MVC/Views/Shared/_Layout.cshtml"
with open(file, "r") as f:
    content = f.read()

# 1. Add Search bar to Top Nav
nav_target = """<div class="flex items-center space-x-6">
                <div class="text-sm font-medium text-slate-500">
                    <i class="fa-regular fa-calendar mr-1.5"></i>
                    @DateTime.Now.ToString("dd MMM yyyy")
                </div>"""

nav_replacement = """<div class="flex items-center space-x-6">
                <button onclick="toggleOmnibar()" class="hidden md:flex items-center space-x-2 bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700 px-3 py-1.5 rounded-lg text-sm text-slate-400 hover:text-slate-500 hover:border-slate-300 transition-colors shadow-sm w-48 focus:outline-none focus:ring-2 focus:ring-brand-500">
                    <i class="fa-solid fa-magnifying-glass"></i>
                    <span class="flex-1 text-left">Search...</span>
                    <span class="text-xs border border-slate-200 dark:border-slate-700 rounded px-1.5 py-0.5 bg-slate-50 dark:bg-slate-900 font-sans tracking-widest">⌘K</span>
                </button>
                
                <div class="hidden sm:block text-sm font-medium text-slate-500">
                    <i class="fa-regular fa-calendar mr-1.5"></i>
                    @DateTime.Now.ToString("dd MMM yyyy")
                </div>"""

if "toggleOmnibar()" not in content:
    content = content.replace(nav_target, nav_replacement)

# 2. Add Modal HTML before </body>
modal_html = """
    <!-- Omnibar Modal -->
    <div id="omnibarModal" class="fixed inset-0 z-[100] hidden">
        <div class="fixed inset-0 bg-slate-900/50 backdrop-blur-sm transition-opacity" onclick="toggleOmnibar()"></div>
        <div class="fixed inset-0 z-10 overflow-y-auto p-4 sm:p-6 md:p-20">
            <div class="mx-auto max-w-2xl transform divide-y divide-slate-100 dark:divide-slate-700 overflow-hidden rounded-xl bg-white dark:bg-slate-800 shadow-2xl ring-1 ring-black ring-opacity-5 transition-all">
                <div class="relative">
                    <i class="fa-solid fa-magnifying-glass pointer-events-none absolute top-4 left-4 h-5 w-5 text-slate-400"></i>
                    <input type="text" id="omnibarInput" class="h-14 w-full border-0 bg-transparent pl-12 pr-4 text-slate-900 dark:text-slate-100 placeholder-slate-400 focus:ring-0 sm:text-lg focus:outline-none" placeholder="Search tasks, ideas, events, and features..." autocomplete="off">
                </div>
                <ul id="omnibarResults" class="max-h-96 scroll-py-2 overflow-y-auto py-2 text-sm text-slate-800 dark:text-slate-200 hidden" role="listbox">
                </ul>
                <div id="omnibarEmpty" class="py-14 px-6 text-center text-sm sm:px-14 hidden">
                    <i class="fa-regular fa-face-frown text-4xl text-slate-300 dark:text-slate-600 mb-4"></i>
                    <p class="text-slate-900 dark:text-slate-100 font-semibold text-lg">No results found</p>
                    <p class="text-slate-500 dark:text-slate-400 mt-1">We couldn't find anything matching that search term.</p>
                </div>
                <div class="flex flex-wrap items-center bg-slate-50 dark:bg-slate-900/50 py-3 px-4 text-xs text-slate-500 dark:text-slate-400">
                    <span class="flex items-center mr-4"><kbd class="bg-white dark:bg-slate-800 border border-slate-200 dark:border-slate-700 rounded px-1.5 py-0.5 font-sans font-medium mr-1.5 shadow-sm text-slate-400">esc</kbd> to close</span>
                </div>
            </div>
        </div>
    </div>
"""

# 3. Add JS
js_logic = """
        // Omnibar Logic
        const omnibarModal = document.getElementById('omnibarModal');
        const omnibarInput = document.getElementById('omnibarInput');
        const omnibarResults = document.getElementById('omnibarResults');
        const omnibarEmpty = document.getElementById('omnibarEmpty');
        let searchTimeout = null;

        function toggleOmnibar() {
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
            if (e.key === 'Escape' && !omnibarModal.classList.contains('hidden')) {
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
"""

if "omnibarModal" not in content:
    content = content.replace("<!-- SignalR & Notifications -->", modal_html + "\n    <!-- SignalR & Notifications -->")
    content = content.replace("</script>\n</body>", js_logic + "\n</script>\n</body>")

with open(file, "w") as f:
    f.write(content)

print("Omnibar Patched")
