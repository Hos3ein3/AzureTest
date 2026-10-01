import re

with open('/Users/mohammadhosein/Development/AzureTest/src/AzureTest.MVC/Views/Shared/_Layout.cshtml', 'r') as f:
    content = f.read()

# Add CSS styles in head
style_block = """
    <!-- Custom Styles -->
    <style>
        #sidebar {
            transition: width 0.3s ease;
        }
        #sidebar.collapsed {
            width: 4.5rem !important;
        }
        #sidebar.collapsed .sidebar-text {
            display: none !important;
        }
        #sidebar.collapsed .logo-brand {
            display: none !important;
        }
        #sidebar.collapsed .logo-container {
            justify-content: center !important;
            padding: 0 !important;
        }
        #sidebar.collapsed .logo-icon {
            display: none !important;
        }
        #sidebar.collapsed #sidebarToggle {
            margin-left: 0 !important;
            margin: 0 auto;
        }
        #sidebar.collapsed .nav-item-link {
            justify-content: center !important;
            padding-left: 0 !important;
            padding-right: 0 !important;
            position: relative;
        }
        #sidebar.collapsed .nav-item-link i {
            margin: 0 !important;
        }
        /* Tooltip */
        #sidebar.collapsed .nav-item-link:hover::after {
            content: attr(data-title);
            position: absolute;
            left: calc(100% + 8px);
            background-color: #1e293b;
            color: #fff;
            padding: 4px 8px;
            border-radius: 4px;
            font-size: 0.875rem;
            white-space: nowrap;
            z-index: 50;
            pointer-events: none;
            opacity: 1;
        }
    </style>
"""
if "<style>" not in content:
    content = content.replace('</head>', style_block + '\n</head>')

# Replace the sidebar toggle JS
new_js = """
<script>
    document.addEventListener("DOMContentLoaded", function() {
        const sb = document.getElementById("sidebar");
        const toggleBtn = document.getElementById("sidebarToggle");
        
        // Restore state from localStorage
        if(localStorage.getItem('sidebarCollapsed') === 'true') {
            sb.classList.add("collapsed");
        }

        toggleBtn?.addEventListener("click", function() {
            sb.classList.toggle("collapsed");
            localStorage.setItem('sidebarCollapsed', sb.classList.contains("collapsed"));
        });
    });
</script>
"""
# Replace old JS
content = re.sub(r'<script>\s*document\.getElementById\("sidebarToggle"\)\?\.addEventListener.*?</script>', new_js, content, flags=re.DOTALL)


# Fix the logo container
old_logo = r'<div class="h-16 flex items-center px-6 border-b border-slate-800">.*?</div>'
new_logo = """<div class="h-16 flex items-center px-6 border-b border-slate-800 logo-container transition-all">
            <i class="fa-solid fa-briefcase text-brand-500 text-2xl mr-3 logo-icon"></i>
            <span class="logo-brand text-xl font-bold tracking-wider overflow-hidden whitespace-nowrap">Tracker<span class="text-brand-500">Pro</span></span>
            <button id="sidebarToggle" class="ml-auto text-slate-400 hover:text-white transition-all p-2 rounded-lg hover:bg-slate-800"><i class="fa-solid fa-bars"></i></button>
        </div>"""
content = re.sub(old_logo, new_logo, content, flags=re.DOTALL)

# Add nav-item-link and data-title to all links
# E.g. <a asp-area="" asp-controller="Home" asp-action="Index" class="flex items-center px-3 py-2.5 rounded-lg transition-colors group @GetActiveClasses("Home")">
# We want to extract the text inside span and put it in data-title
def link_replacer(match):
    full_link = match.group(0)
    # find the text
    text_match = re.search(r'<span[^>]*>(.*?)</span>', full_link)
    text = "Menu"
    if text_match:
        # strip tags inside span if any (e.g. for Sample Notes)
        text = re.sub(r'<[^>]+>', '', text_match.group(1)).strip()
        
    # Inject nav-item-link and data-title
    mod_link = full_link.replace('class="flex', f'data-title="{text}" class="nav-item-link flex')
    return mod_link

content = re.sub(r'<a asp-area="" asp-controller="[^"]+" asp-action="[^"]+" class="flex items-center[^>]+>.*?</a>', link_replacer, content, flags=re.DOTALL)


with open('/Users/mohammadhosein/Development/AzureTest/src/AzureTest.MVC/Views/Shared/_Layout.cshtml', 'w') as f:
    f.write(content)

