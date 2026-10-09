import re

files = ["src/AzureTest.MVC/Views/GymCourses/Create.cshtml", "src/AzureTest.MVC/Views/GymCourses/Edit.cshtml"]

for path in files:
    with open(path, "r") as f:
        content = f.read()

    # We want to replace the whole // --- Schedule Logic --- to // --- Save Logic --- block
    # Or just replace the JS functions and renderWeeks().
    
    # We can use regex to find the script block and rewrite the relevant parts.
    
    # The functions block to replace:
    old_logic_start = "// --- Schedule Logic ---"
    old_logic_end = "// --- Save Logic ---"
    
    new_logic = """// --- Schedule Logic ---
        function addWeek() {
            course.weeks.push({ weekNumber: course.weeks.length + 1, days: [] });
            renderWeeks();
        }
        function removeWeek(wIdx) {
            course.weeks.splice(wIdx, 1);
            course.weeks.forEach((w, i) => w.weekNumber = i + 1);
            renderWeeks();
        }
        function duplicateWeek(wIdx) {
            let cloned = JSON.parse(JSON.stringify(course.weeks[wIdx]));
            cloned.days.forEach(d => {
                d.id = crypto.randomUUID();
                d.sections.forEach(s => s.trainings.forEach(t => t.id = crypto.randomUUID()));
            });
            course.weeks.splice(wIdx + 1, 0, cloned);
            course.weeks.forEach((w, i) => w.weekNumber = i + 1);
            renderWeeks();
        }
        function moveWeek(wIdx, dir) {
            if (wIdx + dir < 0 || wIdx + dir >= course.weeks.length) return;
            let temp = course.weeks[wIdx];
            course.weeks[wIdx] = course.weeks[wIdx + dir];
            course.weeks[wIdx + dir] = temp;
            course.weeks.forEach((w, i) => w.weekNumber = i + 1);
            renderWeeks();
        }

        function addDay(wIdx) {
            course.weeks[wIdx].days.push({ id: crypto.randomUUID(), name: 'Day ' + (course.weeks[wIdx].days.length + 1), description: '', sections: [] });
            renderWeeks();
        }
        function removeDay(wIdx, dIdx) {
            course.weeks[wIdx].days.splice(dIdx, 1);
            renderWeeks();
        }
        function duplicateDay(wIdx, dIdx) {
            let cloned = JSON.parse(JSON.stringify(course.weeks[wIdx].days[dIdx]));
            cloned.id = crypto.randomUUID();
            cloned.sections.forEach(s => s.trainings.forEach(t => t.id = crypto.randomUUID()));
            course.weeks[wIdx].days.splice(dIdx + 1, 0, cloned);
            renderWeeks();
        }
        function moveDay(wIdx, dIdx, dir) {
            let days = course.weeks[wIdx].days;
            if (dIdx + dir < 0 || dIdx + dir >= days.length) return;
            let temp = days[dIdx];
            days[dIdx] = days[dIdx + dir];
            days[dIdx + dir] = temp;
            renderWeeks();
        }

        function addSection(wIdx, dIdx) {
            course.weeks[wIdx].days[dIdx].sections.push({ name: '', trainings: [] });
            renderWeeks();
        }
        function removeSection(wIdx, dIdx, sIdx) {
            course.weeks[wIdx].days[dIdx].sections.splice(sIdx, 1);
            renderWeeks();
        }
        function duplicateSection(wIdx, dIdx, sIdx) {
            let cloned = JSON.parse(JSON.stringify(course.weeks[wIdx].days[dIdx].sections[sIdx]));
            cloned.trainings.forEach(t => t.id = crypto.randomUUID());
            course.weeks[wIdx].days[dIdx].sections.splice(sIdx + 1, 0, cloned);
            renderWeeks();
        }
        function moveSection(wIdx, dIdx, sIdx, dir) {
            let secs = course.weeks[wIdx].days[dIdx].sections;
            if (sIdx + dir < 0 || sIdx + dir >= secs.length) return;
            let temp = secs[sIdx];
            secs[sIdx] = secs[sIdx + dir];
            secs[sIdx + dir] = temp;
            renderWeeks();
        }

        function addTraining(wIdx, dIdx, sIdx) {
            course.weeks[wIdx].days[dIdx].sections[sIdx].trainings.push({
                id: crypto.randomUUID(), trainingId: '', sets: 0, reps: 0, weight: 0, durationSeconds: 0, restSeconds: 0
            });
            renderWeeks();
        }
        function removeTraining(wIdx, dIdx, sIdx, tIdx) {
            course.weeks[wIdx].days[dIdx].sections[sIdx].trainings.splice(tIdx, 1);
            renderWeeks();
        }
        function duplicateTraining(wIdx, dIdx, sIdx, tIdx) {
            let cloned = JSON.parse(JSON.stringify(course.weeks[wIdx].days[dIdx].sections[sIdx].trainings[tIdx]));
            cloned.id = crypto.randomUUID();
            course.weeks[wIdx].days[dIdx].sections[sIdx].trainings.splice(tIdx + 1, 0, cloned);
            renderWeeks();
        }
        function moveTraining(wIdx, dIdx, sIdx, tIdx, dir) {
            let trs = course.weeks[wIdx].days[dIdx].sections[sIdx].trainings;
            if (tIdx + dir < 0 || tIdx + dir >= trs.length) return;
            let temp = trs[tIdx];
            trs[tIdx] = trs[tIdx + dir];
            trs[tIdx + dir] = temp;
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
                        <div class="flex items-center space-x-2">
                            <h4 class="text-lg font-bold text-gray-800">Week ${week.weekNumber}</h4>
                            <button type="button" onclick="moveWeek(${wIdx}, -1)" class="text-gray-400 hover:text-gray-600" title="Move Up"><i class="fa-solid fa-arrow-up"></i></button>
                            <button type="button" onclick="moveWeek(${wIdx}, 1)" class="text-gray-400 hover:text-gray-600" title="Move Down"><i class="fa-solid fa-arrow-down"></i></button>
                        </div>
                        <div>
                            <button type="button" onclick="addDay(${wIdx})" class="px-2 py-1 bg-blue-100 text-blue-700 rounded text-xs font-medium mr-1">Add Day</button>
                            <button type="button" onclick="duplicateWeek(${wIdx})" class="px-2 py-1 bg-yellow-100 text-yellow-700 rounded text-xs font-medium mr-1"><i class="fa-solid fa-copy mr-1"></i>Copy</button>
                            <button type="button" onclick="removeWeek(${wIdx})" class="px-2 py-1 bg-red-100 text-red-700 rounded text-xs font-medium"><i class="fa-solid fa-trash mr-1"></i>Delete</button>
                        </div>
                    </div>
                `;

                week.days.forEach((day, dIdx) => {
                    const dayPath = `weeks[${wIdx}].days[${dIdx}]`;
                    html += `
                    <div class="bg-white border border-gray-200 rounded p-4 ml-4 mb-4 shadow-sm">
                        <div class="flex justify-between items-center mb-2">
                            <div class="flex items-center space-x-2">
                                <button type="button" onclick="moveDay(${wIdx}, ${dIdx}, -1)" class="text-gray-400 hover:text-gray-600"><i class="fa-solid fa-arrow-up"></i></button>
                                <button type="button" onclick="moveDay(${wIdx}, ${dIdx}, 1)" class="text-gray-400 hover:text-gray-600"><i class="fa-solid fa-arrow-down"></i></button>
                                <input type="text" value="${day.name}" oninput="updateField('${dayPath}', 'name', this.value)" class="border border-gray-300 rounded px-2 py-1 text-sm font-semibold" placeholder="Day Name" />
                                <input type="text" value="${day.description}" oninput="updateField('${dayPath}', 'description', this.value)" class="border border-gray-300 rounded px-2 py-1 text-sm w-64" placeholder="Description" />
                            </div>
                            <div>
                                <button type="button" onclick="addSection(${wIdx}, ${dIdx})" class="px-2 py-1 bg-green-100 text-green-700 rounded text-xs font-medium mr-1">Add Section</button>
                                <button type="button" onclick="duplicateDay(${wIdx}, ${dIdx})" class="px-2 py-1 bg-yellow-100 text-yellow-700 rounded text-xs font-medium mr-1"><i class="fa-solid fa-copy"></i></button>
                                <button type="button" onclick="removeDay(${wIdx}, ${dIdx})" class="px-2 py-1 bg-red-100 text-red-700 rounded text-xs font-medium"><i class="fa-solid fa-trash"></i></button>
                            </div>
                        </div>
                    `;

                    day.sections.forEach((section, sIdx) => {
                        const secPath = `${dayPath}.sections[${sIdx}]`;
                        html += `
                        <div class="bg-slate-50 border border-slate-200 rounded p-3 ml-4 mb-3">
                            <div class="flex justify-between items-center mb-2">
                                <div class="flex items-center space-x-2">
                                    <button type="button" onclick="moveSection(${wIdx}, ${dIdx}, ${sIdx}, -1)" class="text-gray-400 hover:text-gray-600"><i class="fa-solid fa-arrow-up"></i></button>
                                    <button type="button" onclick="moveSection(${wIdx}, ${dIdx}, ${sIdx}, 1)" class="text-gray-400 hover:text-gray-600"><i class="fa-solid fa-arrow-down"></i></button>
                                    <input type="text" value="${section.name}" oninput="updateField('${secPath}', 'name', this.value)" class="border border-gray-300 rounded px-2 py-1 text-sm font-medium" placeholder="Section Name (e.g. Warmup)" />
                                </div>
                                <div>
                                    <button type="button" onclick="addTraining(${wIdx}, ${dIdx}, ${sIdx})" class="px-2 py-1 bg-indigo-100 text-indigo-700 rounded text-xs font-medium mr-1">Add Exercise</button>
                                    <button type="button" onclick="duplicateSection(${wIdx}, ${dIdx}, ${sIdx})" class="px-2 py-1 bg-yellow-100 text-yellow-700 rounded text-xs font-medium mr-1"><i class="fa-solid fa-copy"></i></button>
                                    <button type="button" onclick="removeSection(${wIdx}, ${dIdx}, ${sIdx})" class="px-2 py-1 bg-red-100 text-red-700 rounded text-xs font-medium"><i class="fa-solid fa-trash"></i></button>
                                </div>
                            </div>
                        `;

                        if (section.trainings.length > 0) {
                            html += `
                            <table class="min-w-full divide-y divide-gray-200 mt-2">
                                <thead class="bg-gray-200">
                                    <tr>
                                        <th class="px-2 py-1 text-left text-xs font-semibold text-gray-600 w-12">Ord</th>
                                        <th class="px-2 py-1 text-left text-xs font-semibold text-gray-600 w-1/3">Exercise</th>
                                        <th class="px-2 py-1 text-left text-xs font-semibold text-gray-600 w-16">Sets</th>
                                        <th class="px-2 py-1 text-left text-xs font-semibold text-gray-600 w-16">Reps</th>
                                        <th class="px-2 py-1 text-left text-xs font-semibold text-gray-600 w-20">Wt(kg)</th>
                                        <th class="px-2 py-1 text-left text-xs font-semibold text-gray-600 w-20">Dur(s)</th>
                                        <th class="px-2 py-1 text-left text-xs font-semibold text-gray-600 w-20">Rest(s)</th>
                                        <th class="px-2 py-1 w-20 text-right">Actions</th>
                                    </tr>
                                </thead>
                                <tbody>
                            `;

                            section.trainings.forEach((tr, tIdx) => {
                                const trPath = `${secPath}.trainings[${tIdx}]`;
                                const options = availableTrainings.map(t => `<option value="${t.id}" ${t.id === tr.trainingId ? 'selected' : ''}>${t.name}</option>`).join('');
                                html += `
                                    <tr class="hover:bg-white transition-colors">
                                        <td class="px-1 py-1 text-center whitespace-nowrap">
                                            <button type="button" onclick="moveTraining(${wIdx}, ${dIdx}, ${sIdx}, ${tIdx}, -1)" class="text-gray-400 hover:text-indigo-600 p-0.5"><i class="fa-solid fa-chevron-up text-[10px]"></i></button>
                                            <button type="button" onclick="moveTraining(${wIdx}, ${dIdx}, ${sIdx}, ${tIdx}, 1)" class="text-gray-400 hover:text-indigo-600 p-0.5"><i class="fa-solid fa-chevron-down text-[10px]"></i></button>
                                        </td>
                                        <td class="px-1 py-1"><select onchange="updateField('${trPath}', 'trainingId', this.value)" class="w-full border-gray-300 rounded text-sm py-1"><option value="">--Select--</option>${options}</select></td>
                                        <td class="px-1 py-1"><input type="number" value="${tr.sets}" oninput="updateField('${trPath}', 'sets', parseInt(this.value)||0)" class="w-full border-gray-300 rounded text-sm py-1 text-center" /></td>
                                        <td class="px-1 py-1"><input type="number" value="${tr.reps}" oninput="updateField('${trPath}', 'reps', parseInt(this.value)||0)" class="w-full border-gray-300 rounded text-sm py-1 text-center" /></td>
                                        <td class="px-1 py-1"><input type="number" step="0.5" value="${tr.weight}" oninput="updateField('${trPath}', 'weight', parseFloat(this.value)||0)" class="w-full border-gray-300 rounded text-sm py-1 text-center" /></td>
                                        <td class="px-1 py-1"><input type="number" value="${tr.durationSeconds}" oninput="updateField('${trPath}', 'durationSeconds', parseInt(this.value)||0)" class="w-full border-gray-300 rounded text-sm py-1 text-center" /></td>
                                        <td class="px-1 py-1"><input type="number" value="${tr.restSeconds}" oninput="updateField('${trPath}', 'restSeconds', parseInt(this.value)||0)" class="w-full border-gray-300 rounded text-sm py-1 text-center" /></td>
                                        <td class="px-1 py-1 text-right whitespace-nowrap">
                                            <button type="button" onclick="duplicateTraining(${wIdx}, ${dIdx}, ${sIdx}, ${tIdx})" class="text-yellow-500 hover:text-yellow-700 mx-1" title="Duplicate"><i class="fa-solid fa-copy"></i></button>
                                            <button type="button" onclick="removeTraining(${wIdx}, ${dIdx}, ${sIdx}, ${tIdx})" class="text-red-500 hover:text-red-700 mx-1" title="Remove"><i class="fa-solid fa-trash"></i></button>
                                        </td>
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

        // --- Save Logic ---"""
    
    start_idx = content.find(old_logic_start)
    end_idx = content.find(old_logic_end)
    
    if start_idx != -1 and end_idx != -1:
        new_content = content[:start_idx] + new_logic + content[end_idx + len(old_logic_end):]
        with open(path, "w") as f:
            f.write(new_content)
        print(f"Updated {path}")
    else:
        print(f"Could not find logic blocks in {path}")
