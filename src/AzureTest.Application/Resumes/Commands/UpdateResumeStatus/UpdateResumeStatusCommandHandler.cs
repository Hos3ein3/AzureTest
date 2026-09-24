using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Resumes;
using AzureTest.Domain.Documents;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.Resumes.Commands.UpdateResumeStatus;

// ── Command ───────────────────────────────────────────────────────────────────

public sealed record UpdateResumeStatusCommand(UpdateResumeStatusDto Dto) : ICommand;

// ── Handler ───────────────────────────────────────────────────────────────────

public sealed class UpdateResumeStatusCommandHandler : BaseCommandHandler<UpdateResumeStatusCommand>
{
    private readonly IResumeRepository _resumeRepository;
    private readonly IStatusLogRepository _statusLogRepository;

    public UpdateResumeStatusCommandHandler(
        IUnitOfWork unitOfWork,
        IResumeRepository resumeRepository,
        IStatusLogRepository statusLogRepository,
        ILogger<UpdateResumeStatusCommandHandler> logger)
        : base(unitOfWork, logger)
    {
        _resumeRepository = resumeRepository;
        _statusLogRepository = statusLogRepository;
    }

    protected override async Task ExecuteAsync(UpdateResumeStatusCommand command, CancellationToken cancellationToken)
    {
        var resume = await _resumeRepository.GetByIdAsync(command.Dto.Id)
            ?? throw new KeyNotFoundException($"Resume with ID {command.Dto.Id} was not found.");

        var oldStatus = resume.Status;

        if (oldStatus != command.Dto.Status || !string.IsNullOrWhiteSpace(command.Dto.Note))
        {
            var statusLog = new StatusLog
            {
                Id = Guid.CreateVersion7(),
                ResumeId = resume.Id,
                Date = DateTime.UtcNow,
                OldStatus = oldStatus,
                NewStatus = command.Dto.Status,
                Note = command.Dto.Note
            };

            await _statusLogRepository.AddAsync(statusLog);
        }

        resume.UpdateStatus(command.Dto.Status);

        _resumeRepository.Update(resume);
    }
}
