files = [
    '/Users/mohammadhosein/Development/AzureTest/src/AzureTest.MVC/Views/IdeaTracker/Create.cshtml',
    '/Users/mohammadhosein/Development/AzureTest/src/AzureTest.MVC/Views/IdeaTracker/Edit.cshtml',
    '/Users/mohammadhosein/Development/AzureTest/src/AzureTest.MVC/Views/IdeaTracker/Details.cshtml'
]

for file_path in files:
    with open(file_path, 'r') as f:
        content = f.read()
    
    # Replace npm/@editorjs with npm/@@editorjs
    content = content.replace('npm/@editorjs', 'npm/@@editorjs')
    
    with open(file_path, 'w') as f:
        f.write(content)
