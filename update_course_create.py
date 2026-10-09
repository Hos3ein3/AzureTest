import re

path = "src/AzureTest.MVC/Views/GymCourses/Create.cshtml"
with open(path, "r") as f:
    content = f.read()

# 1. Update Tags Input UI
tags_ui = """
            <div class="sm:col-span-3">
                <label class="block text-sm font-medium text-gray-700 mb-1">Tags</label>
                <div class="flex flex-wrap gap-2 border border-gray-300 p-2 rounded-md bg-white shadow-sm focus-within:ring-1 focus-within:ring-indigo-500 focus-within:border-indigo-500">
                    <span v-for="(tag, index) in course.tags" :key="index" class="bg-indigo-100 text-indigo-800 text-xs px-2 py-1 rounded flex items-center">
                        {{ tag }}
                        <button @click.prevent="removeTag(index)" class="ml-1 text-indigo-600 hover:text-indigo-900 font-bold">&times;</button>
                    </span>
                    <input v-model="newTag" @keydown.enter.prevent="addTag" @keydown.comma.prevent="addTag" @blur="addTag" class="outline-none border-none focus:ring-0 p-0 text-sm flex-grow min-w-[100px]" placeholder="Add tag and press Enter" />
                </div>
            </div>
"""
content = re.sub(
    r'<div class="sm:col-span-3">\s*<label class="block text-sm font-medium text-gray-700">Tags \(comma separated\)</label>\s*<input v-model="tagsInput".*?</div>',
    tags_ui,
    content,
    flags=re.DOTALL
)

# 2. Update Description UI to use TinyMCE
desc_ui = """
            <div class="sm:col-span-6">
                <label class="block text-sm font-medium text-gray-700 mb-1">Description</label>
                <textarea id="courseDescription" rows="5" class="mt-1 block w-full border border-gray-300 rounded-md shadow-sm py-2 px-3 sm:text-sm"></textarea>
            </div>
"""
content = re.sub(
    r'<div class="sm:col-span-6">\s*<label class="block text-sm font-medium text-gray-700">Description</label>\s*<textarea v-model="course.description".*?</div>',
    desc_ui,
    content,
    flags=re.DOTALL
)

# 3. Add TinyMCE script and Vue logic
scripts_replacement = """
    <script src="https://unpkg.com/vue@3/dist/vue.global.prod.js"></script>
    <script src="https://cdnjs.cloudflare.com/ajax/libs/tinymce/6.8.2/tinymce.min.js" referrerpolicy="origin"></script>
    <script>
        const { createApp, ref, computed, onMounted } = Vue;

        createApp({
            setup() {
                const availableTrainings = @Html.Raw(System.Text.Json.JsonSerializer.Serialize(availableTrainings, new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase }));
                
                const course = ref({
                    name: '',
                    description: '',
                    tags: [],
                    weeks: []
                });

                const newTag = ref('');

                const addTag = () => {
                    const val = newTag.value.trim();
                    if (val && !course.value.tags.includes(val)) {
                        course.value.tags.push(val);
                    }
                    newTag.value = '';
                };

                const removeTag = (index) => {
                    course.value.tags.splice(index, 1);
                };

                onMounted(() => {
                    tinymce.init({
                        selector: '#courseDescription',
                        plugins: 'lists link image preview searchreplace wordcount code',
                        toolbar: 'undo redo | bold italic | alignleft aligncenter alignright | bullist numlist | link image | code',
                        height: 300,
                        promotion: false,
                        branding: false,
                        setup: function(editor) {
                            editor.on('change', function() {
                                course.value.description = editor.getContent();
                            });
                            editor.on('keyup', function() {
                                course.value.description = editor.getContent();
                            });
                        }
                    });
                });
"""
content = re.sub(
    r'<script src="https://unpkg\.com/vue@3/dist/vue\.global\.prod\.js"></script>\s*<script>\s*const { createApp, ref, computed } = Vue;\s*createApp\({\s*setup\(\) {\s*const availableTrainings = @Html\.Raw.*?;\s*const course = ref\({\s*name: \'\',\s*description: \'\',\s*tags: \[\],\s*weeks: \[\]\s*}\);\s*const tagsInput = ref\(\'\'\);',
    scripts_replacement,
    content,
    flags=re.DOTALL
)

# 4. Fix SaveCourse URL in Vue and remove tagsInput usage
content = content.replace("fetch('@Url.Action(\"CourseCreate\")')", "fetch('@Url.Action(\"Create\")')")
content = re.sub(
    r'if \(tagsInput\.value\) {.*?}',
    '',
    content,
    flags=re.DOTALL
)
content = content.replace("tagsInput,", "newTag, addTag, removeTag,")

with open(path, "w") as f:
    f.write(content)
print("Create.cshtml updated")
