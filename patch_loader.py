import re

path = "src/AzureTest.MVC/Views/Shared/_Layout.cshtml"
with open(path, "r") as f: c = f.read()

old_html = """    <!-- Global Page Loader Overlay -->
    <div id="globalPageLoader" class="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/60 backdrop-blur-xs transition-opacity duration-200 opacity-0 pointer-events-none" style="display: none;" aria-hidden="true">
        <div class="flex flex-col items-center bg-white/95 rounded-2xl p-6 shadow-2xl border border-slate-100 max-w-xs text-center">
            <div class="relative w-12 h-12 mb-3">
                <div class="absolute inset-0 rounded-full border-4 border-slate-200"></div>
                <div class="absolute inset-0 rounded-full border-4 border-brand-500 border-t-transparent animate-spin"></div>
            </div>
            <span class="text-sm font-semibold text-slate-800">Loading...</span>
            <span class="text-xs text-slate-500 mt-1">Please wait while the page loads</span>
        </div>
    </div>"""

new_html = """    <!-- Global Page Loader Overlay -->
    <div id="globalPageLoader" class="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/60 backdrop-blur-xs transition-opacity duration-200 opacity-0 pointer-events-none" style="display: none;" aria-hidden="true">
        <div class="flex flex-col items-center bg-white/95 rounded-2xl p-6 shadow-2xl border border-slate-100 max-w-xs text-center">
            <div class="relative w-12 h-12 mb-3">
                <div class="absolute inset-0 rounded-full border-4 border-slate-200"></div>
                <div class="absolute inset-0 rounded-full border-4 border-brand-500 border-t-transparent animate-spin"></div>
            </div>
            <span class="text-sm font-semibold text-slate-800">Loading...</span>
            <span class="text-xs text-slate-500 mt-1 mb-4">Please wait while the page loads</span>
            <button id="cancelPageLoadBtn" type="button" class="bg-red-50 text-red-600 hover:bg-red-100 border border-red-200 px-4 py-1.5 rounded-lg text-sm font-bold transition-colors shadow-sm pointer-events-auto">
                <i class="fa-solid fa-xmark mr-1"></i> Cancel
            </button>
        </div>
    </div>"""

c = c.replace(old_html, new_html)

old_js = """            window.addEventListener('pageshow', function () {
                hideLoader();
            });
        })();"""

new_js = """            window.addEventListener('pageshow', function () {
                hideLoader();
            });

            document.getElementById('cancelPageLoadBtn')?.addEventListener('click', function(e) {
                e.preventDefault();
                window.stop(); // Stops the HTTP request, triggering the CancellationToken on the server
                hideLoader();
            });
        })();"""

c = c.replace(old_js, new_js)

with open(path, "w") as f: f.write(c)
