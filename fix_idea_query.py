import re

with open('/Users/mohammadhosein/Development/AzureTest/src/AzureTest.Application/IdeaTracker/IdeaQueryUnit.cs', 'r') as f:
    content = f.read()

content = content.replace("x.Title.Contains(query.Filter.Search, StringComparison.OrdinalIgnoreCase) || \n                                         x.Content.Contains(query.Filter.Search, StringComparison.OrdinalIgnoreCase)",
                          "(x.Title != null && x.Title.Contains(query.Filter.Search, StringComparison.OrdinalIgnoreCase)) || (x.Content != null && x.Content.Contains(query.Filter.Search, StringComparison.OrdinalIgnoreCase))")

# Also the user might have null titles?
content = content.replace("x.Title.Contains(", "x.Title != null && x.Title.Contains(")
content = content.replace("x.Content.Contains(", "x.Content != null && x.Content.Contains(")

with open('/Users/mohammadhosein/Development/AzureTest/src/AzureTest.Application/IdeaTracker/IdeaQueryUnit.cs', 'w') as f:
    f.write(content)
