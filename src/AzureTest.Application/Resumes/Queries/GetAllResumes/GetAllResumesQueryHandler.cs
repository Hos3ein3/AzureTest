using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Resumes;
using AzureTest.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.Resumes.Queries.GetAllResumes;

// ── Query ─────────────────────────────────────────────────────────────────────

public sealed record GetAllResumesQuery : IQuery<IReadOnlyList<ResumeDto>>;

// ── Handler ───────────────────────────────────────────────────────────────────

public sealed class GetAllResumesQueryHandler : BaseQueryHandler<GetAllResumesQuery, IReadOnlyList<ResumeDto>>
{
    private readonly IResumeRepository _resumeRepository;

    public GetAllResumesQueryHandler(
        IResumeRepository resumeRepository,
        ILogger<GetAllResumesQueryHandler> logger)
        : base(logger)
    {
        _resumeRepository = resumeRepository;
    }

    protected override async Task<IReadOnlyList<ResumeDto>> ExecuteAsync(GetAllResumesQuery query, CancellationToken cancellationToken)
    {
        var resumes = await _resumeRepository.FindAsync(r => !r.IsDeleted);
        return resumes.Select(MapToDto).ToList().AsReadOnly();
    }

    private static ResumeDto MapToDto(Resume resume) => new(
        resume.Id,
        resume.PositionTitle,
        resume.Description,
        resume.Location,
        resume.JobLink,
        resume.Note,
        resume.Source,
        resume.Status,
        resume.CreatedOn,
        resume.ModifiedOn);
}
