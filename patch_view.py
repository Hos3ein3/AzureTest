file = "src/AzureTest.MVC/Views/FileExplorer/Index.cshtml"
with open(file, "r") as f:
    content = f.read()

# Add format function for Size
size_format = """@functions {
    public string FormatSize(long bytes)
    {
        string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" };
        if (bytes == 0) return "0 B";
        long bytesAbsolute = Math.Abs(bytes);
        int place = Convert.ToInt32(Math.Floor(Math.Log(bytesAbsolute, 1024)));
        double num = Math.Round(bytesAbsolute / Math.Pow(1024, place), 1);
        return (Math.Sign(bytes) * num).ToString() + " " + suf[place];
    }
}
"""

if "@functions {" not in content:
    content = size_format + content

# Update ViewBag declarations
content = content.replace(
    'var parentPath = ViewBag.ParentPath as string;',
    'var parentPath = ViewBag.ParentPath as string;\n    var totalVolume = ViewBag.TotalVolume != null ? FormatSize((long)ViewBag.TotalVolume) : "0 B";'
)

# Add Toggle button to toolbar
toolbar_target = """<button onclick="document.getElementById('fileUploadInput').click()" class="px-3 py-1.5 hover:bg-slate-200 rounded text-sm text-slate-700 flex items-center transition">
                <i class="fa-solid fa-upload text-blue-500 mr-1.5"></i> Upload
            </button>
            <input type="file" id="fileUploadInput" multiple class="hidden" onchange="uploadFiles(this)" />
        </div>
    </div>"""

toolbar_replacement = """<button onclick="document.getElementById('fileUploadInput').click()" class="px-3 py-1.5 hover:bg-slate-200 rounded text-sm text-slate-700 flex items-center transition">
                <i class="fa-solid fa-upload text-blue-500 mr-1.5"></i> Upload
            </button>
            <input type="file" id="fileUploadInput" multiple class="hidden" onchange="uploadFiles(this)" />
            <div class="h-5 w-px bg-slate-300 mx-1"></div>
            <button onclick="togglePreview()" class="w-8 h-8 flex items-center justify-center hover:bg-slate-200 rounded text-slate-600 transition" title="Toggle Preview Pane">
                <i class="fa-solid fa-table-columns"></i>
            </button>
        </div>
    </div>"""

content = content.replace(toolbar_target, toolbar_replacement)

# Update Size rendering
content = content.replace(
    'var sizeStr = item.IsDirectory ? "" : (item.Size / 1024) + " KB";',
    'var sizeStr = FormatSize(item.Size);'
)

# Add Preview Pane to main layout
main_area_target = """        <!-- Files List -->
        <div class="flex-1 overflow-y-auto" id="fileList">"""

main_area_replacement = """        <!-- Files List -->
        <div class="flex-1 overflow-y-auto" id="fileList">"""

# Let's insert the preview pane AFTER the fileList (before status bar)
filelist_end_target = """            </table>
        </div>
    </div>

    <!-- Status Bar -->"""

filelist_end_replacement = """            </table>
        </div>
        
        <!-- Preview Pane -->
        <div id="previewPane" class="w-72 md:w-80 border-l border-slate-200 bg-[#fbfcfd] flex-col shrink-0 hidden">
            <div class="p-3 border-b border-slate-200 bg-slate-50 flex items-center justify-between">
                <span class="font-semibold text-slate-700 text-sm"><i class="fa-regular fa-eye mr-2 text-slate-400"></i>Preview</span>
                <button onclick="togglePreview()" class="text-slate-400 hover:text-slate-600"><i class="fa-solid fa-times"></i></button>
            </div>
            <div id="previewContent" class="flex-1 p-4 flex flex-col items-center justify-center text-center overflow-y-auto">
                <i class="fa-solid fa-file text-4xl text-slate-300 mb-3"></i>
                <p class="text-sm text-slate-500">Select a file to preview.</p>
            </div>
            <div id="previewDetails" class="p-4 border-t border-slate-200 text-xs text-slate-600 bg-white hidden">
                <div class="font-semibold text-sm mb-1 truncate" id="pd-name"></div>
                <div class="mb-1" id="pd-type"></div>
                <div class="mb-1" id="pd-size"></div>
                <div id="pd-date"></div>
            </div>
        </div>
    </div>

    <!-- Status Bar -->"""

