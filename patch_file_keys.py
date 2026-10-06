import os

file = "src/AzureTest.MVC/Views/FileExplorer/Index.cshtml"
with open(file, "r") as f:
    content = f.read()

js_addition = """
        // Keyboard Navigation (Arrow Keys)
        document.addEventListener('keydown', function(e) {
            // Only trigger if no modal/input is active
            if (document.activeElement.tagName === 'INPUT' || document.activeElement.tagName === 'TEXTAREA') return;
            
            const rows = Array.from(document.querySelectorAll('.file-item'));
            if (rows.length === 0) return;

            let currentIndex = rows.findIndex(r => r.classList.contains('selected'));

            if (e.key === 'ArrowDown') {
                e.preventDefault();
                let nextIndex = currentIndex < rows.length - 1 ? currentIndex + 1 : rows.length - 1;
                if (currentIndex === -1) nextIndex = 0;
                selectRow(rows[nextIndex], e);
                rows[nextIndex].scrollIntoView({ block: 'nearest' });
            } 
            else if (e.key === 'ArrowUp') {
                e.preventDefault();
                let prevIndex = currentIndex > 0 ? currentIndex - 1 : 0;
                if (currentIndex === -1) prevIndex = rows.length - 1;
                selectRow(rows[prevIndex], e);
                rows[prevIndex].scrollIntoView({ block: 'nearest' });
            }
            else if (e.key === 'Enter') {
                e.preventDefault();
                if (currentIndex !== -1) {
                    const row = rows[currentIndex];
                    const isDir = row.getAttribute('data-isdir') === 'true';
                    const path = row.getAttribute('data-path');
                    if (isDir) {
                        window.location.href = '?path=' + encodeURIComponent(path);
                    } else {
                        // maybe preview or download
                        if (!previewVisible) togglePreview();
                    }
                }
            }
        });
"""

if "ArrowDown" not in content:
    content = content.replace("function selectRow(row, e) {", js_addition + "\n        function selectRow(row, e) {")
    with open(file, "w") as f:
        f.write(content)
print("FileExplorer Keyboard Nav Added")
