import re

di_file = "src/AzureTest.Application/DependencyInjection.cs"
with open(di_file, "r") as f:
    content = f.read()

if "IHouseCrawlerCommandUnit" not in content:
    content = content.replace(
        "services.AddScoped<IJobCrawlerQueryUnit, JobCrawlerQueryUnit>();",
        "services.AddScoped<IJobCrawlerQueryUnit, JobCrawlerQueryUnit>();\n\n        services.AddScoped<ICommandHandler<AzureTest.Application.HouseCrawler.Commands.CreateHouseSearchHistoryCommand, Abstractions.Result>, AzureTest.Application.HouseCrawler.Commands.CreateHouseSearchHistoryCommandHandler>();\n        services.AddScoped<ICommandHandler<AzureTest.Application.HouseCrawler.Commands.SaveHouseCredentialCommand, Abstractions.Result>, AzureTest.Application.HouseCrawler.Commands.SaveHouseCredentialCommandHandler>();\n        services.AddScoped<IQueryHandler<AzureTest.Application.HouseCrawler.Queries.GetUserHouseSearchHistoryQuery, System.Collections.Generic.IReadOnlyList<AzureTest.Domain.Documents.HouseSearchHistory>>, AzureTest.Application.HouseCrawler.Queries.GetUserHouseSearchHistoryQueryHandler>();\n        services.AddScoped<IQueryHandler<AzureTest.Application.HouseCrawler.Queries.GetUserHouseCredentialsQuery, System.Collections.Generic.IReadOnlyList<AzureTest.Domain.Documents.HouseCrawlerCredential>>, AzureTest.Application.HouseCrawler.Queries.GetUserHouseCredentialsQueryHandler>();\n        services.AddScoped<AzureTest.Application.HouseCrawler.Interfaces.IHouseCrawlerCommandUnit, AzureTest.Application.HouseCrawler.HouseCrawlerCommandUnit>();\n        services.AddScoped<AzureTest.Application.HouseCrawler.Interfaces.IHouseCrawlerQueryUnit, AzureTest.Application.HouseCrawler.HouseCrawlerQueryUnit>();"
    )
    with open(di_file, "w") as f:
        f.write(content)
