import re

file = "src/AzureTest.MVC/Views/Dashboard/Builder.cshtml"
with open(file, "r") as f:
    content = f.read()

content = content.replace(
"""        const rawComponentsData = @Html.Raw(System.Text.Json.JsonSerializer.Serialize(allRawComponents.Select(r => new { r.Id, r.Name, r.Type })));""",
"""        const rawComponentsData = @Html.Raw(System.Text.Json.JsonSerializer.Serialize(allRawComponents.Select(r => new { r.Id, r.Name, r.Type, r.MinWidth, r.MaxWidth, r.MinHeight, r.MaxHeight, r.IsResizable })));"""
)

content = content.replace(
"""                                <label class="block text-sm font-medium text-slate-700 mb-1">Select Component</label>
                                <select id="rawComponentSelect" class="block w-full rounded-md border-slate-300 shadow-sm focus:border-brand-500 focus:ring-brand-500 sm:text-sm">
                                    <!-- Options injected via JS -->
                                </select>
                            </div>""",
"""                                <label class="block text-sm font-medium text-slate-700 mb-1">Select Component</label>
                                <select id="rawComponentSelect" class="block w-full rounded-md border-slate-300 shadow-sm focus:border-brand-500 focus:ring-brand-500 sm:text-sm" onchange="updateRawComponentDetails()">
                                    <!-- Options injected via JS -->
                                </select>
                                <div id="rawComponentDetails" class="mt-3 text-left bg-slate-50 p-3 rounded border border-slate-200 hidden">
                                    <p class="text-xs text-slate-500 mb-2 font-medium uppercase tracking-wider">Layout Constraints</p>
                                    <div class="grid grid-cols-2 gap-2 text-sm text-slate-700">
                                        <div><span class="text-slate-400">Min Width:</span> <span id="rawMinW" class="font-medium"></span></div>
                                        <div><span class="text-slate-400">Max Width:</span> <span id="rawMaxW" class="font-medium"></span></div>
                                        <div><span class="text-slate-400">Min Height:</span> <span id="rawMinH" class="font-medium"></span></div>
                                        <div><span class="text-slate-400">Max Height:</span> <span id="rawMaxH" class="font-medium"></span></div>
                                        <div class="col-span-2 mt-1"><span class="text-slate-400">Resizable:</span> <span id="rawResizable" class="font-medium"></span></div>
                                    </div>
                                </div>
                            </div>"""
)

content = content.replace(
"""            if (!select.disabled) {
                rawSelectInstance = new TomSelect(select, {
                    create: false,
                    dropdownParent: "body",
                    sortField: { field: "text", direction: "asc" }
                });
            }""",
"""            if (!select.disabled) {
                rawSelectInstance = new TomSelect(select, {
                    create: false,
                    dropdownParent: "body",
                    sortField: { field: "text", direction: "asc" },
                    onChange: function(value) {
                        updateRawComponentDetails(value);
                    }
                });
            }"""
)

# We also need to define updateRawComponentDetails
content = content.replace(
"""        function closeRawModal() {""",
"""        function updateRawComponentDetails(id) {
            const detailsDiv = document.getElementById('rawComponentDetails');
            if (!id) {
                detailsDiv.classList.add('hidden');
                return;
            }
            
            const comp = rawComponentsData.find(r => r.Id === id);
            if (comp) {
                document.getElementById('rawMinW').textContent = comp.MinWidth > 0 ? comp.MinWidth : 'None';
                document.getElementById('rawMaxW').textContent = comp.MaxWidth > 0 ? comp.MaxWidth : 'None';
                document.getElementById('rawMinH').textContent = comp.MinHeight > 0 ? comp.MinHeight : 'None';
                document.getElementById('rawMaxH').textContent = comp.MaxHeight > 0 ? comp.MaxHeight : 'None';
                document.getElementById('rawResizable').innerHTML = comp.IsResizable ? '<span class="text-green-600">Yes</span>' : '<span class="text-red-500">No (Fixed)</span>';
                detailsDiv.classList.remove('hidden');
            } else {
                detailsDiv.classList.add('hidden');
            }
        }

        function closeRawModal() {"""
)

# And we need to make sure we also pass these limits to gridstack!
content = content.replace(
"""            const firstGridId = Object.keys(grids)[0];
            grids[firstGridId].addWidget({w: 4, h: 4, content: content, id: id, autoPosition: true});
            closeRawModal();""",
"""            const firstGridId = Object.keys(grids)[0];
            const comp = rawComponentsData.find(r => r.Id === id);
            let widgetProps = {w: 4, h: 4, content: content, id: id, autoPosition: true};
            if (comp) {
                if (comp.MinWidth > 0) widgetProps.minW = comp.MinWidth;
                if (comp.MaxWidth > 0) widgetProps.maxW = comp.MaxWidth;
                if (comp.MinHeight > 0) widgetProps.minH = comp.MinHeight;
                if (comp.MaxHeight > 0) widgetProps.maxH = comp.MaxHeight;
                widgetProps.noResize = !comp.IsResizable;
            }
            grids[firstGridId].addWidget(widgetProps);
            closeRawModal();"""
)

with open(file, "w") as f:
    f.write(content)
