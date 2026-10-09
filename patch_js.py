def add_js(path):
    with open(path, "r") as f: c = f.read()
    
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
    if "async function uploadMedia" not in c:
        c = c.replace("// --- Tags Logic ---", js_func + "\n        // --- Tags Logic ---")
        with open(path, "w") as f: f.write(c)

add_js("src/AzureTest.MVC/Views/GymCourses/Create.cshtml")
add_js("src/AzureTest.MVC/Views/GymCourses/Edit.cshtml")
print("JS added")
