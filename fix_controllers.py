import re
import os

def fix_company(path):
    with open(path, 'r') as f:
        c = f.read()
    c = c.replace('result.TotalProcessed', 'result.Value.TotalProcessed')
    c = c.replace('result.NewlyAdded', 'result.Value.NewlyAdded')
    c = c.replace('if (result)\n', 'if (result.IsSuccess)\n')
    c = c.replace('if (!result)\n', 'if (result.IsFailure)\n')
    with open(path, 'w') as f:
        f.write(c)

def fix_auth(path):
    with open(path, 'r') as f:
        c = f.read()
    c = c.replace('result.Success', 'result.IsSuccess')
    c = c.replace('result.Token', 'result.Value.Token')
    c = c.replace('result.RefreshToken', 'result.Value.RefreshToken')
    c = c.replace('result.Errors', 'result.Error') # wait, Result has string Error, not List<string> Errors, maybe check if it was result.Errors
    with open(path, 'w') as f:
        f.write(c)
        
def fix_resume(path):
    with open(path, 'r') as f:
        c = f.read()
    c = c.replace('new { result}', 'new { id = result.Value }')
    c = c.replace('new { result }', 'new { id = result.Value }')
    c = c.replace('if (!result)', 'if (result.IsFailure)')
    c = c.replace('if (result)', 'if (result.IsSuccess)')
    with open(path, 'w') as f:
        f.write(c)

fix_company("src/AzureTest.MVC/Controllers/CompanyController.cs")
fix_auth("src/AzureTest.MVC/Controllers/AuthController.cs")
if os.path.exists("src/AzureTest.MVC/Controllers/ResumeController.cs"):
    fix_resume("src/AzureTest.MVC/Controllers/ResumeController.cs")

