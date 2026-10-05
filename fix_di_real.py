import re

di_file = "src/AzureTest.Application/DependencyInjection.cs"
with open(di_file, "r") as f:
    content = f.read()

if "IHouseCrawlerCommandUnit" not in content:
    content = content.replace(
        "// Job Crawler\n        services.AddScoped<AzureTest.Application.JobCrawler.IJobCrawlerCommandUnit, AzureTest.Application.JobCrawler.JobCrawlerCommandUnit>();\n        services.AddScoped<AzureTest.Application.JobCrawler.IJobCrawlerQueryUnit, AzureTest.Application.JobCrawler.JobCrawlerQueryUnit>();",
        "// Job Crawler\n        services.AddScoped<AzureTest.Application.JobCrawler.IJobCrawlerCommandUnit, AzureTest.Application.JobCrawler.JobCrawlerCommandUnit>();\n        services.AddScoped<AzureTest.Application.JobCrawler.IJobCrawlerQueryUnit, AzureTest.Application.JobCrawler.JobCrawlerQueryUnit>();\n\n        // House Crawler\n        services.AddScoped<AzureTest.Application.HouseCrawler.Interfaces.IHouseCrawlerCommandUnit, AzureTest.Application.HouseCrawler.HouseCrawlerCommandUnit>();\n        services.AddScoped<AzureTest.Application.HouseCrawler.Interfaces.IHouseCrawlerQueryUnit, AzureTest.Application.HouseCrawler.HouseCrawlerQueryUnit>();"
    )
    with open(di_file, "w") as f:
        f.write(content)
