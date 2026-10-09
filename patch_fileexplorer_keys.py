import re

path = "src/AzureTest.MVC/Views/FileExplorer/Index.cshtml"
with open(path, "r") as f:
    content = f.read()

keys_logic = """
        // --- Keyboard Navigation ---
        document.addEventListener('keydown', (e) => {
            const isInputFocused = document.activeElement.tagName === 'INPUT' || document.activeElement.tagName === 'TEXTAREA';
            if (isInputFocused) return;
            
            const menuOpen = !document.getElementById('contextMenu').classList.contains('hidden');
            if (menuOpen) return;

            const items = Array.from(document.querySelectorAll('.file-item'));
            if (items.length === 0) return;

            let currentIndex = items.findIndex(el => el.classList.contains('selected'));

            if (e.key === 'ArrowDown') {
                e.preventDefault();
                let nextIndex = currentIndex < items.length - 1 ? currentIndex + 1 : 0;
                selectItem(items[nextIndex], e);
                items[nextIndex].scrollIntoView({ block: 'nearest' });
            } 
            else if (e.key === 'ArrowUp') {
                e.preventDefault();
                let prevIndex = currentIndex > 0 ? currentIndex - 1 : items.length - 1;
                selectItem(items[prevIndex], e);
                items[prevIndex].scrollIntoView({ block: 'nearest' });
            }
            else if (e.key === 'Enter') {
                if (currentIndex !== -1) {
                    e.preventDefault();
                    const path = items[currentIndex].getAttribute('data-path');
                    const isDir = items[currentIndex].getAttribute('data-isdir') === 'true';
                    openItem(path, isDir);
                }
            }
            else if (e.key === 'Backspace') {
                e.preventDefault();
                const parentPath = currentPath.substring(0, currentPath.lastIndexOf('/'));
                window.location.href = '?path=' + encodeURIComponent(parentPath);
            }
        });
"""
content = content.replace("// --- Context Menu Logic ---", keys_logic + "\n        // --- Context Menu Logic ---")

with open(path, "w") as f:
    f.write(content)
print("Patched FileExplorer Keys")
