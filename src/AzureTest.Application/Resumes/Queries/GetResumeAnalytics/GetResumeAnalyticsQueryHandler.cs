using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Resumes;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AzureTest.Application.Resumes.Queries.GetResumeAnalytics;

public sealed record GetResumeAnalyticsQuery() : IQuery<ResumeAnalyticsDto>;

public sealed class GetResumeAnalyticsQueryHandler : BaseQueryHandler<GetResumeAnalyticsQuery, ResumeAnalyticsDto>
{
    private readonly IResumeRepository _resumeRepository;

    public GetResumeAnalyticsQueryHandler(
        IResumeRepository resumeRepository,
        ILogger<GetResumeAnalyticsQueryHandler> logger)
        : base(logger)
    {
        _resumeRepository = resumeRepository;
    }

    protected override async Task<ResumeAnalyticsDto> ExecuteAsync(GetResumeAnalyticsQuery query, CancellationToken cancellationToken)
    {
        var resumes = await _resumeRepository.GetAllAsync();

        var statusCounts = resumes
            .GroupBy(r => r.Status.ToString())
            .ToDictionary(g => g.Key, g => g.Count());
        
        var dateCounts = resumes
            .GroupBy(r => r.CreatedOn.ToLocalTime().ToString("yyyy-MM-dd"))
            .OrderBy(g => g.Key)
            .ToDictionary(g => g.Key, g => g.Count());

        return new ResumeAnalyticsDto(statusCounts, dateCounts);
    }
}
