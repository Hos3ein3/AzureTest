using System.Collections.Generic;
using AzureTest.Application.Messaging;
using AzureTest.Application.Resumes.Queries.GetAllResumes;
using AzureTest.Application.Resumes.Queries.GetResumeById;
using AzureTest.Application.Resumes.Queries.GetResumeStatusLogsByResumeId;
using AzureTest.Contracts.DTOs.Resumes;
using AzureTest.Contracts.DTOs.Common;

namespace AzureTest.Application.Resumes;

public sealed class ResumeQueryUnit : IResumeQueryUnit
{
    public IQueryHandler<GetAllResumesQuery, PagedResult<ResumeDto>> GetAll { get; }
    public IQueryHandler<GetResumeByIdQuery, ResumeDto?> GetById { get; }
    public IQueryHandler<AzureTest.Application.Resumes.Queries.GetResumeAnalytics.GetResumeAnalyticsQuery, ResumeAnalyticsDto> GetAnalytics { get; }
    public IQueryHandler<GetResumeStatusLogsByResumeIdQuery, IReadOnlyList<ResumeStatusLogDto>> GetResumeStatusLogs { get; }
    public IQueryHandler<GetResumeStatusLogsByResumeIdQuery, IReadOnlyList<ResumeStatusLogDto>> GetResumeStatusLogsByResumeId => GetResumeStatusLogs;

    public ResumeQueryUnit(
        IQueryHandler<GetAllResumesQuery, PagedResult<ResumeDto>> getAll,
        IQueryHandler<GetResumeByIdQuery, ResumeDto?> getById,
        IQueryHandler<AzureTest.Application.Resumes.Queries.GetResumeAnalytics.GetResumeAnalyticsQuery, ResumeAnalyticsDto> getAnalytics,
        IQueryHandler<GetResumeStatusLogsByResumeIdQuery, IReadOnlyList<ResumeStatusLogDto>> getResumeStatusLogs)
    {
        GetAll = getAll;
        GetById = getById;
        GetAnalytics = getAnalytics;
        GetResumeStatusLogs = getResumeStatusLogs;
    }
}
