import re

path = "src/AzureTest.MVC/Views/Shared/_Layout.cshtml"
with open(path, "r") as f: lines = f.readlines()

new_lines = []
skip = False
for i, line in enumerate(lines):
    if "/* Sidebar Collapse */" in line:
        skip = True
    if skip and "/* Tooltip */" in line:
        pass
    if skip and "    /* Scrollbar override */" in line:
        skip = False
    
    if "<!-- Sidebar -->" in line:
        new_lines.append("    <partial name=\"_Sidebar\" />\n")
        skip = True
    
    if skip and "</aside>" in line:
        skip = False
        continue
        
    if "const sb = document.getElementById(\"sidebar\");" in line:
        skip = True
        
    if skip and "});" in line and i > 850:
        # We also need to skip the closing script tag lines if they were part of it, but let's be careful.
        pass

    if not skip:
        new_lines.append(line)

with open(path + ".tmp", "w") as f: f.writelines(new_lines)

