path = "src/AzureTest.MVC/Views/GymCourses/Edit.cshtml"
content = """@model AzureTest.Domain.Documents.Gym.TrainingCourse
@using AzureTest.Domain.Documents.Gym
@{
    ViewData["Title"] = "Edit Course";
    var availableTrainings = ViewBag.Trainings as List<Training> ?? new List<Training>();
}

<div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
    <div class="mb-6 flex items-center justify-between">
        <h2 class="text-2xl font-bold text-gray-900">Edit Course</h2>
        <button type="button" onclick="saveCourse()" class="px-4 py-2 bg-indigo-600 text-white rounded-md hover:bg-indigo-700 shadow-sm font-medium">Save Changes</button>
    </div>

    <div class="bg-white shadow rounded-lg p-6 mb-6">
        <div class="grid grid-cols-1 sm:grid-cols-6 gap-6">
            <div class="sm:col-span-3">
                <label class="block text-sm font-medium text-gray-700 mb-1">Name</label>
                <input id="courseName" type="text" class="block w-full border border-gray-300 rounded-md shadow-sm py-2 px-3 sm:text-sm" />
            </div>

            <div class="sm:col-span-3">
                <label class="block text-sm font-medium text-gray-700 mb-1">Tags</label>
                <div class="flex flex-wrap gap-2 border border-gray-300 p-2 rounded-md bg-white shadow-sm">
                    <div id="tagsContainer" class="flex flex-wrap gap-2"></div>
                    <input id="tagInput" type="text" onkeydown="handleTagInput(event)" class="outline-none border-none focus:ring-0 p-0 text-sm flex-grow min-w-[100px]" placeholder="Add tag and press Enter" />
                </div>
            </div>

            <div class="sm:col-span-6">
                <label class="block text-sm font-medium text-gray-700 mb-1">Description</label>
                <textarea id="courseDescription" rows="5" class="block w-full border border-gray-300 rounded-md shadow-sm py-2 px-3 sm:text-sm"></textarea>
            </div>
        </div>
    </div>

    <div class="mb-4 flex justify-between items-center">
        <h3 class="text-xl font-bold text-gray-900">Course Schedule</h3>
        <button type="button" onclick="addWeek()" class="px-3 py-1.5 bg-indigo-100 text-indigo-700 rounded-md hover:bg-indigo-200 text-sm font-medium">Add Week</button>
    </div>

    <div id="weeksContainer" class="space-y-6"></div>
</div>

@section Scripts {
    <script src="https://cdnjs.cloudflare.com/ajax/libs/tinymce/6.8.2/tinymce.min.js" referrerpolicy="origin"></script>
    <script>
        const availableTrainings = @Html.Raw(System.Text.Json.JsonSerializer.Serialize(availableTrainings, new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase }));
        
        let course = @Html.Raw(System.Text.Json.JsonSerializer.Serialize(Model, new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase }));
        if (!course.tags) course.tags = [];
        if (!course.weeks) course.weeks = [];

        // --- Tags Logic ---
        function renderTags() {
            const container = document.getElementById('tagsContainer');
            container.innerHTML = course.tags.map((tag, idx) => `
                <span class="bg-indigo-100 text-indigo-800 text-xs px-2 py-1 rounded flex items-center">
                    ${tag}
                    <button type="button" onclick="removeTag(${idx})" class="ml-1 text-indigo-600 hover:text-indigo-900 font-bold">&times;</button>
                </span>
            `).join('');
        }

        function handleTagInput(e) {
            if (e.key === 'Enter' || e.key === ',') {
                e.preventDefault();
                const val = e.target.value.trim().replace(',', '');
                if (val && !course.tags.includes(val)) {
                    course.tags.push(val);
                    renderTags();
                }
                e.target.value = '';
            }
        }

        function removeTag(idx) {
            course.tags.splice(idx, 1);
            renderTags();
        }

        // --- Schedule Logic ---
        function addWeek() {
            course.weeks.push({
                weekNumber: course.weeks.length + 1,
                days: []
            });
            renderWeeks();
        }

        function removeWeek(wIdx) {
            course.weeks.splice(wIdx, 1);
            course.weeks.forEach((w, i) => w.weekNumber = i + 1);
            renderWeeks();
        }

        function addDay(wIdx) {
            course.weeks[wIdx].days.push({
                id: crypto.randomUUID(),
                name: 'Day ' + (course.weeks[wIdx].days.length + 1),
                description: '',
                sections: []
            });
            renderWeeks();
        }

        function removeDay(wIdx, dIdx) {
            course.weeks[wIdx].days.splice(dIdx, 1);
            renderWeeks();
        }

        function addSection(wIdx, dIdx) {
            course.weeks[wIdx].days[dIdx].sections.push({
                name: '',
                trainings: []
            });
            renderWeeks();
        }

        function removeSection(wIdx, dIdx, sIdx) {
            course.weeks[wIdx].days[dIdx].sections.splice(sIdx, 1);
            renderWeeks();
        }

        function addTraining(wIdx, dIdx, sIdx) {
            course.weeks[wIdx].days[dIdx].sections[sIdx].trainings.push({
                id: crypto.randomUUID(),
                trainingId: '',
                sets: 0,
                reps: 0,
                weight: 0,
                durationSeconds: 0,
                restSeconds: 0
            });
            renderWeeks();
        }

        function removeTraining(wIdx, dIdx, sIdx, tIdx) {
            course.weeks[wIdx].days[dIdx].sections[sIdx].trainings.splice(tIdx, 1);
            renderWeeks();
        }

        function updateField(objPath, field, value) {
            eval(`course.${objPath}.${field} = value`);
        }

        function renderWeeks() {
            const container = document.getElementById('weeksContainer');
            
            let html = '';
            course.weeks.forEach((week, wIdx) => {
                html += `
                <div class="bg-gray-50 border border-gray-200 rounded-lg p-4 mb-4">
                    <div class="flex justify-between items-center mb-4">
                        <h4 class="text-lg font-bold text-gray-800">Week ${week.weekNumber}</h4>
                        <div>
                            <button type="button" onclick="addDay(${wIdx})" class="px-2 py-1 bg-blue-100 text-blue-700 rounded text-xs mr-2">Add Day</button>
                            <button type="button" onclick="removeWeek(${wIdx})" class="px-2 py-1 bg-red-100 text-red-700 rounded text-xs">Remove Week</button>
                        </div>
                    </div>
                `;

                week.days.forEach((day, dIdx) => {
                    const dayPath = `weeks[${wIdx}].days[${dIdx}]`;
                    html += `
                    <div class="bg-white border border-gray-200 rounded p-4 ml-4 mb-4">
                        <div class="flex justify-between items-center mb-2">
                            <div class="flex space-x-2">
                                <input type="text" value="${day.name}" oninput="updateField('${dayPath}', 'name', this.value)" class="border border-gray-300 rounded px-2 py-1 text-sm" placeholder="Day Name" />
                                <input type="text" value="${day.description}" oninput="updateField('${dayPath}', 'description', this.value)" class="border border-gray-300 rounded px-2 py-1 text-sm w-64" placeholder="Day Description" />
                            </div>
                            <div>
                                <button type="button" onclick="addSection(${wIdx}, ${dIdx})" class="px-2 py-1 bg-green-100 text-green-700 rounded text-xs mr-2">Add Section</button>
                                <button type="button" onclick="removeDay(${wIdx}, ${dIdx})" class="px-2 py-1 bg-red-100 text-red-700 rounded text-xs">Delete Day</button>
                            </div>
                        </div>
                    `;

                    day.sections.forEach((section, sIdx) => {
                        const secPath = `${dayPath}.sections[${sIdx}]`;
                        html += `
                        <div class="bg-gray-50 border border-gray-200 rounded p-3 ml-4 mb-3">
                            <div class="flex justify-between items-center mb-2">
                                <input type="text" value="${section.name}" oninput="updateField('${secPath}', 'name', this.value)" class="border border-gray-300 rounded px-2 py-1 text-sm" placeholder="Section Name (e.g. Warmup)" />
                                <div>
                                    <button type="button" onclick="addTraining(${wIdx}, ${dIdx}, ${sIdx})" class="px-2 py-1 bg-indigo-100 text-indigo-700 rounded text-xs mr-2">Add Exercise</button>
                                    <button type="button" onclick="removeSection(${wIdx}, ${dIdx}, ${sIdx})" class="text-red-500 text-xs">Remove</button>
                                </div>
                            </div>
                        `;

                        if (section.trainings.length > 0) {
                            html += `
                            <table class="min-w-full divide-y divide-gray-200 mt-2">
                                <thead class="bg-gray-100">
                                    <tr>
                                        <th class="px-2 py-1 text-left text-xs font-medium text-gray-500 w-1/3">Exercise</th>
                                        <th class="px-2 py-1 text-left text-xs font-medium text-gray-500 w-16">Sets</th>
                                        <th class="px-2 py-1 text-left text-xs font-medium text-gray-500 w-16">Reps</th>
                                        <th class="px-2 py-1 text-left text-xs font-medium text-gray-500 w-20">Wt (kg)</th>
                                        <th class="px-2 py-1 text-left text-xs font-medium text-gray-500 w-20">Dur (s)</th>
                                        <th class="px-2 py-1 text-left text-xs font-medium text-gray-500 w-20">Rest (s)</th>
                                        <th class="px-2 py-1 w-10"></th>
                                    </tr>
                                </thead>
                                <tbody>
                            `;

                            section.trainings.forEach((tr, tIdx) => {
                                const trPath = `${secPath}.trainings[${tIdx}]`;
                                const options = availableTrainings.map(t => `<option value="${t.id}" ${t.id === tr.trainingId ? 'selected' : ''}>${t.name}</option>`).join('');
                                html += `
                                    <tr>
                                        <td class="px-1 py-1"><select onchange="updateField('${trPath}', 'trainingId', this.value)" class="w-full border-gray-300 rounded text-sm py-1"><option value="">--Select--</option>${options}</select></td>
                                        <td class="px-1 py-1"><input type="number" value="${tr.sets}" oninput="updateField('${trPath}', 'sets', parseInt(this.value)||0)" class="w-full border-gray-300 rounded text-sm py-1 text-center" /></td>
                                        <td class="px-1 py-1"><input type="number" value="${tr.reps}" oninput="updateField('${trPath}', 'reps', parseInt(this.value)||0)" class="w-full border-gray-300 rounded text-sm py-1 text-center" /></td>
                                        <td class="px-1 py-1"><input type="number" step="0.5" value="${tr.weight}" oninput="updateField('${trPath}', 'weight', parseFloat(this.value)||0)" class="w-full border-gray-300 rounded text-sm py-1 text-center" /></td>
                                        <td class="px-1 py-1"><input type="number" value="${tr.durationSeconds}" oninput="updateField('${trPath}', 'durationSeconds', parseInt(this.value)||0)" class="w-full border-gray-300 rounded text-sm py-1 text-center" /></td>
                                        <td class="px-1 py-1"><input type="number" value="${tr.restSeconds}" oninput="updateField('${trPath}', 'restSeconds', parseInt(this.value)||0)" class="w-full border-gray-300 rounded text-sm py-1 text-center" /></td>
                                        <td class="px-1 py-1"><button type="button" onclick="removeTraining(${wIdx}, ${dIdx}, ${sIdx}, ${tIdx})" class="text-red-500 font-bold">&times;</button></td>
                                    </tr>
                                `;
                            });

                            html += `</tbody></table>`;
                        }
                        
                        html += `</div>`; // section
                    });

                    html += `</div>`; // day
                });

                html += `</div>`; // week
            });

            container.innerHTML = html;
        }

        // --- Save Logic ---
        async function saveCourse() {
            course.name = document.getElementById('courseName').value;
            course.description = tinymce.get('courseDescription').getContent();

            try {
                const response = await fetch('@Url.Action("Edit", new { id = Model.Id })', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(course)
                });
                
                const result = await response.json();
                if (result.success) {
                    window.location.href = '@Url.Action("Index")';
                } else {
                    alert('Error: ' + result.message);
                }
            } catch(e) {
                alert('Error saving course: ' + e);
            }
        }

        // --- Init ---
        document.addEventListener('DOMContentLoaded', () => {
            document.getElementById('courseName').value = course.name;
            renderTags();
            renderWeeks();

            tinymce.init({
                selector: '#courseDescription',
                plugins: 'lists link image preview searchreplace wordcount code',
                toolbar: 'undo redo | bold italic | alignleft aligncenter alignright | bullist numlist | link image | code',
                height: 300,
                promotion: false,
                branding: false,
                setup: function (editor) {
                    editor.on('init', function () {
                        editor.setContent(course.description || '');
                    });
                }
            });
        });
    </script>
}
"""
with open(path, "w") as f:
    f.write(content)
print("Vanilla JS rewrite done for Edit")
