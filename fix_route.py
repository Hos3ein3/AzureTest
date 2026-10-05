import re

file = "src/AzureTest.MVC/Controllers/Api/AnalyticsEndpointController.cs"
with open(file, "r") as f:
    content = f.read()

content = content.replace('[Route("api/[controller]")]', '[Route("api/analytics")]')

with open(file, "w") as f:
    f.write(content)
