using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Resumes;
using AzureTest.Domain.Entities;
using AzureTest.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.Resumes.Queries.GetResumeById;

// ── Query ─────────────────────────────────────────────────────────────────────

public sealed record GetResumeByIdQuery(Guid Id) : IQuery<ResumeDto?>;

// ── Handler ───────────────────────────────────────────────────────────────────

public sealed class GetResumeByIdQueryHandler : BaseQueryHandler<GetResumeByIdQuery, ResumeDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INoSqlUnitOfWork _noSqlUnitOfWork;
    private readonly ICurrentUser _currentUser;

    public GetResumeByIdQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        INoSqlUnitOfWork noSqlUnitOfWork,
        ILogger<GetResumeByIdQueryHandler> logger)
        : base(logger)
    {
        _unitOfWork = unitOfWork;
        _noSqlUnitOfWork = noSqlUnitOfWork;
        _currentUser = currentUser;
    }

    protected override async Task<ResumeDto?> ExecuteAsync(GetResumeByIdQuery query, CancellationToken cancellationToken)
    {
        var resume = await _unitOfWork.Resumes.GetResumeWithDetailsAsync(query.Id);
        if (resume is null || resume.UserId != _currentUser.Id)
            return null;
        
        var dto = MapToDto(resume);
        if (resume.CompanyId.HasValue)
        {
            var company = await _noSqlUnitOfWork.Companies.GetByIdAsync(resume.CompanyId.Value, cancellationToken);
            if (company != null)
                dto = dto with { CompanyName = company.CompanyName };
        }
        return dto;
    }

    private static ResumeDto MapToDto(Resume resume) => new(
        resume.Id,
        resume.PositionTitle,
        resume.Description,
        resume.Location,
        resume.JobLink,
        resume.Note,
        resume.ChatUrl,
        resume.Source,
        resume.Status,
        resume.CreatedOn,
        resume.ModifiedOn,
        resume.CompanyId,
        null,
        resume.AtsScore);
}
