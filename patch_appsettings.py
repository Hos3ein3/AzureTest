import json
import os

paths = [
    "src/AzureTest.MVC/appsettings.json",
    "src/AzureTest.MVC/appsettings.Development.json",
    "src/AzureTest.MVC/appsettings.ProductionSimulateLocal.json"
]

for path in paths:
    if os.path.exists(path):
        with open(path, "r") as f:
            data = json.load(f)
        
        data["AppIdentitySetting"] = {
            "RedirectAfterRegister": "/Auth/Login",
            "RedirectAfterLogin": "/",
            "RolesForRegister": ["User"],
            "StayLoginAfterRegister": False
        }
        
        with open(path, "w") as f:
            json.dump(data, f, indent=2)

print("Appsettings patched")
