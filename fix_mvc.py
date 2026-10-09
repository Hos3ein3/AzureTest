import re

# PagesController
path = "src/AzureTest.MVC/Controllers/PagesController.cs"
with open(path, "r") as f: c = f.read()
c = c.replace("public class PagesController : BaseController", "public class PagesController : Controller")
c = c.replace("GetUserId()", "Guid.Parse(User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier))")
c = c.replace("using Microsoft.AspNetCore.Authorization;", "using Microsoft.AspNetCore.Authorization;\nusing System.Security.Claims;")
with open(path, "w") as f: f.write(c)

# Read.cshtml
path2 = "src/AzureTest.MVC/Views/Pages/Read.cshtml"
with open(path2, "r") as f: c2 = f.read()
c2 = c2.replace('@inject AzureTest.MVC.Services.ICurrentUserService CurrentUser', '@using System.Security.Claims')
c2 = c2.replace('CurrentUser.UserId', 'User.FindFirstValue(ClaimTypes.NameIdentifier)')
c2 = c2.replace('CurrentUser.IsAdmin', 'User.IsInRole("Admin")')
with open(path2, "w") as f: f.write(c2)

print("Fixed MVC files")
