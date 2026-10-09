import re

path = "src/AzureTest.MVC/Views/Shared/_Layout.cshtml"
with open(path, "r") as f: c = f.read()

# 1. Remove old sidebar CSS
css_pattern = re.compile(r'/\* Sidebar Collapse \*/.*?/\* Tooltip \*/.*?}\n', re.DOTALL)
c = css_pattern.sub('', c)
c = re.sub(r'#sidebar\.collapsed.*?\n\s*}\n\s*#sidebar\.collapsed.*?\n\s*}\n', '', c, flags=re.DOTALL)
# A simpler way is to just find the specific lines since I know them:
# Lines 73 to 109 roughly. Let's just use regex for <style> content if we can, but it's mixed with other styles.
