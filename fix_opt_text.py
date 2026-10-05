import re

file = "src/AzureTest.MVC/Views/Dashboard/Builder.cshtml"
with open(file, "r") as f:
    content = f.read()

old_js = """                filtered.forEach(r => {
                    const opt = document.createElement('option');
                    opt.value = r.Id;
                    opt.textContent = r.Name;
                    select.appendChild(opt);
                });"""
new_js = """                filtered.forEach(r => {
                    const opt = document.createElement('option');
                    opt.value = r.Id;
                    const minTxt = r.MinWidth > 0 && r.MinHeight > 0 ? r.MinWidth + "x" + r.MinHeight : "None";
                    const maxTxt = r.MaxWidth > 0 && r.MaxHeight > 0 ? r.MaxWidth + "x" + r.MaxHeight : "None";
                    opt.textContent = `${r.Name} (Min: ${minTxt} | Max: ${maxTxt} | Resize: ${r.IsResizable ? 'Y' : 'N'})`;
                    select.appendChild(opt);
                });"""
content = content.replace(old_js, new_js)

with open(file, "w") as f:
    f.write(content)