content = content.replace(filelist_end_target, filelist_end_replacement)

# Update Status bar
status_target = """    <!-- Status Bar -->
    <div class="bg-slate-100 border-t border-slate-300 px-3 py-1 flex items-center text-xs text-slate-600">
        <span>@Model.Count item(s)</span>
        <span class="mx-2">|</span>
        <span id="selectedCount">0 item(s) selected</span>
    </div>"""

status_replacement = """    <!-- Status Bar -->
    <div class="bg-slate-100 border-t border-slate-300 px-3 py-1 flex items-center justify-between text-xs text-slate-600">
        <div class="flex items-center">
            <span>@Model.Count item(s)</span>
            <span class="mx-2">|</span>
            <span id="selectedCount">0 item(s) selected</span>
        </div>
        <div class="flex items-center font-medium">
            <i class="fa-solid fa-hard-drive text-slate-400 mr-1.5"></i> Total Volume: @totalVolume
        </div>
    </div>"""
content = content.replace(status_target, status_replacement)

# Add Download to Context menu
cm_target = """    <div id="cm-paste" class="px-6 py-1 hover:bg-blue-500 hover:text-white cursor-default" onclick="handleCmAction('paste')">Paste</div>
    <div class="my-1 border-t border-slate-300"></div>"""

cm_replacement = """    <div id="cm-paste" class="px-6 py-1 hover:bg-blue-500 hover:text-white cursor-default" onclick="handleCmAction('paste')">Paste</div>
    <div class="my-1 border-t border-slate-300"></div>
    <div id="cm-download" class="px-6 py-1 hover:bg-blue-500 hover:text-white cursor-default" onclick="handleCmAction('download')">Download</div>
    <div class="my-1 border-t border-slate-300" id="cm-download-sep"></div>"""
content = content.replace(cm_target, cm_replacement)

# Javascript additions
js_target = """        function selectItem(row, e) {"""

