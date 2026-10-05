import re

file = "src/AzureTest.MVC/Controllers/Api/AnalyticsEndpointController.cs"
with open(file, "r") as f:
    content = f.read()

content = content.replace('IQueryHandler<GetResumeByIdQuery, ResumeDto>', 'IQueryHandler<GetResumeByIdQuery, ResumeDto?>')

with open(file, "w") as f:
    f.write(content)
