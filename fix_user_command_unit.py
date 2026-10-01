path1 = "src/AzureTest.Application/Users/IUserCommandUnit.cs"
path2 = "src/AzureTest.Application/Users/UserCommandUnit.cs"

def fix_unit(path):
    with open(path, 'r') as f:
        c = f.read()
    c = c.replace("ICommandHandler<RegisterUserCommand, (bool Success, string[] Errors)>", "ICommandHandler<RegisterUserCommand>")
    with open(path, 'w') as f:
        f.write(c)

fix_unit(path1)
fix_unit(path2)
