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
using AzureTest.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.Resumes.Commands.UpdateResumeStatus;

// ── Command ───────────────────────────────────────────────────────────────────

public sealed record UpdateResumeStatusCommand(UpdateResumeStatusDto Dto) : ICommand;

// ── Handler ───────────────────────────────────────────────────────────────────

public sealed class UpdateResumeStatusCommandHandler : BaseCommandHandler<UpdateResumeStatusCommand>
{

    private readonly INoSqlUnitOfWork _noSqlUnitOfWork;
    private readonly ICurrentUser _currentUser;

    public UpdateResumeStatusCommandHandler(
        IUnitOfWork unitOfWork,
        INoSqlUnitOfWork noSqlUnitOfWork,
        ICurrentUser currentUser,
        ILogger<UpdateResumeStatusCommandHandler> logger)
        : base(unitOfWork, logger)
    {
        _noSqlUnitOfWork = noSqlUnitOfWork;
        _currentUser = currentUser;
    }

    protected override async Task ExecuteAsync(UpdateResumeStatusCommand command, CancellationToken cancellationToken)
    {
        var resume = await UnitOfWork.Resumes.GetByIdAsync(command.Dto.Id)
            ?? throw new KeyNotFoundException($"Resume with ID {command.Dto.Id} was not found.");

        if (resume.UserId != _currentUser.Id)
            throw new UnauthorizedAccessException("You are not authorized to update this resume.");

        var oldStatus = resume.Status;

        if (oldStatus != command.Dto.Status || !string.IsNullOrWhiteSpace(command.Dto.Note))
        {
            var resumeStatusLog = new ResumeStatusLog
            {
                Id = Guid.CreateVersion7(),
                ResumeId = resume.Id,
                Date = DateTime.UtcNow,
                OldStatus = oldStatus,
                NewStatus = command.Dto.Status,
                Note = command.Dto.Note
            };

            await _noSqlUnitOfWork.ResumeStatusLogs.AddAsync(resumeStatusLog);
        }

        resume.UpdateStatus(command.Dto.Status);

        UnitOfWork.Resumes.Update(resume);
    }
}
