import re
with open('./src/AzureTest.MVC/Controllers/TicketController.cs', 'r') as f:
    content = f.read()

content = content.replace('bool isStaff = _currentUser.Roles.Contains("Admin") || _currentUser.Roles.Contains("Support");', 'bool isStaff = _currentUser.Roles.Contains("Admin") || _currentUser.Permissions.Contains("Ticket/UpdateStatus");')

with open('./src/AzureTest.MVC/Controllers/TicketController.cs', 'w') as f:
    f.write(content)

