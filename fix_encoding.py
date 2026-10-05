import re

file1 = "src/AzureTest.MVC/Views/Error/Error.cshtml"
with open(file1, "r") as f:
    content1 = f.read()

content1 = content1.replace("var errorContent = $@\"", "var errorContent = $@\"").replace("{exception?.Message}", "{System.Net.WebUtility.HtmlEncode(exception?.Message)}").replace("{exception?.InnerException?.Message}", "{System.Net.WebUtility.HtmlEncode(exception?.InnerException?.Message)}").replace("{exception?.StackTrace}", "{System.Net.WebUtility.HtmlEncode(exception?.StackTrace)}")

with open(file1, "w") as f:
    f.write(content1)

file2 = "src/AzureTest.MVC/Views/Auth/AccessDenied.cshtml"
with open(file2, "r") as f:
    content2 = f.read()

content2 = content2.replace("{returnUrl}", "{System.Net.WebUtility.HtmlEncode(returnUrl)}")

with open(file2, "w") as f:
    f.write(content2)

