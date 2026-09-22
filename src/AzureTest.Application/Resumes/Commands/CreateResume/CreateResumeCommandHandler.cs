using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Resumes;
using AzureTest.Domain.Documents;
using AzureTest.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.Resumes.Commands.CreateResume;

// ── Command ───────────────────────────────────────────────────────────────────

public sealed record CreateResumeCommand(CreateResumeDto Dto) : ICommand<Guid>;

// ── Handler ───────────────────────────────────────────────────────────────────

public sealed class CreateResumeCommandHandler : BaseCommandHandler<CreateResumeCommand, Guid>
{
    private readonly IResumeRepository _resumeRepository;
    private readonly ICompanyRepository _companyRepository;

    public CreateResumeCommandHandler(
        IUnitOfWork unitOfWork,
        IResumeRepository resumeRepository,
        ICompanyRepository companyRepository,
        ILogger<CreateResumeCommandHandler> logger)
        : base(unitOfWork, logger)
    {
        _resumeRepository = resumeRepository;
        _companyRepository = companyRepository;
    }

    protected override async Task<Guid> ExecuteAsync(CreateResumeCommand command, CancellationToken cancellationToken)
    {
        var dto = command.Dto;

        // ── Resolve Company ──────────────────────────────────────────────────
        Guid? companyId = dto.CompanyId;

        if (companyId is null && !string.IsNullOrWhiteSpace(dto.NewCompanyName))
        {
            // Check if a company with this name already exists (case-insensitive)
            var existing = await _companyRepository.GetByNameAsync(dto.NewCompanyName.Trim());
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
                await _companyRepository.AddAsync(newCompany);
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
            dto.Source,
            dto.Status);

        resume.CreatedOn = dto.CreatedOn;
        resume.CompanyId = companyId;
        resume.ChatUrl = dto.ChatUrl;

        _resumeRepository.Add(resume);

        return await Task.FromResult(resume.Id);
    }
}
