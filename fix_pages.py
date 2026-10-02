import os

files = [
    '/Users/mohammadhosein/Development/AzureTest/src/AzureTest.MVC/Views/IdeaPage/Create.cshtml',
    '/Users/mohammadhosein/Development/AzureTest/src/AzureTest.MVC/Views/IdeaPage/Edit.cshtml',
    '/Users/mohammadhosein/Development/AzureTest/src/AzureTest.MVC/Views/IdeaSection/Create.cshtml',
    '/Users/mohammadhosein/Development/AzureTest/src/AzureTest.MVC/Views/IdeaSection/Edit.cshtml'
]

tinymce_new_cb = """file_picker_callback: function (cb, value, meta) {
                    var input = document.createElement('input');
                    input.setAttribute('type', 'file');
                    input.setAttribute('accept', 'image/*');
                    input.onchange = async function () {
                        var file = this.files[0];
                        var formData = new FormData();
                        formData.append('image', file);
                        try {
                            const res = await fetch('/IdeaTracker/UploadImage', { method: 'POST', body: formData });
                            if(res.ok) {
                                const data = await res.json();
                                cb(data.location, { title: file.name });
                            }
                        } catch(e) { console.error(e); }
                    };
                    input.click();
                  },"""

for f in files:
    with open(f, 'r') as file:
        c = file.read()
    
    # Regex replacement for file_picker_callback block
    import re
    c = re.sub(r'file_picker_callback:\s*function.*?input\.click\(\);\s*},', tinymce_new_cb, c, flags=re.DOTALL)
    
    with open(f, 'w') as file:
        file.write(c)

