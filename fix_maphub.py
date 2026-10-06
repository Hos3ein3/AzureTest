import os

file = "src/AzureTest.MVC/Program.cs"
with open(file, "r") as f:
    lines = f.readlines()

new_lines = []
found = False
for line in lines:
    if "MapHub" in line:
        if not found:
            new_lines.append(line)
            found = True
        else:
            continue
    else:
        new_lines.append(line)

with open(file, "w") as f:
    f.writelines(new_lines)
print("MapHub Fixed")
