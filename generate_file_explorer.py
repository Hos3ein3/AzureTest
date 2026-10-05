import os

controller_code = """using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.MVC.Controllers;

[Authorize(Roles = "Admin")]
public class FileExplorerController : Controller
{
    private readonly IWebHostEnvironment _env;

    public FileExplorerController(IWebHostEnvironment env)
    {
        _env = env;
    }

    private string GetSafePath(string relativePath)
    {
        if (string.IsNullOrEmpty(relativePath)) relativePath = "";
        relativePath = relativePath.Replace("\\", "/").TrimStart('/');
        var basePath = _env.WebRootPath;
        var fullPath = Path.GetFullPath(Path.Combine(basePath, relativePath));
        if (!fullPath.StartsWith(basePath, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException("Invalid path.");
        }
        return fullPath;
    }

    private string GetRelativePath(string fullPath)
    {
        var basePath = _env.WebRootPath;
        if (!fullPath.StartsWith(basePath, StringComparison.OrdinalIgnoreCase))
            return "";
        return fullPath.Substring(basePath.Length).Replace("\\", "/").TrimStart('/');
    }

    public IActionResult Index(string path = "")
    {
        try
        {
            var fullPath = GetSafePath(path);
            if (!Directory.Exists(fullPath))
            {
                return NotFound("Directory not found.");
            }

            var dirInfo = new DirectoryInfo(fullPath);
            var folders = dirInfo.GetDirectories().Select(d => new FileSystemItem
            {
                Name = d.Name,
                IsDirectory = true,
                Path = GetRelativePath(d.FullName),
                Size = 0,
                Modified = d.LastWriteTime
            }).OrderBy(f => f.Name).ToList();

            var files = dirInfo.GetFiles().Select(f => new FileSystemItem
            {
                Name = f.Name,
                IsDirectory = false,
                Path = GetRelativePath(f.FullName),
                Size = f.Length,
                Modified = f.LastWriteTime,
                Extension = f.Extension
            }).OrderBy(f => f.Name).ToList();

            folders.AddRange(files);

            ViewBag.CurrentPath = GetRelativePath(fullPath);
            ViewBag.ParentPath = GetRelativePath(dirInfo.Parent?.FullName ?? _env.WebRootPath);
            if (fullPath.Equals(_env.WebRootPath, StringComparison.OrdinalIgnoreCase))
            {
                ViewBag.ParentPath = null;
            }

            return View(folders);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost]
    public IActionResult CreateFolder(string path, string folderName)
    {
        try
        {
            var parentPath = GetSafePath(path);
            var newPath = Path.Combine(parentPath, folderName);
            if (!Directory.Exists(newPath))
            {
                Directory.CreateDirectory(newPath);
            }
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost]
    public IActionResult Rename(string path, string newName)
    {
        try
        {
            var fullPath = GetSafePath(path);
            var parent = Path.GetDirectoryName(fullPath);
            var destPath = Path.Combine(parent, newName);

            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Move(fullPath, destPath);
            }
            else if (Directory.Exists(fullPath))
            {
                Directory.Move(fullPath, destPath);
            }
            else
            {
                return NotFound();
            }
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost]
    public IActionResult Delete(string path)
    {
        try
        {
            var fullPath = GetSafePath(path);
            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }
            else if (Directory.Exists(fullPath))
            {
                Directory.Delete(fullPath, true);
            }
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost]
    public IActionResult Paste(string sourcePath, string targetPath, string action) // action = "copy" or "cut"
    {
        try
        {
            var source = GetSafePath(sourcePath);
            var targetDir = GetSafePath(targetPath);
            var itemName = Path.GetFileName(source);
            var dest = Path.Combine(targetDir, itemName);

            if (System.IO.File.Exists(source))
            {
                if (action == "cut") System.IO.File.Move(source, dest);
                else System.IO.File.Copy(source, dest, true);
            }
            else if (Directory.Exists(source))
            {
                if (action == "cut") Directory.Move(source, dest);
                else CopyDirectory(source, dest);
            }
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> Upload(string path, List<IFormFile> files)
    {
        try
        {
            var targetDir = GetSafePath(path);
            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    var filePath = Path.Combine(targetDir, file.FileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                }
            }
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    private void CopyDirectory(string sourceDir, string destinationDir)
    {
        var dir = new DirectoryInfo(sourceDir);
        if (!dir.Exists) throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

        Directory.CreateDirectory(destinationDir);

        foreach (var file in dir.GetFiles())
        {
            var targetFilePath = Path.Combine(destinationDir, file.Name);
            file.CopyTo(targetFilePath);
        }

        foreach (var subDir in dir.GetDirectories())
        {
            var newDestinationDir = Path.Combine(destinationDir, subDir.Name);
            CopyDirectory(subDir.FullName, newDestinationDir);
        }
    }
}

public class FileSystemItem
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public bool IsDirectory { get; set; }
    public long Size { get; set; }
    public DateTime Modified { get; set; }
    public string Extension { get; set; } = string.Empty;
}
"""

