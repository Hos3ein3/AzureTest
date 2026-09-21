using AzureTest.Application.Messaging;
using AzureTest.Application.Resumes.Queries.GetAllResumes;
using AzureTest.Application.Resumes.Queries.GetResumeById;
using AzureTest.Contracts.DTOs.Resumes;

namespace AzureTest.Application.Resumes;

public sealed class ResumeQueryUnit : IResumeQueryUnit
{
    public IQueryHandler<GetAllResumesQuery, IReadOnlyList<ResumeDto>> GetAll { get; }
    public IQueryHandler<GetResumeByIdQuery, ResumeDto?> GetById { get; }

    public ResumeQueryUnit(
        IQueryHandler<GetAllResumesQuery, IReadOnlyList<ResumeDto>> getAll,
        IQueryHandler<GetResumeByIdQuery, ResumeDto?> getById)
    {
        GetAll = getAll;
        GetById = getById;
    }
}
