import os
import re

def write_file(path, content):
    with open(path, "w") as f:
        f.write(content)

def read_file(path):
    with open(path, "r") as f:
        return f.read()

def consolidate(directory):
    for root, dirs, files in os.walk(directory):
        if not files or ("Queries" not in root and "Commands" not in root):
            continue
        # check if it's a specific query folder
        if root.endswith("Commands") or root.endswith("Queries"):
            continue
        
        print(f"Processing {root}...")
        
        cs_files = [f for f in files if f.endswith(".cs")]
        if len(cs_files) <= 1:
            continue
            
        main_file = None
        for f in cs_files:
            if "Handler" in f:
                main_file = f
                break
        if not main_file:
            for f in cs_files:
                if "Query" in f or "Command" in f:
                    main_file = f
                    break
        
        if not main_file:
            continue
            
        usings = set()
        code_blocks = []
        
        for f in cs_files:
            content = read_file(os.path.join(root, f))
            # extract usings
            for line in content.split("\n"):
                if line.startswith("using ") and ";" in line:
                    usings.add(line.strip())
            
            content_no_usings = re.sub(r"^using .*?;", "", content, flags=re.MULTILINE)
            content_no_usings = content_no_usings.strip()
            code_blocks.append(content_no_usings)
            
            if f != main_file:
                os.remove(os.path.join(root, f))
                
        final_usings = "\n".join(sorted(usings))
        final_content = final_usings + "\n\n" + "\n\n".join(code_blocks)
        write_file(os.path.join(root, main_file), final_content)

consolidate("src/AzureTest.Application/Users/Queries")
consolidate("src/AzureTest.Application/Companies/Queries")

