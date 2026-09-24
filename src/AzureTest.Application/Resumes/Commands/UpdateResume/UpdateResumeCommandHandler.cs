using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Resumes;
using AzureTest.Domain.Documents;
using AzureTest.Application.Interfaces.Services;

using Microsoft.Extensions.Logging;

namespace AzureTest.Application.Resumes.Commands.UpdateResume;

// ── Command ───────────────────────────────────────────────────────────────────

public sealed record UpdateResumeCommand(UpdateResumeDto Dto) : ICommand<bool>;

// ── Handler ───────────────────────────────────────────────────────────────────

public sealed class UpdateResumeCommandHandler : BaseCommandHandler<UpdateResumeCommand,bool>
{
    private readonly IResumeRepository _resumeRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly ICurrentUser _currentUser;

    public UpdateResumeCommandHandler(
        IUnitOfWork unitOfWork,
        IResumeRepository resumeRepository,
        ICompanyRepository companyRepository,
        ICurrentUser currentUser,
        ILogger<UpdateResumeCommandHandler> logger)
        : base(unitOfWork, logger)
    {
        _resumeRepository = resumeRepository;
        _companyRepository = companyRepository;
        _currentUser = currentUser;
    }

    protected override async Task<bool> ExecuteAsync(UpdateResumeCommand command, CancellationToken cancellationToken)
    {
        var dto = command.Dto;
        
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
        

        var resume = await _resumeRepository.GetByIdAsync(dto.Id)
            ?? throw new KeyNotFoundException($"Resume with ID {dto.Id} was not found.");

        if (resume.UserId != _currentUser.Id)
            throw new UnauthorizedAccessException("You are not authorized to update this resume.");

        resume.UpdateDetails(dto.PositionTitle, dto.Description, dto.Location, dto.JobLink,dto.CompanyId, dto.Note,dto.ChatUrl);
        resume.CompanyId = companyId;
        resume.ChatUrl = dto.ChatUrl;
        resume.UpdateStatus(dto.Status);
        resume.CreatedOn = dto.CreatedOn;

        _resumeRepository.Update(resume);
        return true;
    }
}
