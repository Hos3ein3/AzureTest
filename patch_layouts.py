import re

files = [
    '/Users/mohammadhosein/Development/AzureTest/src/AzureTest.MVC/Views/Shared/_Layout.cshtml',
    '/Users/mohammadhosein/Development/AzureTest/src/AzureTest.MVC/Views/Shared/_Layout.Auth.cshtml'
]

pwa_head = """    <title>@ViewData["Title"] - TrackerPro</title>
    
    <!-- Favicon and PWA -->
    <link rel="icon" type="image/png" href="~/favicon.png">
    <link rel="manifest" href="~/manifest.json">
    <meta name="theme-color" content="#3b82f6">
    <link rel="apple-touch-icon" href="~/icon-192.png">"""

pwa_script = """    <!-- PWA Service Worker Registration -->
    <script>
        if ('serviceWorker' in navigator) {
            window.addEventListener('load', () => {
                navigator.serviceWorker.register('/sw.js').then(reg => {
                    console.log('ServiceWorker registration successful with scope: ', reg.scope);
                }).catch(err => {
                    console.error('ServiceWorker registration failed: ', err);
                });
            });
        }
    </script>
</body>"""

for file_path in files:
    try:
        with open(file_path, 'r') as f:
            content = f.read()
        
        # Replace title and add head elements
        content = re.sub(r'<title>.*?<title>', '<title>@ViewData["Title"] - TrackerPro</title>', content, count=1, flags=re.DOTALL)
        
        if 'favicon.png' not in content:
            # specifically find the <title> block and replace it
            content = re.sub(r'<title>.*?</title>', pwa_head, content, count=1, flags=re.DOTALL)
            
        if 'serviceWorker' not in content:
            content = content.replace('</body>', pwa_script)
            
        with open(file_path, 'w') as f:
            f.write(content)
            
        print(f"Patched {file_path}")
    except Exception as e:
        print(f"Error on {file_path}: {e}")

