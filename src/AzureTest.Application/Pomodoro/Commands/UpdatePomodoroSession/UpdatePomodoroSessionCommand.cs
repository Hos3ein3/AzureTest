using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Pomodoro;
using AzureTest.Domain.Entities.Pomodoro;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.Pomodoro.Commands.UpdatePomodoroSession;

public sealed record UpdatePomodoroSessionCommand(Guid UserId, UpdatePomodoroSessionDto Dto) : ICommand<Result>;

public sealed class UpdatePomodoroSessionCommandValidator : AbstractValidator<UpdatePomodoroSessionCommand>
{
    public UpdatePomodoroSessionCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Dto).NotNull();
        RuleFor(x => x.Dto.Id).NotEmpty();
        RuleFor(x => x.Dto.Title)
            .NotEmpty()
            .MaximumLength(PomodoroSession.Rules.TitleMaxLength);

        RuleFor(x => x.Dto.Category)
            .MaximumLength(PomodoroSession.Rules.CategoryMaxLength)
            .When(x => !string.IsNullOrEmpty(x.Dto.Category));

        RuleFor(x => x.Dto.Notes)
            .MaximumLength(PomodoroSession.Rules.NotesMaxLength)
            .When(x => !string.IsNullOrEmpty(x.Dto.Notes));

        RuleFor(x => x.Dto.PlannedDurationSeconds)
            .InclusiveBetween(PomodoroSession.Rules.PlannedDurationSecondsMin, PomodoroSession.Rules.PlannedDurationSecondsMax);

        RuleFor(x => x.Dto.ActualDurationSeconds)
            .InclusiveBetween(PomodoroSession.Rules.ActualDurationSecondsMin, PomodoroSession.Rules.ActualDurationSecondsMax);
    }
}

public sealed class UpdatePomodoroSessionCommandHandler : ICommandHandler<UpdatePomodoroSessionCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdatePomodoroSessionCommandHandler> _logger;

    public UpdatePomodoroSessionCommandHandler(IUnitOfWork unitOfWork, ILogger<UpdatePomodoroSessionCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> HandleAsync(UpdatePomodoroSessionCommand command, CancellationToken cancellationToken = default)
    {
        var session = await _unitOfWork.Pomodoros.GetByIdAsync(command.Dto.Id);
        if (session == null || session.UserId != command.UserId)
        {
            return Result.Failure("Pomodoro session was not found.");
        }

        session.Update(
            title: command.Dto.Title,
            category: command.Dto.Category,
            taskItemId: command.Dto.TaskItemId,
            sessionType: command.Dto.SessionType,
            plannedDurationSeconds: command.Dto.PlannedDurationSeconds,
            actualDurationSeconds: command.Dto.ActualDurationSeconds,
            startedAtUtc: command.Dto.StartedAtUtc,
            endedAtUtc: command.Dto.EndedAtUtc,
            status: command.Dto.Status,
            interruptionCount: command.Dto.InterruptionCount,
            notes: command.Dto.Notes,
            usedSkin: command.Dto.UsedSkin);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Pomodoro session {SessionId} updated successfully", session.Id);

        return Result.Success();
    }
}
