import re

for view in ["Create.cshtml", "Edit.cshtml"]:
    path = f"src/AzureTest.MVC/Views/GymCourses/{view}"
    with open(path, "r") as f: c = f.read()

    old_option = "const options = availableTrainings.map(t => `<option value=\"${t.id}\" ${t.id === tr.trainingId ? 'selected' : ''}>${t.name}</option>`).join('');"
    new_option = "const options = availableTrainings.map(t => { const eq = (t.equipments && t.equipments.length > 0) ? ` (${t.equipments.join(', ')})` : ''; return `<option value=\"${t.id}\" ${t.id === tr.trainingId ? 'selected' : ''}>${t.name}${eq}</option>`; }).join('');"
    c = c.replace(old_option, new_option)

    # Add searchable class
    c = c.replace('class="w-full border-gray-300 rounded text-sm py-1"', 'class="w-full border-gray-300 rounded text-sm py-1 searchable-select"')

    # Add TomSelect scripts
    if "TomSelect" not in c:
        c = c.replace('</script>\n}', '''
        function initSelects() {
            document.querySelectorAll('.searchable-select').forEach(el => {
                if(!el.tomselect) {
                    new TomSelect(el, { create: false, sortField: {field: "text", direction: "asc"} });
                }
            });
        }
        const originalRenderWeeks = renderWeeks;
        renderWeeks = function() {
            originalRenderWeeks();
            initSelects();
        };
</script>
<link href="https://cdn.jsdelivr.net/npm/tom-select@2.2.2/dist/css/tom-select.css" rel="stylesheet">
<script src="https://cdn.jsdelivr.net/npm/tom-select@2.2.2/dist/js/tom-select.complete.min.js"></script>
}''')
    
    with open(path, "w") as f: f.write(c)

print("GymCourses views patched")
