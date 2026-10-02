import os
for file in ['Create.cshtml', 'Edit.cshtml']:
    path = f'/Users/mohammadhosein/Development/AzureTest/src/AzureTest.MVC/Views/IdeaTracker/{file}'
    with open(path, 'r') as f:
        c = f.read()
    c = c.replace("document.getElementById('Icon').value", "document.getElementById('iconInput').value")
    
    # Fix TinyMCE upload to use Fetch API physically
    tinymce_old_cb = """file_picker_callback: function (cb, value, meta) {
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
                  },"""
    
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
    
    c = c.replace(tinymce_old_cb, tinymce_new_cb)
    with open(path, 'w') as f:
        f.write(c)

