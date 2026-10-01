sed -i '' '/services.AddScoped<AzureTest.Application.JobCrawler.IJobCrawlerQueryUnit/a\
\
        // Idea Tracker\
        services.AddScoped<AzureTest.Application.IdeaTracker.IIdeaCommandUnit, AzureTest.Application.IdeaTracker.IdeaCommandUnit>();\
        services.AddScoped<AzureTest.Application.IdeaTracker.IIdeaQueryUnit, AzureTest.Application.IdeaTracker.IdeaQueryUnit>();\
' /Users/mohammadhosein/Development/AzureTest/src/AzureTest.Application/DependencyInjection.cs
