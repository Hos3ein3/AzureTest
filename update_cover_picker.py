files = [
    '/Users/mohammadhosein/Development/AzureTest/src/AzureTest.MVC/Views/IdeaTracker/Create.cshtml',
    '/Users/mohammadhosein/Development/AzureTest/src/AzureTest.MVC/Views/IdeaTracker/Edit.cshtml'
]

import re

for file_path in files:
    with open(file_path, 'r') as f:
        content = f.read()

    # Replace the CoverImage section
    old_cover = r'<div>\s*<label asp-for="CoverImage".*?</label>\s*<input asp-for="CoverImage".*?/>\s*</div>'
    new_cover = """<div>
            <label class="block text-sm font-medium text-slate-700 mb-1">Cover Image (Optional)</label>
            <div class="flex items-center gap-4">
                <!-- Hidden input that actually submits the URL -->
                <input type="hidden" asp-for="CoverImage" id="CoverImageInput" />
                
                <!-- Preview Thumbnail -->
                <div id="coverPreviewContainer" class="w-32 h-20 bg-slate-100 rounded-lg border border-slate-200 overflow-hidden flex items-center justify-center relative group">
                    <img id="coverPreviewImg" src="@(Model?.CoverImage ?? "")" class="w-full h-full object-cover @(string.IsNullOrEmpty(Model?.CoverImage) ? "hidden" : "")" />
                    <i id="coverPlaceholder" class="fa-regular fa-image text-slate-400 text-2xl @(string.IsNullOrEmpty(Model?.CoverImage) ? "" : "hidden")"></i>
                    
                    <button type="button" id="removeCoverBtn" class="absolute inset-0 bg-black/50 text-white flex items-center justify-center opacity-0 group-hover:opacity-100 transition @(string.IsNullOrEmpty(Model?.CoverImage) ? "hidden" : "")" onclick="removeCover()">
                        <i class="fa-solid fa-trash"></i>
                    </button>
                </div>
                
                <div class="flex-1">
                    <div class="flex items-center gap-2 mb-2">
                        <button type="button" onclick="document.getElementById('coverImageFile').click()" class="px-4 py-2 bg-white border border-slate-300 rounded-lg shadow-sm text-sm font-medium text-slate-700 hover:bg-slate-50 transition">
                            <i class="fa-solid fa-upload mr-1"></i> Upload File
                        </button>
                        <span class="text-xs text-slate-500">or</span>
                        <input type="text" id="CoverImageUrl" class="flex-1 px-3 py-2 border border-slate-300 rounded-lg text-sm" placeholder="Paste URL here..." value="@Model?.CoverImage" onchange="updateCoverUrl(this.value)" />
                    </div>
                    <input type="file" id="coverImageFile" accept="image/*" class="hidden" onchange="uploadCoverImage(this)" />
                    <p class="text-xs text-slate-400" id="uploadStatus">JPG, PNG, GIF up to 5MB.</p>
                </div>
            </div>
        </div>"""
        
    content = re.sub(old_cover, new_cover, content, flags=re.DOTALL)
    
    # Add JS script to the end of Scripts block
    js = """
        function updateCoverUrl(url) {
            document.getElementById('CoverImageInput').value = url;
            const img = document.getElementById('coverPreviewImg');
            const placeholder = document.getElementById('coverPlaceholder');
            const removeBtn = document.getElementById('removeCoverBtn');
            
            if (url) {
                img.src = url;
                img.classList.remove('hidden');
                placeholder.classList.add('hidden');
                removeBtn.classList.remove('hidden');
            } else {
                img.src = '';
                img.classList.add('hidden');
                placeholder.classList.remove('hidden');
                removeBtn.classList.add('hidden');
            }
        }

        function removeCover() {
            document.getElementById('CoverImageUrl').value = '';
            updateCoverUrl('');
        }

        async function uploadCoverImage(input) {
            if (!input.files || input.files.length === 0) return;
            const file = input.files[0];
            const status = document.getElementById('uploadStatus');
            
            status.innerHTML = '<i class="fa-solid fa-spinner fa-spin text-brand-500 mr-1"></i> Uploading...';
            
            const formData = new FormData();
            formData.append('image', file);
            
            try {
                const response = await fetch('/IdeaTracker/UploadImage', {
                    method: 'POST',
                    body: formData
                });
                
                if (response.ok) {
                    const data = await response.json();
                    if (data.location) {
                        document.getElementById('CoverImageUrl').value = data.location;
                        updateCoverUrl(data.location);
                        status.innerHTML = '<i class="fa-solid fa-check text-emerald-500 mr-1"></i> Upload successful.';
                    } else {
                        status.innerHTML = '<i class="fa-solid fa-triangle-exclamation text-rose-500 mr-1"></i> Failed to get URL.';
                    }
                } else {
                    status.innerHTML = '<i class="fa-solid fa-triangle-exclamation text-rose-500 mr-1"></i> Upload failed.';
                }
            } catch (err) {
                console.error(err);
                status.innerHTML = '<i class="fa-solid fa-triangle-exclamation text-rose-500 mr-1"></i> Error uploading.';
            }
            
            // clear input
            input.value = '';
        }
    </script>
}"""

    content = content.replace('    </script>\n}', js)
    
    with open(file_path, 'w') as f:
        f.write(content)

