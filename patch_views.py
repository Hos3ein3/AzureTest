import re

def patch_view(path):
    with open(path, "r") as f: c = f.read()

    # Replace Image URL block
    old_img = """<label class="block text-sm font-medium text-gray-700 mb-1">Image URL</label>
                <input id="imagePath" type="text" class="block w-full border border-gray-300 rounded-md shadow-sm py-2 px-3 sm:text-sm" placeholder="https://..." """
    if "value=\"@Model.ImagePath\"" in c:
        old_img += 'value="@Model.ImagePath" />'
    else:
        old_img += '/>'

    new_img = old_img.replace('<input id="imagePath"', '<div class="flex gap-2">\n                    <input id="imagePath"').replace('/>', '/>\n                    <label class="cursor-pointer bg-gray-100 hover:bg-gray-200 border border-gray-300 rounded-md px-3 py-2 text-sm font-medium text-gray-700 flex items-center shadow-sm whitespace-nowrap">\n                        Browse PC\n                        <input type="file" class="hidden" accept="image/*" onchange="uploadMedia(this, \'imagePath\')" />\n                    </label>\n                </div>')
    
    c = c.replace(old_img, new_img)

    # Replace Video URL block
    old_vid = """<label class="block text-sm font-medium text-gray-700 mb-1">Video URL (Hover Play)</label>
                <input id="videoPath" type="text" class="block w-full border border-gray-300 rounded-md shadow-sm py-2 px-3 sm:text-sm" placeholder="https://..." """
    if "value=\"@Model.VideoPath\"" in c:
        old_vid += 'value="@Model.VideoPath" />'
    else:
        old_vid += '/>'

    new_vid = old_vid.replace('<input id="videoPath"', '<div class="flex gap-2">\n                    <input id="videoPath"').replace('/>', '/>\n                    <label class="cursor-pointer bg-gray-100 hover:bg-gray-200 border border-gray-300 rounded-md px-3 py-2 text-sm font-medium text-gray-700 flex items-center shadow-sm whitespace-nowrap">\n                        Browse PC\n                        <input type="file" class="hidden" accept="video/*" onchange="uploadMedia(this, \'videoPath\')" />\n                    </label>\n                </div>')
    
    c = c.replace(old_vid, new_vid)

    # Add uploadMedia JS function
    js_func = """
        async function uploadMedia(inputElement, targetInputId) {
            if (!inputElement.files || inputElement.files.length === 0) return;
            const file = inputElement.files[0];
            const formData = new FormData();
            formData.append('file', file);
            
            const btn = inputElement.parentElement;
            const originalText = btn.innerHTML;
            btn.innerHTML = 'Uploading...';
            btn.classList.add('opacity-50', 'cursor-not-allowed');
            
            try {
                const response = await fetch('@Url.Action("UploadMedia")', {
                    method: 'POST',
                    body: formData
                });
                const result = await response.json();
                if (result.success) {
                    document.getElementById(targetInputId).value = result.url;
                } else {
                    alert('Upload failed: ' + result.message);
                }
            } catch (e) {
                alert('Upload error: ' + e);
            } finally {
                btn.innerHTML = originalText;
                btn.classList.remove('opacity-50', 'cursor-not-allowed');
                inputElement.value = ''; // reset so it can be selected again
            }
        }
    """
    
    if "uploadMedia(" not in c:
        c = c.replace("// --- Tags Logic ---", js_func + "\n        // --- Tags Logic ---")

    with open(path, "w") as f: f.write(c)

patch_view("src/AzureTest.MVC/Views/GymCourses/Create.cshtml")
patch_view("src/AzureTest.MVC/Views/GymCourses/Edit.cshtml")
print("Views patched")
