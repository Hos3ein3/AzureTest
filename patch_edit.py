import re

path = "src/AzureTest.MVC/Views/GymCourses/Edit.cshtml"
with open(path, "r") as f: c = f.read()

# Add to UI
ui = """
            <div class="sm:col-span-3">
                <label class="block text-sm font-medium text-gray-700 mb-1">Image URL</label>
                <input id="imagePath" type="text" class="block w-full border border-gray-300 rounded-md shadow-sm py-2 px-3 sm:text-sm" placeholder="https://..." value="@Model.ImagePath" />
            </div>
            <div class="sm:col-span-3">
                <label class="block text-sm font-medium text-gray-700 mb-1">Video URL (Hover Play)</label>
                <input id="videoPath" type="text" class="block w-full border border-gray-300 rounded-md shadow-sm py-2 px-3 sm:text-sm" placeholder="https://..." value="@Model.VideoPath" />
            </div>
"""
c = c.replace('<div class="sm:col-span-6">', ui + '            <div class="sm:col-span-6">')

# Add to JS course object
c = c.replace("tags: initialCourse.tags || [],", "tags: initialCourse.tags || [],\n            imagePath: initialCourse.imagePath || '',\n            videoPath: initialCourse.videoPath || '',")

# Add to save logic
js_save = """course.name = document.getElementById('courseName').value;
            course.imagePath = document.getElementById('imagePath').value;
            course.videoPath = document.getElementById('videoPath').value;"""
c = c.replace("course.name = document.getElementById('courseName').value;", js_save)

with open(path, "w") as f: f.write(c)
