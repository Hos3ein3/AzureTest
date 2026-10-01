import re

path = "src/AzureTest.Application/Users/Commands/RegisterUser/RegisterUserCommand.cs"
with open(path, 'r') as f:
    c = f.read()

c = c.replace("ICommand<(bool Success, string[] Errors)>", "ICommand")
c = c.replace("ICommandHandler<RegisterUserCommand, (bool Success, string[] Errors)>", "ICommandHandler<RegisterUserCommand>")
c = c.replace("public async Task<(bool Success, string[] Errors)> HandleAsync", "public async Task<Result> HandleAsync")
c = c.replace("return await _userManagerService.RegisterUserAsync(request.Email, request.Password, request.FirstName, request.LastName);", 
"""var res = await _userManagerService.RegisterUserAsync(request.Email, request.Password, request.FirstName, request.LastName);
        if (res.Success) return Result.Success();
        return Result.Failure(string.Join(", ", res.Errors));""")

if "using AzureTest.Application.Abstractions;" not in c:
    c = "using AzureTest.Application.Abstractions;\n" + c

with open(path, 'w') as f:
    f.write(c)

