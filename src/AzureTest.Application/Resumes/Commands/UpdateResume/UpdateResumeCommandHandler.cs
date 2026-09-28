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

    private readonly INoSqlUnitOfWork _noSqlUnitOfWork;
    private readonly ICurrentUser _currentUser;

    public UpdateResumeCommandHandler(
        IUnitOfWork unitOfWork,
        INoSqlUnitOfWork noSqlUnitOfWork,
        ICurrentUser currentUser,
        ILogger<UpdateResumeCommandHandler> logger)
        : base(unitOfWork, logger)
    {
        _noSqlUnitOfWork = noSqlUnitOfWork;
        _currentUser = currentUser;
    }

    protected override async Task<bool> ExecuteAsync(UpdateResumeCommand command, CancellationToken cancellationToken)
    {
        var dto = command.Dto;
        
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
        

        var resume = await UnitOfWork.Resumes.GetByIdAsync(dto.Id)
            ?? throw new KeyNotFoundException($"Resume with ID {dto.Id} was not found.");

        if (resume.UserId != _currentUser.Id)
            throw new UnauthorizedAccessException("You are not authorized to update this resume.");

        resume.UpdateDetails(dto.PositionTitle, dto.Description, dto.Location, dto.JobLink,dto.CompanyId, dto.Note,dto.ChatUrl, dto.AtsScore);
        resume.CompanyId = companyId;
        resume.ChatUrl = dto.ChatUrl;
        resume.UpdateStatus(dto.Status);
        resume.CreatedOn = dto.CreatedOn;

        UnitOfWork.Resumes.Update(resume);
        return true;
    }
}
