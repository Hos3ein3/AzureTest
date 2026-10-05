import re
with open('./src/AzureTest.MVC/Controllers/ExpenseController.cs', 'r') as f:
    content = f.read()

content = content.replace('if (!_currentUser.Roles.Contains("Admin"))', 'if (!_currentUser.Roles.Contains("Admin") && !_currentUser.Permissions.Contains("Expense/Analytics"))')
content = content.replace('var isAdmin = _currentUser.Roles.Contains("Admin");', 'var isAdmin = _currentUser.Roles.Contains("Admin") || _currentUser.Permissions.Contains("Expense/Analytics");')

with open('./src/AzureTest.MVC/Controllers/ExpenseController.cs', 'w') as f:
    f.write(content)

