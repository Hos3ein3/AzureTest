import re

file = "src/AzureTest.MVC/Controllers/AnalyticsEndpointController.cs"
with open(file, "r") as f:
    content = f.read()

content = content.replace("using AzureTest.Application.Abstractions;", "using AzureTest.Application.Abstractions;\nusing AzureTest.Application.Messaging;\nusing AzureTest.Contracts.DTOs.Resumes;")
content = content.replace("IQueryHandler<GetResumeAnalyticsQuery, ResumeAnalyticsDto?>", "IQueryHandler<GetResumeAnalyticsQuery, ResumeAnalyticsDto>")

with open(file, "w") as f:
    f.write(content)
