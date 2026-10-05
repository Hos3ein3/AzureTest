import re
with open('./src/AzureTest.Infrastructure/Seeding/IdentityDataSeeder.cs', 'r') as f:
    content = f.read()

content = content.replace('await _unitOfWork.RolePermissions.AddAsync', '_unitOfWork.RolePermissions.Add')
content = content.replace('RoleId = userRole.Id', 'RoleName = userRole.Name')
content = content.replace('Id = Guid.NewGuid(),', '')

with open('./src/AzureTest.Infrastructure/Seeding/IdentityDataSeeder.cs', 'w') as f:
    f.write(content)
