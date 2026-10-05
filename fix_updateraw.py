import re

file = "src/AzureTest.MVC/Views/Dashboard/Builder.cshtml"
with open(file, "r") as f:
    content = f.read()

new_method = """        function updateRawComponentDetails(id) {
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
                
                // Update Preview
                const previewContent = document.getElementById('rawPreviewContent');
                const viewNameBadge = document.getElementById('rawViewName');
                
                if (comp.Type === 'RawHtml') {
                    viewNameBadge.classList.add('hidden');
                    previewContent.innerHTML = comp.Content || '<span class="text-slate-400 italic">No HTML content available.</span>';
                } else if (comp.Type === 'JsonConfig') {
                    if (comp.ViewName) {
                        viewNameBadge.textContent = comp.ViewName;
                        viewNameBadge.classList.remove('hidden');
                    } else {
                        viewNameBadge.classList.add('hidden');
                    }
                    
                    if (comp.JsonConfiguration) {
                        try {
                            const parsed = JSON.parse(comp.JsonConfiguration);
                            previewContent.textContent = JSON.stringify(parsed, null, 2);
                        } catch(e) {
                            previewContent.textContent = comp.JsonConfiguration;
                        }
                    } else {
                        previewContent.innerHTML = '<span class="text-slate-400 italic">No JSON config available.</span>';
                    }
                }
                
                detailsDiv.classList.remove('hidden');
            } else {
                detailsDiv.classList.add('hidden');
            }
        }"""
        
content = re.sub(r"        function updateRawComponentDetails\(id\) \{.*?detailsDiv\.classList\.add\('hidden'\);\s*\}\s*\}", new_method, content, flags=re.DOTALL)

with open(file, "w") as f:
    f.write(content)
