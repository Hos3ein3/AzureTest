import os
import re

def read_file(path):
    with open(path, "r") as f:
        return f.read()

def write_file(path, content):
    with open(path, "w") as f:
        f.write(content)

def process_dir(directory):
    for root, dirs, files in os.walk(directory):
        for f in files:
            if not f.endswith(".cs"):
                continue
            path = os.path.join(root, f)
            content = read_file(path)
            
            # Check if it has a Command
            cmd_match = re.search(r"public (?:sealed )?record (\w+Command)\s*\((.*?)\)", content, re.DOTALL)
            if not cmd_match:
                continue
                
            cmd_name = cmd_match.group(1)
            cmd_args = cmd_match.group(2)
            
            # Check if it already has a validator
            if f"AbstractValidator<{cmd_name}>" in content:
                continue
                
            # Generate rules
            rules = []
            if cmd_args:
                # split by comma, but be careful with generics. Assuming simple types.
                args = [arg.strip() for arg in cmd_args.split(",")]
                for arg in args:
                    parts = arg.split()
                    if len(parts) >= 2:
                        type_name = parts[0]
                        prop_name = parts[1]
                        # remove default values
                        prop_name = prop_name.split("=")[0].strip()
                        if type_name == "string":
                            rules.append(f"        RuleFor(v => v.{prop_name}).NotEmpty();")
                        elif type_name == "Guid":
                            rules.append(f"        RuleFor(v => v.{prop_name}).NotEmpty();")
            
            rules_str = "\n".join(rules)
            validator_code = f"""
public sealed class {cmd_name}Validator : AbstractValidator<{cmd_name}>
{{
    public {cmd_name}Validator()
    {{
{rules_str}
    }}
}}
"""
            # add using FluentValidation if not exists
            if "using FluentValidation;" not in content:
                content = "using FluentValidation;\n" + content
                
            content = content + "\n" + validator_code
            write_file(path, content)

process_dir("src/AzureTest.Application/Users/Commands")
process_dir("src/AzureTest.Application/Companies/Commands")
