using AzureTest.Application.Messaging;
using AzureTest.Application.Resumes.Queries.GetAllResumes;
using AzureTest.Application.Resumes.Queries.GetResumeById;
using AzureTest.Contracts.DTOs.Resumes;
using AzureTest.Contracts.DTOs.Common;
using System.Collections.Generic;

namespace AzureTest.Application.Resumes;

public sealed class ResumeQueryUnit : IResumeQueryUnit
{
    public IQueryHandler<GetAllResumesQuery, PagedResult<ResumeDto>> GetAll { get; }
    public IQueryHandler<GetResumeByIdQuery, ResumeDto?> GetById { get; }
    public IQueryHandler<AzureTest.Application.Resumes.Queries.GetResumeAnalytics.GetResumeAnalyticsQuery, ResumeAnalyticsDto> GetAnalytics { get; }

    public ResumeQueryUnit(
        IQueryHandler<GetAllResumesQuery, PagedResult<ResumeDto>> getAll,
        IQueryHandler<GetResumeByIdQuery, ResumeDto?> getById,
        IQueryHandler<AzureTest.Application.Resumes.Queries.GetResumeAnalytics.GetResumeAnalyticsQuery, ResumeAnalyticsDto> getAnalytics)
    {
        GetAll = getAll;
        GetById = getById;
        GetAnalytics = getAnalytics;
    }
}
