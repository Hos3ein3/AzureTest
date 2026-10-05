import re

for view in ["src/AzureTest.MVC/Views/Shared/Widgets/_ResumeStatsA.cshtml", "src/AzureTest.MVC/Views/Shared/Widgets/_ResumeStatsB.cshtml"]:
    with open(view, "r") as f:
        content = f.read()
    
    content = content.replace(
        "AzureTest.Application.Resumes.Queries.GetResumeAnalytics.ResumeAnalyticsDto",
        "AzureTest.Contracts.DTOs.Resumes.ResumeAnalyticsDto"
    )
    
    with open(view, "w") as f:
        f.write(content)
