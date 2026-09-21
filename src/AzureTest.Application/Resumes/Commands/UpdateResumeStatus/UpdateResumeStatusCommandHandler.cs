using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Resumes;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.Resumes.Commands.UpdateResumeStatus;

// ── Command ───────────────────────────────────────────────────────────────────

public sealed record UpdateResumeStatusCommand(UpdateResumeStatusDto Dto) : ICommand;

// ── Handler ───────────────────────────────────────────────────────────────────

public sealed class UpdateResumeStatusCommandHandler : BaseCommandHandler<UpdateResumeStatusCommand>
{
    private readonly IResumeRepository _resumeRepository;

    public UpdateResumeStatusCommandHandler(
        IUnitOfWork unitOfWork,
        IResumeRepository resumeRepository,
        ILogger<UpdateResumeStatusCommandHandler> logger)
        : base(unitOfWork, logger)
    {
        _resumeRepository = resumeRepository;
    }

    protected override async Task ExecuteAsync(UpdateResumeStatusCommand command, CancellationToken cancellationToken)
    {
        var resume = await _resumeRepository.GetByIdAsync(command.Dto.Id)
            ?? throw new KeyNotFoundException($"Resume with ID {command.Dto.Id} was not found.");

        resume.UpdateStatus(command.Dto.Status);

        _resumeRepository.Update(resume);
    }
}