view_code = """@model List<AzureTest.MVC.Controllers.FileSystemItem>
@{
    ViewData["Title"] = "File Explorer";
    var currentPath = ViewBag.CurrentPath as string ?? "";
    var parentPath = ViewBag.ParentPath as string;
    
    var pathParts = string.IsNullOrEmpty(currentPath) ? new string[0] : currentPath.Split('/');
}

<div class="bg-white rounded-lg border border-slate-200 shadow-sm overflow-hidden h-[calc(100vh-8rem)] flex flex-col font-sans select-none" id="fileExplorerContainer">
    <!-- Top Toolbar (Windows Style) -->
    <div class="bg-[#f5f6f7] border-b border-slate-300 p-2 flex items-center gap-2 shrink-0">
        <div class="flex items-center gap-1">
            <a href="?path=@(parentPath ?? "")" class="w-8 h-8 flex items-center justify-center hover:bg-slate-200 rounded text-slate-600 transition @(parentPath == null ? "opacity-50 pointer-events-none" : "")">
                <i class="fa-solid fa-arrow-up"></i>
            </a>
            <a href="?path=" class="w-8 h-8 flex items-center justify-center hover:bg-slate-200 rounded text-slate-600 transition">
                <i class="fa-solid fa-house"></i>
            </a>
            <button onclick="window.location.reload()" class="w-8 h-8 flex items-center justify-center hover:bg-slate-200 rounded text-slate-600 transition">
                <i class="fa-solid fa-rotate-right"></i>
            </button>
        </div>
        
        <!-- Address Bar -->
        <div class="flex-1 flex items-center bg-white border border-slate-300 rounded overflow-hidden px-2 py-1 text-sm">
            <i class="fa-solid fa-desktop text-slate-400 mr-2"></i>
            <div class="flex items-center text-slate-700">
                <a href="?path=" class="hover:bg-slate-100 px-1 rounded">wwwroot</a>
                @if (pathParts.Length > 0)
                {
                    var accumPath = "";
                    foreach (var part in pathParts)
                    {
                        accumPath += (accumPath == "" ? "" : "/") + part;
                        <i class="fa-solid fa-chevron-right text-[10px] mx-1 text-slate-400"></i>
                        <a href="?path=@accumPath" class="hover:bg-slate-100 px-1 rounded">@part</a>
                    }
                }
            </div>
        </div>

        <div class="flex items-center gap-1 border-l border-slate-300 pl-2">
            <button onclick="promptCreateFolder()" class="px-3 py-1.5 hover:bg-slate-200 rounded text-sm text-slate-700 flex items-center transition">
                <i class="fa-solid fa-folder-plus text-yellow-500 mr-1.5"></i> New Folder
            </button>
            <button onclick="document.getElementById('fileUploadInput').click()" class="px-3 py-1.5 hover:bg-slate-200 rounded text-sm text-slate-700 flex items-center transition">
                <i class="fa-solid fa-upload text-blue-500 mr-1.5"></i> Upload
            </button>
            <input type="file" id="fileUploadInput" multiple class="hidden" onchange="uploadFiles(this)" />
        </div>
    </div>

    <!-- Main Content Area -->
    <div class="flex-1 flex overflow-hidden bg-white" id="mainArea" oncontextmenu="showContextMenu(event, 'background', null)">
        <!-- Left Sidebar (Quick Access) -->
        <div class="w-48 border-r border-slate-200 bg-[#fbfcfd] overflow-y-auto hidden md:block shrink-0">
            <ul class="p-2 space-y-1 text-sm text-slate-700">
                <li>
                    <a href="?path=" class="flex items-center px-2 py-1.5 hover:bg-blue-50 rounded @(currentPath == "" ? "bg-blue-50 font-medium" : "")">
                        <i class="fa-solid fa-globe text-blue-400 w-5 text-center mr-1"></i> Web Root
                    </a>
                </li>
                <li>
                    <a href="?path=uploads" class="flex items-center px-2 py-1.5 hover:bg-blue-50 rounded @(currentPath.StartsWith("uploads") ? "bg-blue-50 font-medium" : "")">
                        <i class="fa-solid fa-upload text-green-400 w-5 text-center mr-1"></i> Uploads
                    </a>
                </li>
                <li>
                    <a href="?path=css" class="flex items-center px-2 py-1.5 hover:bg-blue-50 rounded @(currentPath.StartsWith("css") ? "bg-blue-50 font-medium" : "")">
                        <i class="fa-brands fa-css3-alt text-blue-500 w-5 text-center mr-1"></i> CSS
                    </a>
                </li>
                <li>
                    <a href="?path=js" class="flex items-center px-2 py-1.5 hover:bg-blue-50 rounded @(currentPath.StartsWith("js") ? "bg-blue-50 font-medium" : "")">
                        <i class="fa-brands fa-js text-yellow-400 w-5 text-center mr-1"></i> JavaScript
                    </a>
                </li>
            </ul>
        </div>

        <!-- Files List -->
        <div class="flex-1 overflow-y-auto" id="fileList">
            <table class="w-full text-sm text-left whitespace-nowrap">
                <thead class="sticky top-0 bg-white border-b border-slate-200 z-10 text-slate-500 font-normal">
                    <tr>
                        <th class="px-4 py-2 font-normal w-1/2">Name</th>
                        <th class="px-4 py-2 font-normal">Date modified</th>
                        <th class="px-4 py-2 font-normal">Type</th>
                        <th class="px-4 py-2 font-normal text-right">Size</th>
                    </tr>
                </thead>
                <tbody>
                    @foreach (var item in Model)
                    {
                        var icon = item.IsDirectory ? "fa-solid fa-folder text-yellow-400" :
                                   item.Extension switch {
                                       ".jpg" or ".png" or ".gif" or ".jpeg" => "fa-solid fa-image text-blue-400",
                                       ".pdf" => "fa-solid fa-file-pdf text-red-500",
                                       ".zip" or ".rar" => "fa-solid fa-file-zipper text-purple-500",
                                       ".txt" or ".md" => "fa-solid fa-file-lines text-slate-400",
                                       ".css" or ".js" or ".html" => "fa-solid fa-file-code text-blue-500",
                                       _ => "fa-solid fa-file text-slate-400"
                                   };
                        var type = item.IsDirectory ? "File folder" : $"{item.Extension.TrimStart('.').ToUpper()} File";
                        var sizeStr = item.IsDirectory ? "" : (item.Size / 1024) + " KB";
                        
                        <tr class="hover:bg-blue-50 cursor-default border-b border-transparent hover:border-slate-100 group file-item" 
                            tabindex="0"
                            data-path="@item.Path" 
                            data-name="@item.Name" 
                            data-isdir="@item.IsDirectory.ToString().ToLower()"
                            ondblclick="openItem('@item.Path', @item.IsDirectory.ToString().ToLower())"
                            oncontextmenu="showContextMenu(event, 'item', this)"
                            onclick="selectItem(this, event)">
                            <td class="px-4 py-1.5 flex items-center">
                                <i class="@icon text-lg mr-2 w-5 text-center"></i>
                                <span class="text-slate-800">@item.Name</span>
                            </td>
                            <td class="px-4 py-1.5 text-slate-500">@item.Modified.ToString("yyyy-MM-dd HH:mm")</td>
                            <td class="px-4 py-1.5 text-slate-500">@type</td>
                            <td class="px-4 py-1.5 text-slate-500 text-right">@sizeStr</td>
                        </tr>
                    }
                    @if (!Model.Any())
                    {
                        <tr>
                            <td colspan="4" class="px-4 py-8 text-center text-slate-400">This folder is empty.</td>
                        </tr>
                    }
                </tbody>
            </table>
        </div>
    </div>

    <!-- Status Bar -->
    <div class="bg-slate-100 border-t border-slate-300 px-3 py-1 flex items-center text-xs text-slate-600">
        <span>@Model.Count item(s)</span>
        <span class="mx-2">|</span>
        <span id="selectedCount">0 item(s) selected</span>
    </div>
</div>

<!-- Windows Context Menu -->
<div id="contextMenu" class="hidden fixed bg-[#eeeeee] border border-slate-400 shadow-[2px_2px_5px_rgba(0,0,0,0.2)] rounded-sm py-1 min-w-[200px] z-50 text-sm font-sans">
    <div id="cm-open" class="px-6 py-1 hover:bg-blue-500 hover:text-white cursor-default" onclick="handleCmAction('open')">Open</div>
    <div class="my-1 border-t border-slate-300"></div>
    <div id="cm-cut" class="px-6 py-1 hover:bg-blue-500 hover:text-white cursor-default" onclick="handleCmAction('cut')">Cut</div>
    <div id="cm-copy" class="px-6 py-1 hover:bg-blue-500 hover:text-white cursor-default" onclick="handleCmAction('copy')">Copy</div>
    <div id="cm-paste" class="px-6 py-1 hover:bg-blue-500 hover:text-white cursor-default" onclick="handleCmAction('paste')">Paste</div>
    <div class="my-1 border-t border-slate-300"></div>
    <div id="cm-rename" class="px-6 py-1 hover:bg-blue-500 hover:text-white cursor-default" onclick="handleCmAction('rename')">Rename</div>
    <div id="cm-delete" class="px-6 py-1 hover:bg-blue-500 hover:text-white cursor-default" onclick="handleCmAction('delete')">Delete</div>
    <div class="my-1 border-t border-slate-300"></div>
    <div id="cm-newfolder" class="px-6 py-1 hover:bg-blue-500 hover:text-white cursor-default" onclick="handleCmAction('newfolder')">New Folder</div>
    <div id="cm-refresh" class="px-6 py-1 hover:bg-blue-500 hover:text-white cursor-default" onclick="handleCmAction('refresh')">Refresh</div>
</div>

@section Scripts {
    <style>
        .file-item:focus, .file-item.selected {
            background-color: #cce8ff !important;
            border-color: #99d1ff !important;
            outline: none;
        }
    </style>
    <script>
        const currentPath = '@Html.Raw(currentPath)';
        let selectedItems = [];
        let clipboard = null; // { action: 'copy'|'cut', path: '...' }

        function selectItem(row, e) {
            e.stopPropagation();
            if (!e.ctrlKey && !e.metaKey) {
                document.querySelectorAll('.file-item').forEach(r => r.classList.remove('selected'));
                selectedItems = [];
            }
            row.classList.add('selected');
            const path = row.getAttribute('data-path');
            if (!selectedItems.includes(path)) selectedItems.push(path);
            document.getElementById('selectedCount').innerText = `${selectedItems.length} item(s) selected`;
            
            // Hide context menu if open
            document.getElementById('contextMenu').classList.add('hidden');
        }

        // Deselect all when clicking background
        document.getElementById('fileExplorerContainer').addEventListener('click', (e) => {
            if (!e.target.closest('tr')) {
                document.querySelectorAll('.file-item').forEach(r => r.classList.remove('selected'));
                selectedItems = [];
                document.getElementById('selectedCount').innerText = `0 item(s) selected`;
            }
            document.getElementById('contextMenu').classList.add('hidden');
        });

        function openItem(path, isDir) {
            if (isDir) {
                window.location.href = '?path=' + encodeURIComponent(path);
            } else {
                window.open('/' + path, '_blank');
            }
        }

        // --- Context Menu Logic ---
        let contextTarget = null;
        let contextType = 'background';

        function showContextMenu(e, type, target) {
            e.preventDefault();
            e.stopPropagation();
            
            contextType = type;
            contextTarget = target;
            
            if (type === 'item') {
                if (!target.classList.contains('selected')) {
                    selectItem(target, {stopPropagation:()=>{} });
                }
            }

            const menu = document.getElementById('contextMenu');
            menu.classList.remove('hidden');
            
            // Toggle visibility of menu items
            const isItem = type === 'item';
            const hasClipboard = clipboard !== null;
            
            document.getElementById('cm-open').style.display = isItem ? 'block' : 'none';
            document.getElementById('cm-cut').style.display = isItem ? 'block' : 'none';
            document.getElementById('cm-copy').style.display = isItem ? 'block' : 'none';
            document.getElementById('cm-rename').style.display = isItem ? 'block' : 'none';
            document.getElementById('cm-delete').style.display = isItem ? 'block' : 'none';
            
            document.getElementById('cm-paste').style.display = (!isItem && hasClipboard) ? 'block' : 'none';
            document.getElementById('cm-newfolder').style.display = !isItem ? 'block' : 'none';
            document.getElementById('cm-refresh').style.display = !isItem ? 'block' : 'none';

            // Positioning
            let x = e.clientX;
            let y = e.clientY;
            
            // keep within bounds
            if (x + menu.offsetWidth > window.innerWidth) x = window.innerWidth - menu.offsetWidth;
            if (y + menu.offsetHeight > window.innerHeight) y = window.innerHeight - menu.offsetHeight;
            
            menu.style.left = x + 'px';
            menu.style.top = y + 'px';
        }

        window.addEventListener('click', () => {
            document.getElementById('contextMenu').classList.add('hidden');
        });

        function handleCmAction(action) {
            document.getElementById('contextMenu').classList.add('hidden');
            
            if (action === 'open') {
                const path = contextTarget.getAttribute('data-path');
                const isDir = contextTarget.getAttribute('data-isdir') === 'true';
                openItem(path, isDir);
            } 
            else if (action === 'copy' || action === 'cut') {
                clipboard = { action: action, path: contextTarget.getAttribute('data-path') };
                // Optional UI hint for cut
            }
            else if (action === 'paste') {
                if (!clipboard) return;
                fetch('/FileExplorer/Paste', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
                    body: `sourcePath=${encodeURIComponent(clipboard.path)}&targetPath=${encodeURIComponent(currentPath)}&action=${clipboard.action}`
                }).then(res => {
                    if (res.ok) window.location.reload();
                    else alert('Paste failed');
                });
            }
            else if (action === 'delete') {
                if (confirm('Are you sure you want to permanently delete this item?')) {
                    const path = contextTarget.getAttribute('data-path');
                    fetch('/FileExplorer/Delete', {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
                        body: `path=${encodeURIComponent(path)}`
                    }).then(res => {
                        if (res.ok) window.location.reload();
                        else alert('Delete failed');
                    });
                }
            }
            else if (action === 'rename') {
                const oldName = contextTarget.getAttribute('data-name');
                const newName = prompt('Enter new name:', oldName);
                if (newName && newName !== oldName) {
                    const path = contextTarget.getAttribute('data-path');
                    fetch('/FileExplorer/Rename', {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
                        body: `path=${encodeURIComponent(path)}&newName=${encodeURIComponent(newName)}`
                    }).then(res => {
                        if (res.ok) window.location.reload();
                        else alert('Rename failed');
                    });
                }
            }
            else if (action === 'newfolder') {
                promptCreateFolder();
            }
            else if (action === 'refresh') {
                window.location.reload();
            }
        }

        function promptCreateFolder() {
            const name = prompt('Enter folder name:');
            if (name) {
                fetch('/FileExplorer/CreateFolder', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
                    body: `path=${encodeURIComponent(currentPath)}&folderName=${encodeURIComponent(name)}`
                }).then(res => {
                    if (res.ok) window.location.reload();
                    else alert('Folder creation failed');
                });
            }
        }

        function uploadFiles(input) {
            if (!input.files || input.files.length === 0) return;
            const formData = new FormData();
            formData.append('path', currentPath);
            for (let i = 0; i < input.files.length; i++) {
                formData.append('files', input.files[i]);
            }
            
            fetch('/FileExplorer/Upload', {
                method: 'POST',
                body: formData
            }).then(res => {
                if (res.ok) window.location.reload();
                else alert('Upload failed');
            });
        }
    </script>
}
"""

os.makedirs("src/AzureTest.MVC/Controllers", exist_ok=True)
os.makedirs("src/AzureTest.MVC/Views/FileExplorer", exist_ok=True)

with open("src/AzureTest.MVC/Controllers/FileExplorerController.cs", "w") as f:
    f.write(controller_code)
    
with open("src/AzureTest.MVC/Views/FileExplorer/Index.cshtml", "w") as f:
    f.write(view_code)

