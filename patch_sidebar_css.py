import re

path = "src/AzureTest.MVC/Views/Shared/_Sidebar.cshtml"
with open(path, "r") as f: c = f.read()

css_block = """    /* Dropdown overrides for collapsed state */
    #sidebar.collapsed details summary .sidebar-text,
    #sidebar.collapsed details summary i.fa-chevron-down {
        display: none;
    }
    #sidebar.collapsed details ul {
        display: none;
    }"""

c = c.replace(css_block, "")

with open(path, "w") as f: f.write(c)
