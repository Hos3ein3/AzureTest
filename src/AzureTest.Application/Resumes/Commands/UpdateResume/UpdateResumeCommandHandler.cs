using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Resumes;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.Resumes.Commands.UpdateResume;

// ── Command ───────────────────────────────────────────────────────────────────

public sealed record UpdateResumeCommand(UpdateResumeDto Dto) : ICommand;

// ── Handler ───────────────────────────────────────────────────────────────────

public sealed class UpdateResumeCommandHandler : BaseCommandHandler<UpdateResumeCommand>
{
    private readonly IResumeRepository _resumeRepository;

    public UpdateResumeCommandHandler(
        IUnitOfWork unitOfWork,
        IResumeRepository resumeRepository,
        ILogger<UpdateResumeCommandHandler> logger)
        : base(unitOfWork, logger)
    {
        _resumeRepository = resumeRepository;
    }

    protected override async Task ExecuteAsync(UpdateResumeCommand command, CancellationToken cancellationToken)
    {
        var dto = command.Dto;

        var resume = await _resumeRepository.GetByIdAsync(dto.Id)
            ?? throw new KeyNotFoundException($"Resume with ID {dto.Id} was not found.");

        resume.UpdateDetails(dto.PositionTitle, dto.Description, dto.Location, dto.JobLink, dto.Note);
        resume.UpdateStatus(dto.Status);
        resume.CreatedOn = dto.CreatedOn;

        _resumeRepository.Update(resume);
    }
}
