import re

with open('/Users/mohammadhosein/Development/AzureTest/src/AzureTest.MVC/Views/IdeaTracker/Details.cshtml', 'r') as f:
    content = f.read()

sub_ideas = re.search(r'(<!-- Sub-Pages Section -->.*?)(<!-- Document Pages Section -->)', content, re.DOTALL)
doc_pages = re.search(r'(<!-- Document Pages Section -->.*?)(</div>\s*<!-- Sticky Table of Contents \(Right Sidebar\) -->)', content, re.DOTALL)

if sub_ideas and doc_pages:
    new_content = content.replace(sub_ideas.group(1), "")
    new_content = new_content.replace(doc_pages.group(1), doc_pages.group(1) + "\n" + sub_ideas.group(1))
    
    # "when I click on Document Pages, it doesnt open" -> Need to fix the title link
    # Find: <div class="flex-1 min-w-0">\s*<h4 class="font-bold text-slate-900 truncate">@pageDto.Title</h4>
    # Replace with: <div class="flex-1 min-w-0">\s*<a asp-controller="IdeaPage" asp-action="Details" asp-route-id="@pageDto.Id" class="font-bold text-slate-900 truncate hover:text-brand-600 block">@pageDto.Title</a>
    
    new_content = re.sub(r'<h4 class="font-bold text-slate-900 truncate">@pageDto\.Title</h4>',
                         r'<a asp-controller="IdeaPage" asp-action="Details" asp-route-id="@pageDto.Id" class="font-bold text-slate-900 truncate hover:text-brand-600 block">@pageDto.Title</a>',
                         new_content)
    
    with open('/Users/mohammadhosein/Development/AzureTest/src/AzureTest.MVC/Views/IdeaTracker/Details.cshtml', 'w') as f:
        f.write(new_content)

