import os
import re

def write_file(path, content):
    with open(path, "w") as f:
        f.write(content)

def read_file(path):
    with open(path, "r") as f:
        return f.read()

def process_file(path):
    content = read_file(path)
    
    # find all namespaces
    namespaces = re.findall(r"^namespace\s+([\w\.]+);$", content, flags=re.MULTILINE)
    if not namespaces:
        return
        
    main_ns = namespaces[0]
    
    # remove all namespace declarations
    content = re.sub(r"^namespace\s+[\w\.]+;\s*", "", content, flags=re.MULTILINE)
    
    # add the first one back right after usings
    # find the end of usings
    lines = content.split("\n")
    usings = []
    other_lines = []
    for line in lines:
        if line.startswith("using ") and ";" in line:
            usings.append(line)
        elif line.strip() == "":
            pass
        else:
            other_lines.append(line)
            
    final_content = "\n".join(usings) + f"\n\nnamespace {main_ns};\n\n" + "\n".join(other_lines)
    write_file(path, final_content)

for root, dirs, files in os.walk("src/AzureTest.Application/Users"):
    for f in files:
        if f.endswith(".cs"):
            process_file(os.path.join(root, f))
            
for root, dirs, files in os.walk("src/AzureTest.Application/Companies"):
    for f in files:
        if f.endswith(".cs"):
            process_file(os.path.join(root, f))

