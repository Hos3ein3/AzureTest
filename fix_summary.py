with open('/Users/mohammadhosein/Development/AzureTest/src/AzureTest.MVC/Views/Shared/_Layout.cshtml', 'r') as f:
    content = f.read()

# Add sidebar-text to summary spans
content = content.replace('<span class="ml-2 font-medium flex-1">Financials</span>', '<span class="ml-2 font-medium flex-1 sidebar-text">Financials</span>')
content = content.replace('<span class="ml-2 font-medium flex-1">Settings</span>', '<span class="ml-2 font-medium flex-1 sidebar-text">Settings</span>')
content = content.replace('<span class="flex-1">Personalization</span>', '<span class="flex-1 sidebar-text">Personalization</span>')

# Hide the chevron arrows on collapse
content = content.replace('<style>', '<style>\n        #sidebar.collapsed summary i.fa-chevron-down {\n            display: none !important;\n        }')

with open('/Users/mohammadhosein/Development/AzureTest/src/AzureTest.MVC/Views/Shared/_Layout.cshtml', 'w') as f:
    f.write(content)
