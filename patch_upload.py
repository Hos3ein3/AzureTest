import re

with open('/Users/mohammadhosein/Development/AzureTest/src/AzureTest.MVC/Controllers/IdeaTrackerController.cs', 'r') as f:
    content = f.read()

# Replace the Editor.js upload response with TinyMCE response
old_return = 'return Json(new { success = 1, file = new { url = $"/uploads/ideas/{fileName}" } });'
new_return = 'return Json(new { location = $"/uploads/ideas/{fileName}" });'
content = content.replace(old_return, new_return)

with open('/Users/mohammadhosein/Development/AzureTest/src/AzureTest.MVC/Controllers/IdeaTrackerController.cs', 'w') as f:
    f.write(content)
