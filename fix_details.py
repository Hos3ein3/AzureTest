import re

with open('/Users/mohammadhosein/Development/AzureTest/src/AzureTest.MVC/Views/IdeaTracker/Details.cshtml', 'r') as f:
    content = f.read()

# Replace everything from @section Scripts to the end of the file
scripts_block = """@section Scripts {
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
}
"""

content = re.sub(r'@section Scripts \{.*', scripts_block, content, flags=re.DOTALL)

with open('/Users/mohammadhosein/Development/AzureTest/src/AzureTest.MVC/Views/IdeaTracker/Details.cshtml', 'w') as f:
    f.write(content)

