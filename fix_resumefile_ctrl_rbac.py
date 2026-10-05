import re
with open('./src/AzureTest.MVC/Controllers/ResumeFileController.cs', 'r') as f:
    content = f.read()

content = content.replace('bool isAdmin = _currentUser.Roles.Contains("Admin");', 'bool isAdmin = _currentUser.Roles.Contains("Admin") || _currentUser.Permissions.Contains("ResumeFile/Delete");')

with open('./src/AzureTest.MVC/Controllers/ResumeFileController.cs', 'w') as f:
    f.write(content)

