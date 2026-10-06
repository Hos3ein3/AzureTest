import os

file = "src/AzureTest.MVC/Views/FileExplorer/Index.cshtml"
with open(file, "r") as f:
    content = f.read()

target = """        <div class="flex items-center gap-1">
            <a href="?path=@(parentPath ?? "")" class="w-8 h-8 flex items-center justify-center hover:bg-slate-200 rounded text-slate-600 transition @(parentPath == null ? "opacity-50 pointer-events-none" : "")">
                <i class="fa-solid fa-arrow-up"></i>
            </a>
            <a href="?path=" class="w-8 h-8 flex items-center justify-center hover:bg-slate-200 rounded text-slate-600 transition">
                <i class="fa-solid fa-house"></i>
            </a>"""

replacement = """        <div class="flex items-center gap-1">
            <button onclick="window.history.back()" class="w-8 h-8 flex items-center justify-center hover:bg-slate-200 rounded text-slate-600 transition" title="Back">
                <i class="fa-solid fa-arrow-left"></i>
            </button>
            <button onclick="window.history.forward()" class="w-8 h-8 flex items-center justify-center hover:bg-slate-200 rounded text-slate-600 transition" title="Forward">
                <i class="fa-solid fa-arrow-right"></i>
            </button>
            <a href="?path=@(parentPath ?? "")" class="w-8 h-8 flex items-center justify-center hover:bg-slate-200 rounded text-slate-600 transition @(parentPath == null ? "opacity-50 pointer-events-none" : "")" title="Up">
                <i class="fa-solid fa-arrow-up"></i>
            </a>
            <a href="?path=" class="w-8 h-8 flex items-center justify-center hover:bg-slate-200 rounded text-slate-600 transition" title="Home">
                <i class="fa-solid fa-house"></i>
            </a>"""

if "window.history.back()" not in content:
    content = content.replace(target, replacement)
    with open(file, "w") as f:
        f.write(content)
print("File Explorer Patched")
