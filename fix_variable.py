file_path = '/Users/mohammadhosein/Development/AzureTest/src/AzureTest.MVC/Views/IdeaTracker/Details.cshtml'

with open(file_path, 'r') as f:
    content = f.read()

# Replace 'page' variable with 'subPage'
content = content.replace('var page in subPages', 'var subPage in subPages')
content = content.replace('@page.Id', '@subPage.Id')
content = content.replace('@page.Icon', '@subPage.Icon')
content = content.replace('@page.Title', '@subPage.Title')
content = content.replace('@page.Status', '@subPage.Status')

with open(file_path, 'w') as f:
    f.write(content)
