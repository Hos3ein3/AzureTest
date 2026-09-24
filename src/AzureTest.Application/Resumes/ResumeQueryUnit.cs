using System.Collections.Generic;
using AzureTest.Application.Messaging;
using AzureTest.Application.Resumes.Queries.GetAllResumes;
using AzureTest.Application.Resumes.Queries.GetResumeById;
using AzureTest.Application.Resumes.Queries.GetStatusLogsByResumeId;
using AzureTest.Contracts.DTOs.Resumes;
using AzureTest.Contracts.DTOs.Common;

namespace AzureTest.Application.Resumes;

public sealed class ResumeQueryUnit : IResumeQueryUnit
{
    public IQueryHandler<GetAllResumesQuery, PagedResult<ResumeDto>> GetAll { get; }
    public IQueryHandler<GetResumeByIdQuery, ResumeDto?> GetById { get; }
    public IQueryHandler<AzureTest.Application.Resumes.Queries.GetResumeAnalytics.GetResumeAnalyticsQuery, ResumeAnalyticsDto> GetAnalytics { get; }
    public IQueryHandler<GetStatusLogsByResumeIdQuery, IReadOnlyList<StatusLogDto>> GetStatusLogs { get; }
    public IQueryHandler<GetStatusLogsByResumeIdQuery, IReadOnlyList<StatusLogDto>> GetStatusLogsByResumeId => GetStatusLogs;

    public ResumeQueryUnit(
        IQueryHandler<GetAllResumesQuery, PagedResult<ResumeDto>> getAll,
        IQueryHandler<GetResumeByIdQuery, ResumeDto?> getById,
        IQueryHandler<AzureTest.Application.Resumes.Queries.GetResumeAnalytics.GetResumeAnalyticsQuery, ResumeAnalyticsDto> getAnalytics,
        IQueryHandler<GetStatusLogsByResumeIdQuery, IReadOnlyList<StatusLogDto>> getStatusLogs)
    {
        GetAll = getAll;
        GetById = getById;
        GetAnalytics = getAnalytics;
        GetStatusLogs = getStatusLogs;
    }
}
