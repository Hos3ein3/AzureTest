with open('/Users/mohammadhosein/Development/AzureTest/src/AzureTest.MVC/Views/IdeaTracker/Details.cshtml', 'r') as f:
    content = f.read()

import re

# 1. Fix Cover Banner styling
# Current: <div class="h-64 md:h-80 w-full relative shrink-0 @(string.IsNullOrEmpty(Model.CoverImage) ? $"bg-gradient-to-r from-{Model.ThemeColor}-400 to-{Model.ThemeColor}-600" : "")">
# New: <div class="h-64 md:h-80 w-full relative shrink-0" style="@(string.IsNullOrEmpty(Model.CoverImage) && !string.IsNullOrEmpty(Model.ThemeColor) ? $"background: linear-gradient(135deg, {Model.ThemeColor} 0%, rgba(0,0,0,0.3) 100%); background-color: {Model.ThemeColor};" : "")">
old_cover = r'<div class="h-64 md:h-80 w-full relative shrink-0 @\(string\.IsNullOrEmpty\(Model\.CoverImage\).*?\)'
new_cover = r'<div class="h-64 md:h-80 w-full relative shrink-0" style="@(string.IsNullOrEmpty(Model.CoverImage) && !string.IsNullOrEmpty(Model.ThemeColor) ? $"background: linear-gradient(135deg, {Model.ThemeColor} 0%, rgba(0,0,0,0.3) 100%); background-color: {Model.ThemeColor};" : "")"'
content = re.sub(old_cover, new_cover, content)

# 2. Fix the Editor container
# Current: <input type="hidden" id="hiddenContent" value="@Model.Content" />
#          <div id="editorjs" class="prose prose-slate max-w-none editor-readonly-mode"></div>
# New: <div id="content-container" class="prose prose-slate prose-img:rounded-xl prose-img:shadow-sm max-w-none">@Html.Raw(Model.Content)</div>
old_editor = r'<input type="hidden" id="hiddenContent" value="@Model\.Content" />\s*<div id="editorjs" class="prose prose-slate max-w-none editor-readonly-mode"></div>'
new_editor = r'<div id="content-container" class="prose prose-slate prose-img:rounded-xl prose-img:shadow-sm max-w-none">@Html.Raw(Model.Content)</div>'
content = re.sub(old_editor, new_editor, content)


# 3. Replace entire script section
scripts = """@section Scripts {
    <script>
        document.addEventListener('DOMContentLoaded', () => {
            buildTableOfContents();

            function buildTableOfContents() {
                const tocContainer = document.getElementById('toc-container');
                const contentContainer = document.getElementById('content-container');
                const headers = contentContainer.querySelectorAll('h1, h2, h3, h4');
                
                if (headers.length === 0) {
                    tocContainer.innerHTML = '<p class="text-xs text-slate-400 italic">No headings found.</p>';
                    return;
                }

                headers.forEach((header, index) => {
                    // Create an anchor id if it doesn't have one
                    if (!header.id) {
                        const anchorId = 'heading-' + index;
                        header.id = anchorId;
                    }

                    const a = document.createElement('a');
                    a.href = '#' + header.id;
                    a.textContent = header.textContent;
                    a.className = 'block py-1 hover:text-brand-600 transition-colors truncate toc-item';
                    
                    // Indent based on heading level
                    const level = parseInt(header.tagName.substring(1));
                    if (level === 3) a.classList.add('ml-3');
                    if (level === 4) a.classList.add('ml-6');

                    tocContainer.appendChild(a);
                });
            }
        });
    </script>
}"""

content = re.sub(r'@section Scripts \{.*?\}', scripts, content, flags=re.DOTALL)

with open('/Users/mohammadhosein/Development/AzureTest/src/AzureTest.MVC/Views/IdeaTracker/Details.cshtml', 'w') as f:
    f.write(content)
