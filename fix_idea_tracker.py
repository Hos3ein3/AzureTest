import re
import os

files = [
    '/Users/mohammadhosein/Development/AzureTest/src/AzureTest.MVC/Views/IdeaTracker/Create.cshtml',
    '/Users/mohammadhosein/Development/AzureTest/src/AzureTest.MVC/Views/IdeaTracker/Edit.cshtml'
]

file_picker_code = """
                file_picker_callback: function (cb, value, meta) {
                    var input = document.createElement('input');
                    input.setAttribute('type', 'file');
                    input.setAttribute('accept', 'image/*');

                    input.onchange = function () {
                      var file = this.files[0];
                      var reader = new FileReader();
                      
                      reader.onload = function () {
                        var id = 'blobid' + (new Date()).getTime();
                        var blobCache =  tinymce.activeEditor.editorUpload.blobCache;
                        var base64 = reader.result.split(',')[1];
                        var blobInfo = blobCache.create(id, file, base64);
                        blobCache.add(blobInfo);

                        cb(blobInfo.blobUri(), { title: file.name });
                      };
                      reader.readAsDataURL(file);
                    };

                    input.click();
                  },
"""

for filepath in files:
    if not os.path.exists(filepath): continue
    with open(filepath, 'r') as f:
        content = f.read()
    
    # Fix Icon JS
    content = content.replace("document.getElementById('iconInput').value", "document.getElementById('Icon').value")
    # Fix Cover JS
    content = content.replace("document.getElementById('CoverImageInput')", "document.getElementById('CoverImage')")
    # Fix TinyMCE file_picker
    if "file_picker_callback:" not in content:
        content = content.replace("file_picker_types: 'image',", "file_picker_types: 'image',\n" + file_picker_code)
    
    with open(filepath, 'w') as f:
        f.write(content)
