using AzureTest.Application.JobCrawler.Commands;
using AzureTest.Application.Messaging;

namespace AzureTest.Application.JobCrawler;

public interface IJobCrawlerCommandUnit
{
    ICommandHandler<CreateJobSearchHistoryCommand, string> CreateHistory { get; }
}

public class JobCrawlerCommandUnit : IJobCrawlerCommandUnit
{
    public ICommandHandler<CreateJobSearchHistoryCommand, string> CreateHistory { get; }

    public JobCrawlerCommandUnit(ICommandHandler<CreateJobSearchHistoryCommand, string> createHistory)
    {
        CreateHistory = createHistory;
    }
}
