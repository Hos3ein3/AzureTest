import re
with open('./src/AzureTest.MVC/Controllers/HomeController.cs', 'r') as f:
    content = f.read()

content = content.replace('    }\n);\n    }\n}', '    }\n}')

with open('./src/AzureTest.MVC/Controllers/HomeController.cs', 'w') as f:
    f.write(content)
