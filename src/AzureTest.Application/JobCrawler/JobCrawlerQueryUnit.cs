using System.Collections.Generic;
using AzureTest.Application.JobCrawler.Queries;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.JobCrawler;

namespace AzureTest.Application.JobCrawler;

public interface IJobCrawlerQueryUnit
{
    IQueryHandler<GetUserJobSearchHistoryQuery, IReadOnlyList<JobSearchHistoryDto>> GetHistory { get; }
}

public class JobCrawlerQueryUnit : IJobCrawlerQueryUnit
{
    public IQueryHandler<GetUserJobSearchHistoryQuery, IReadOnlyList<JobSearchHistoryDto>> GetHistory { get; }

    public JobCrawlerQueryUnit(IQueryHandler<GetUserJobSearchHistoryQuery, IReadOnlyList<JobSearchHistoryDto>> getHistory)
    {
        GetHistory = getHistory;
    }
}
