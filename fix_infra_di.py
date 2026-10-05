import re

di_file = "src/AzureTest.Infrastructure/DependencyInjection.cs"
with open(di_file, "r") as f:
    content = f.read()

if "IHouseCrawlerService" not in content:
    content = content.replace(
        "services.AddHttpClient<IIndCrawlerService, IndCrawlerService>();",
        "services.AddHttpClient<IIndCrawlerService, IndCrawlerService>();\n        services.AddScoped<AzureTest.Application.Interfaces.Services.IHouseCrawlerService, AzureTest.Infrastructure.Services.HouseCrawlerService>();"
    )
    with open(di_file, "w") as f:
        f.write(content)
