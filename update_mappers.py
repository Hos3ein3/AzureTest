import re

def fix_mapper(path, is_page):
    with open(path, 'r') as f:
        content = f.read()
    
    if is_page:
        content = content.replace("doc.Id, doc.IdeaId, doc.Title, doc.Content, doc.CreatedAt, doc.UpdatedAt",
                                  "doc.Id, doc.IdeaId, doc.Title, doc.Content, doc.CreatedAt, doc.X, doc.Y, doc.Width, doc.Height, doc.UpdatedAt")
    else:
        content = content.replace("doc.Id, doc.IdeaPageId, doc.Title, doc.Content, doc.OrderIndex, doc.CreatedAt, doc.UpdatedAt",
                                  "doc.Id, doc.IdeaPageId, doc.Title, doc.Content, doc.OrderIndex, doc.CreatedAt, doc.X, doc.Y, doc.Width, doc.Height, doc.UpdatedAt")
        
    with open(path, 'w') as f:
        f.write(content)

fix_mapper('/Users/mohammadhosein/Development/AzureTest/src/AzureTest.Application/IdeaTracker/IdeaPageQueryUnit.cs', True)
fix_mapper('/Users/mohammadhosein/Development/AzureTest/src/AzureTest.Application/IdeaTracker/IdeaSectionQueryUnit.cs', False)
