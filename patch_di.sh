sed -i '' '/services.AddScoped<ISampleNoteQueryUnit, SampleNoteQueryUnit>();/a\
\
        // Job Crawler\
        services.AddScoped<AzureTest.Application.JobCrawler.IJobCrawlerCommandUnit, AzureTest.Application.JobCrawler.JobCrawlerCommandUnit>();\
        services.AddScoped<AzureTest.Application.JobCrawler.IJobCrawlerQueryUnit, AzureTest.Application.JobCrawler.JobCrawlerQueryUnit>();\
' /Users/mohammadhosein/Development/AzureTest/src/AzureTest.Application/DependencyInjection.cs
