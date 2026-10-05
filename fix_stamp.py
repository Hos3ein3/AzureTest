import re
import glob

# 1. UpdateUserCommandHandler
with open('./src/AzureTest.Application/Users/Commands/UpdateUser/UpdateUserCommandHandler.cs', 'r') as f:
    c = f.read()
c = c.replace('_logger.LogInformation("Admin updated user {UserId}", command.Id);', '_logger.LogInformation("Admin updated user {UserId}", command.Id);\n        await _userManager.UpdateSecurityStampAsync(user);')
with open('./src/AzureTest.Application/Users/Commands/UpdateUser/UpdateUserCommandHandler.cs', 'w') as f:
    f.write(c)

# 2. AssignRolesToUserCommandHandler
with open('./src/AzureTest.Application/Users/Commands/AssignRolesToUser/AssignRolesToUserCommandHandler.cs', 'r') as f:
    c = f.read()
c = c.replace('_logger.LogInformation("Admin updated roles for user {UserId}", command.UserId);', '_logger.LogInformation("Admin updated roles for user {UserId}", command.UserId);\n        await _userManager.UpdateSecurityStampAsync(user);')
with open('./src/AzureTest.Application/Users/Commands/AssignRolesToUser/AssignRolesToUserCommandHandler.cs', 'w') as f:
    f.write(c)

