using AzureTest.Application.Abstractions;


using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Resumes;
using AzureTest.Domain.Entities;
using AzureTest.Domain.Documents;
using AzureTest.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.Resumes.Commands.CreateResume;

// ── Command ───────────────────────────────────────────────────────────────────

public sealed record CreateResumeCommand(CreateResumeDto Dto) : ICommand<Result<Guid>>;

// ── Handler ───────────────────────────────────────────────────────────────────

public sealed class CreateResumeCommandHandler : BaseCommandHandler<CreateResumeCommand, Result<Guid>>
{

    private readonly INoSqlUnitOfWork _noSqlUnitOfWork;
    private readonly ICurrentUser _currentUser;

    public CreateResumeCommandHandler(
        IUnitOfWork unitOfWork,
        INoSqlUnitOfWork noSqlUnitOfWork,
        ICurrentUser currentUser,
        ILogger<CreateResumeCommandHandler> logger)
        : base(unitOfWork, logger)
    {
        _noSqlUnitOfWork = noSqlUnitOfWork;
        _currentUser = currentUser;
    }

    protected override async Task<Result<Guid>> ExecuteAsync(CreateResumeCommand command, CancellationToken cancellationToken)
    {
        var dto = command.Dto;

        // ── Resolve Company ──────────────────────────────────────────────────
        Guid? companyId = dto.CompanyId;

        if (companyId is null && !string.IsNullOrWhiteSpace(dto.NewCompanyName))
        {
            // Check if a company with this name already exists (case-insensitive)
            var existing = await _noSqlUnitOfWork.Companies.GetByNameAsync(dto.NewCompanyName.Trim());
            if (existing is not null)
            {
                companyId = existing.Id;
            }
            else
            {
                // Create a new Company document in MongoDB
                var newCompany = new Company
                {
                    CompanyName = dto.NewCompanyName.Trim()
                };
                await _noSqlUnitOfWork.Companies.AddAsync(newCompany);
                companyId = newCompany.Id;
            }
        }

        // ── Create Resume ────────────────────────────────────────────────────
        var resume = Resume.Create(
            dto.PositionTitle,
            dto.Description,
            dto.Location,
            dto.JobLink,
            dto.Note,
            dto.ChatUrl,
            dto.CompanyId,
            _currentUser.Id,
            dto.Source,
            dto.Status,
            dto.AtsScore);

        resume.CreatedOn = dto.CreatedOn;
        resume.CompanyId = companyId;
        resume.ChatUrl = dto.ChatUrl;

        UnitOfWork.Resumes.Add(resume);

        return await Task.FromResult(resume.Id);
    }
}
