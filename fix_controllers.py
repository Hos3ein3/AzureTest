import re

def fix(path):
    with open(path, 'r') as f:
        content = f.read()
    
    # Remove all trailing UpdateLayout methods and broken brackets
    # Find the last valid method "Delete"
    delete_match = re.search(r'public async Task<IActionResult> Delete\(.*?\n\s*\{.*?\n\s*\}', content, re.DOTALL)
    if delete_match:
        end_idx = delete_match.end()
        content = content[:end_idx] + "\n}\n"
    
    with open(path, 'w') as f:
        f.write(content)

fix('/Users/mohammadhosein/Development/AzureTest/src/AzureTest.MVC/Controllers/IdeaSectionController.cs')
fix('/Users/mohammadhosein/Development/AzureTest/src/AzureTest.MVC/Controllers/IdeaPageController.cs')

