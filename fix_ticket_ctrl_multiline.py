import re
with open('./src/AzureTest.MVC/Controllers/TicketController.cs', 'r') as f:
    content = f.read()

# Change $" to $@"" for the content variable
content = content.replace('var content = $"An automated bug report', 'var content = $@\"An automated bug report')

with open('./src/AzureTest.MVC/Controllers/TicketController.cs', 'w') as f:
    f.write(content)
