import re

path = "src/AzureTest.MVC/Views/FileExplorer/Index.cshtml"
with open(path, "r") as f:
    content = f.read()

# Update context menu visibility logic
visibility_patch = """
            const isDir = isItem && target.getAttribute('data-isdir') === 'true';
            document.getElementById('cm-download').style.display = (isItem && !isDir) ? 'block' : 'none';
            document.getElementById('cm-copydl').style.display = (isItem && !isDir) ? 'block' : 'none';
            document.getElementById('cm-download-sep').style.display = (isItem && !isDir) ? 'block' : 'none';
"""
content = re.sub(
    r'const isDir = isItem && target\.getAttribute\(\'data-isdir\'\) === \'true\';\s*document\.getElementById\(\'cm-download\'\)\.style\.display = \(isItem && !isDir\) \? \'block\' : \'none\';\s*document\.getElementById\(\'cm-download-sep\'\)\.style\.display = \(isItem && !isDir\) \? \'block\' : \'none\';',
    visibility_patch,
    content,
    flags=re.DOTALL
)

# Update action logic
action_patch = """
            if (action === 'download') {
                const path = contextTarget.getAttribute('data-path');
                const url = '/' + path.split('/').map(p => encodeURIComponent(p)).join('/');
                const a = document.createElement('a');
                a.href = url;
                a.download = contextTarget.getAttribute('data-name');
                document.body.appendChild(a);
                a.click();
                document.body.removeChild(a);
            }
            else if (action === 'copydl') {
                const path = contextTarget.getAttribute('data-path');
                const url = window.location.origin + '/' + path.split('/').map(p => encodeURIComponent(p)).join('/');
                navigator.clipboard.writeText(url).then(() => {
                    // Optional toast
                }).catch(err => {
                    alert("Failed to copy URL: " + err);
                });
            }
"""
content = re.sub(
    r'if \(action === \'download\'\) \{.*?document\.body\.removeChild\(a\);\s*\}',
    action_patch,
    content,
    flags=re.DOTALL
)

with open(path, "w") as f:
    f.write(content)
print("Patched FileExplorer JS")