js_replacement = """        let previewVisible = localStorage.getItem('fileExplorerPreview') === 'true';
        if (previewVisible) { document.getElementById('previewPane').style.display = 'flex'; }
        
        function togglePreview() {
            previewVisible = !previewVisible;
            document.getElementById('previewPane').style.display = previewVisible ? 'flex' : 'none';
            localStorage.setItem('fileExplorerPreview', previewVisible);
            if (previewVisible && selectedItems.length === 1) updatePreview(document.querySelector('.file-item.selected'));
        }
        
        function formatBytes(bytes, decimals = 1) {
            if (!+bytes) return '0 B';
            const k = 1024;
            const dm = decimals < 0 ? 0 : decimals;
            const sizes = ['B', 'KB', 'MB', 'GB', 'TB', 'PB', 'EB', 'ZB', 'YB'];
            const i = Math.floor(Math.log(bytes) / Math.log(k));
            return `${parseFloat((bytes / Math.pow(k, i)).toFixed(dm))} ${sizes[i]}`;
        }
        
        function updatePreview(row) {
            if (!row) {
                document.getElementById('previewContent').innerHTML = '<i class="fa-solid fa-file text-4xl text-slate-300 mb-3"></i><p class="text-sm text-slate-500">Select a file to preview.</p>';
                document.getElementById('previewDetails').classList.add('hidden');
                return;
            }
            
            const isDir = row.getAttribute('data-isdir') === 'true';
            const path = row.getAttribute('data-path');
            const name = row.getAttribute('data-name');
            const url = '/' + path.split('/').map(p => encodeURIComponent(p)).join('/');
            
            // Update Details
            document.getElementById('previewDetails').classList.remove('hidden');
            document.getElementById('pd-name').innerText = name;
            document.getElementById('pd-type').innerText = row.cells[2].innerText;
            document.getElementById('pd-size').innerText = row.cells[3].innerText;
            document.getElementById('pd-date').innerText = "Modified: " + row.cells[1].innerText;
            
            const content = document.getElementById('previewContent');
            if (isDir) {
                content.innerHTML = '<i class="fa-solid fa-folder text-6xl text-yellow-400 mb-3"></i><p class="text-sm font-medium text-slate-700">' + name + '</p>';
            } else {
                const ext = name.split('.').pop().toLowerCase();
                if (['jpg','jpeg','png','gif','webp','svg'].includes(ext)) {
                    content.innerHTML = `<img src="${url}" class="max-w-full max-h-full object-contain rounded border border-slate-200 shadow-sm" />`;
                } else if (['mp4','webm','ogg'].includes(ext)) {
                    content.innerHTML = `<video controls class="max-w-full max-h-full rounded"><source src="${url}"></video>`;
                } else if (['mp3','wav','ogg'].includes(ext)) {
                    content.innerHTML = `<div class="w-full text-center"><i class="fa-solid fa-music text-4xl text-blue-400 mb-4"></i><audio controls class="w-full"><source src="${url}"></audio></div>`;
                } else if (ext === 'pdf') {
                    content.innerHTML = `<iframe src="${url}" class="w-full h-full border-0"></iframe>`;
                } else {
                    const icon = row.querySelector('td i').className;
                    content.innerHTML = `<i class="${icon} text-6xl mb-3"></i><p class="text-sm font-medium text-slate-700">${name}</p><p class="text-xs text-slate-400 mt-1">No preview available</p>`;
                }
            }
        }
        
        function selectItem(row, e) {"""
content = content.replace(js_target, js_replacement)

js_select_target = """            document.getElementById('selectedCount').innerText = `${selectedItems.length} item(s) selected`;
            
            // Hide context menu if open"""
js_select_replacement = """            let selectedSize = 0;
            if (selectedItems.length > 0) {
                document.querySelectorAll('.file-item.selected').forEach(r => {
                    // Try to parse size from the row text (a bit hacky but works for UI)
                    // or just pass it in data attribute
                });
            }
            document.getElementById('selectedCount').innerText = `${selectedItems.length} item(s) selected`;
            
            if (previewVisible) {
                if (selectedItems.length === 1) updatePreview(row);
                else updatePreview(null);
            }
            
            // Hide context menu if open"""
content = content.replace(js_select_target, js_select_replacement)

js_cm_target = """            document.getElementById('cm-rename').style.display = isItem ? 'block' : 'none';
            document.getElementById('cm-delete').style.display = isItem ? 'block' : 'none';"""

js_cm_replacement = """            document.getElementById('cm-rename').style.display = isItem ? 'block' : 'none';
            document.getElementById('cm-delete').style.display = isItem ? 'block' : 'none';
            
            const isDir = isItem && target.getAttribute('data-isdir') === 'true';
            document.getElementById('cm-download').style.display = (isItem && !isDir) ? 'block' : 'none';
            document.getElementById('cm-download-sep').style.display = (isItem && !isDir) ? 'block' : 'none';"""
content = content.replace(js_cm_target, js_cm_replacement)

js_action_target = """            if (action === 'open') {"""

js_action_replacement = """            if (action === 'download') {
                const path = contextTarget.getAttribute('data-path');
                const url = '/' + path.split('/').map(p => encodeURIComponent(p)).join('/');
                const a = document.createElement('a');
                a.href = url;
                a.download = contextTarget.getAttribute('data-name');
                document.body.appendChild(a);
                a.click();
                document.body.removeChild(a);
            }
            else if (action === 'open') {"""
content = content.replace(js_action_target, js_action_replacement)

with open(file, "w") as f:
    f.write(content)

print("Patched.")
