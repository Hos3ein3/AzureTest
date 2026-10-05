import re

file = "src/AzureTest.MVC/Controllers/AnalyticsEndpointController.cs"
with open(file, "r") as f:
    content = f.read()

content = content.replace("GetResumeAnalyticsQuery", "GetResumeByIdQuery")
content = content.replace("AzureTest.Application.Resumes.Queries.GetResumeAnalytics", "AzureTest.Application.Resumes.Queries.GetResumeById")
content = content.replace("ResumeAnalyticsDto", "ResumeDto")

with open(file, "w") as f:
    f.write(content)

for view in ["src/AzureTest.MVC/Views/Shared/Widgets/_ResumeStatsA.cshtml", "src/AzureTest.MVC/Views/Shared/Widgets/_ResumeStatsB.cshtml"]:
    with open(view, "r") as f:
        content = f.read()
    content = content.replace("ResumeAnalyticsDto", "ResumeDto")
    with open(view, "w") as f:
        f.write(content)
