import re

path = "src/AzureTest.MVC/Views/Shared/_Layout.cshtml"
with open(path, "r") as f: content = f.read()

# Remove old sidebar CSS
css_pattern = re.compile(r'/\* Sidebar Collapse \*/.*?(?=/\* Scrollbar override \*/)', re.DOTALL)
content = css_pattern.sub('', content)

# Remove aside block
aside_pattern = re.compile(r'<!-- Sidebar -->\s*<aside id="sidebar".*?</aside>', re.DOTALL)
content = aside_pattern.sub('<partial name="_Sidebar" />', content)

# Add ID to mobile menu button so we can hook it up in _Sidebar
content = content.replace(
    'class="md:hidden text-slate-500 hover:text-slate-700"',
    'id="mobileMenuBtn" class="md:hidden text-slate-500 hover:text-slate-700 focus:outline-none"'
)

# Remove old sidebarToggle JS
js_pattern = re.compile(r'const sb = document.getElementById\("sidebar"\);.*?\}\);', re.DOTALL)
content = js_pattern.sub('// Sidebar logic moved to _Sidebar.cshtml', content)

with open(path, "w") as f: f.write(content)
