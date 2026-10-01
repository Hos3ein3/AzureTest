using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.JobCrawler;
using AzureTest.Domain.Documents;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.JobCrawler.Queries;

public record JobSearchHistoryFilter(Guid UserId, int Page = 1, int PageSize = 10);

public record GetUserJobSearchHistoryQuery(JobSearchHistoryFilter Filter) : IQuery<IReadOnlyList<JobSearchHistoryDto>>;

public class GetUserJobSearchHistoryQueryHandler : BaseQueryHandler<GetUserJobSearchHistoryQuery, IReadOnlyList<JobSearchHistoryDto>>
{
    private readonly INoSqlUnitOfWork _unitOfWork;

    public GetUserJobSearchHistoryQueryHandler(INoSqlUnitOfWork unitOfWork, ILogger<GetUserJobSearchHistoryQueryHandler> logger) 
        : base(logger)
    {
        _unitOfWork = unitOfWork;
    }

    protected override async Task<IReadOnlyList<JobSearchHistoryDto>> ExecuteAsync(GetUserJobSearchHistoryQuery query, CancellationToken cancellationToken)
    {
        var records = await _unitOfWork.JobSearchHistories.FindAsync(x => x.UserId == query.Filter.UserId);
        
        var result = records
            .OrderByDescending(x => x.SearchDate)
            .Take(50) // Max history to show
            .Select(MapToDto)
            .ToList();

        return result;
    }

    private static JobSearchHistoryDto MapToDto(JobSearchHistory doc) => new(
        doc.Id.ToString(),
        doc.UserId,
        doc.Position,
        doc.Location,
        doc.DatePosted,
        doc.Source,
        doc.SearchDate
    );
}
