import re

for path in ["src/AzureTest.MVC/Views/GymAdmin/CreateTraining.cshtml", "src/AzureTest.MVC/Views/GymAdmin/EditTraining.cshtml"]:
    with open(path, "r") as f:
        content = f.read()
    
    # Replace textarea with TinyMCE id
    content = re.sub(
        r'<textarea asp-for="Description" rows="3" class=".*?"></textarea>',
        r'<textarea asp-for="Description" id="Description"></textarea>',
        content
    )
    
    # Add Scripts section
    scripts = """
@section Scripts {
    <script src="https://cdnjs.cloudflare.com/ajax/libs/tinymce/6.8.2/tinymce.min.js" referrerpolicy="origin"></script>
    <script>
        document.addEventListener('DOMContentLoaded', () => {
            tinymce.init({
                selector: '#Description',
                plugins: 'lists link image preview searchreplace wordcount code',
                toolbar: 'undo redo | bold italic | alignleft aligncenter alignright | bullist numlist | link image | code',
                height: 300,
                promotion: false,
                branding: false,
                setup: function(editor) {
                    editor.on('change', function() {
                        editor.save();
                    });
                }
            });
        });
    </script>
}
"""
    if "@section Scripts" not in content:
        content += scripts
    
    with open(path, "w") as f:
        f.write(content)
print("Trainings updated")
