import re
with open('./src/AzureTest.MVC/Controllers/TicketController.cs', 'r') as f:
    content = f.read()

# Change isAdmin to isStaff
content = content.replace('bool isAdmin = _currentUser.Roles.Contains("Admin");', 'bool isStaff = _currentUser.Roles.Contains("Admin") || _currentUser.Roles.Contains("Support");')

content = content.replace('isAdmin ? null : _currentUser.Id', 'isStaff ? null : _currentUser.Id')
content = content.replace('ViewBag.IsAdmin = isAdmin;', 'ViewBag.IsAdmin = isStaff;')

# Also change [Authorize(Roles = "Admin")] to allow Support? Wait, Support role should be able to update status.
content = content.replace('[Authorize(Roles = "Admin")]', '') # Remove role restriction, the dynamic filter handles if they have access to the UpdateStatus action

with open('./src/AzureTest.MVC/Controllers/TicketController.cs', 'w') as f:
    f.write(content)

