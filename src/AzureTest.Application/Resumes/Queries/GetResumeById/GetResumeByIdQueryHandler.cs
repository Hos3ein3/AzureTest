using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Resumes;
using AzureTest.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.Resumes.Queries.GetResumeById;

// ── Query ─────────────────────────────────────────────────────────────────────

public sealed record GetResumeByIdQuery(Guid Id) : IQuery<ResumeDto?>;

// ── Handler ───────────────────────────────────────────────────────────────────

public sealed class GetResumeByIdQueryHandler : BaseQueryHandler<GetResumeByIdQuery, ResumeDto?>
{
    private readonly IResumeRepository _resumeRepository;

    public GetResumeByIdQueryHandler(
        IResumeRepository resumeRepository,
        ILogger<GetResumeByIdQueryHandler> logger)
        : base(logger)
    {
        _resumeRepository = resumeRepository;
    }

    protected override async Task<ResumeDto?> ExecuteAsync(GetResumeByIdQuery query, CancellationToken cancellationToken)
    {
        var resume = await _resumeRepository.GetResumeWithDetailsAsync(query.Id);
        return resume is null ? null : MapToDto(resume);
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
