import os
import re

def write_file(path, content):
    os.makedirs(os.path.dirname(path), exist_ok=True)
    with open(path, "w") as f:
        f.write(content)

def read_file(path):
    with open(path, "r") as f:
        return f.read()

# 1. LoginUserResult
content = read_file("src/AzureTest.Application/Users/Commands/LoginUser/LoginUserCommandHandler.cs")
dto_pattern = re.compile(r"public record LoginUserResult.*?\}\n", re.DOTALL)
match = dto_pattern.search(content)
if match:
    dto_code = match.group(0)
    content = dto_pattern.sub("", content)
    content = "using AzureTest.Contracts.DTOs.Users;\n" + content
    write_file("src/AzureTest.Application/Users/Commands/LoginUser/LoginUserCommandHandler.cs", content)
    
    dto_file_content = "namespace AzureTest.Contracts.DTOs.Users;\n\n" + dto_code
    write_file("src/AzureTest.Contracts/DTOs/Users/LoginUserResult.cs", dto_file_content)

# 2. RefreshTokenResult
try:
    content = read_file("src/AzureTest.Application/Users/Commands/RefreshToken/RefreshTokenCommandHandler.cs")
    dto_pattern = re.compile(r"public record RefreshTokenResult.*?;")
    match = dto_pattern.search(content)
    if match:
        dto_code = match.group(0)
        content = dto_pattern.sub("", content)
        content = "using AzureTest.Contracts.DTOs.Users;\n" + content
        write_file("src/AzureTest.Application/Users/Commands/RefreshToken/RefreshTokenCommandHandler.cs", content)
        
        dto_file_content = "namespace AzureTest.Contracts.DTOs.Users;\n\n" + dto_code
        write_file("src/AzureTest.Contracts/DTOs/Users/RefreshTokenResult.cs", dto_file_content)
except FileNotFoundError:
    pass

# update UserCommandUnit and IUserCommandUnit
def add_using(path, using_stmt):
    c = read_file(path)
    if using_stmt not in c:
        c = using_stmt + "\n" + c
        write_file(path, c)

add_using("src/AzureTest.Application/Users/UserCommandUnit.cs", "using AzureTest.Contracts.DTOs.Users;")
add_using("src/AzureTest.Application/Users/IUserCommandUnit.cs", "using AzureTest.Contracts.DTOs.Users;")
