using AzureTest.Application.Resumes.Queries.GetAllResumes;
using AzureTest.Application.Resumes.Queries.GetResumeById;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Resumes;
using AzureTest.Contracts.DTOs.Common;

namespace AzureTest.Application.Resumes;

/// <summary>
/// Aggregates all Resume query handlers into a single injectable unit.
/// </summary>
public interface IResumeQueryUnit
{
    IQueryHandler<GetAllResumesQuery, PagedResult<ResumeDto>> GetAll { get; }
    IQueryHandler<GetResumeByIdQuery, ResumeDto?> GetById { get; }
    IQueryHandler<AzureTest.Application.Resumes.Queries.GetResumeAnalytics.GetResumeAnalyticsQuery, ResumeAnalyticsDto> GetAnalytics { get; }
}
