import os
import re

app_dir = "src/AzureTest.Application"
contracts_dir = "src/AzureTest.Contracts/DTOs"

def write_file(path, content):
    with open(path, "w") as f:
        f.write(content)

def read_file(path):
    with open(path, "r") as f:
        return f.read()

# For each directory in Users/Commands and Companies/Commands, list files
def consolidate(directory):
    for root, dirs, files in os.walk(directory):
        if not files or "Commands" not in root:
            continue
        # check if it's a specific command folder
        if root.endswith("Commands") or root.endswith("Queries"):
            continue
        
        # we have a specific action folder, e.g. src/AzureTest.Application/Users/Commands/LoginUser
        print(f"Processing {root}...")
        
        # files to merge
        cs_files = [f for f in files if f.endswith(".cs")]
        
        # pick the main file: Handler > Command > Validator
        main_file = None
        for f in cs_files:
            if "Handler" in f:
                main_file = f
                break
        if not main_file:
            for f in cs_files:
                if "Command" in f or "Query" in f:
                    main_file = f
                    break
        
        if not main_file:
            continue
            
        merged_content = ""
        # collect usings
        usings = set()
        code_blocks = []
        
        for f in cs_files:
            content = read_file(os.path.join(root, f))
            # extract usings
            for line in content.split("\n"):
                if line.startswith("using ") and ";" in line:
                    usings.add(line.strip())
            
            # remove usings from content
            content_no_usings = re.sub(r"^using .*?;", "", content, flags=re.MULTILINE)
            content_no_usings = content_no_usings.strip()
            code_blocks.append(content_no_usings)
            
            # delete the other files
            if f != main_file:
                os.remove(os.path.join(root, f))
                
        # write the merged file
        final_usings = "\n".join(sorted(usings))
        final_content = final_usings + "\n\n" + "\n\n".join(code_blocks)
        write_file(os.path.join(root, main_file), final_content)

consolidate("src/AzureTest.Application/Users/Commands")
consolidate("src/AzureTest.Application/Users/Queries")
consolidate("src/AzureTest.Application/Companies/Commands")
consolidate("src/AzureTest.Application/Companies/Queries")

