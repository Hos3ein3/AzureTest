using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Messaging;
using AzureTest.Domain.Entities.Pomodoro;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.Pomodoro.Commands.AbandonPomodoroSession;

public sealed record AbandonPomodoroSessionCommand(Guid UserId, Guid SessionId, int ActualDurationSeconds, string? Notes = null) : ICommand<Result>;

public sealed class AbandonPomodoroSessionCommandValidator : AbstractValidator<AbandonPomodoroSessionCommand>
{
    public AbandonPomodoroSessionCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.SessionId).NotEmpty();
        RuleFor(x => x.ActualDurationSeconds)
            .InclusiveBetween(PomodoroSession.Rules.ActualDurationSecondsMin, PomodoroSession.Rules.ActualDurationSecondsMax);

        RuleFor(x => x.Notes)
            .MaximumLength(PomodoroSession.Rules.NotesMaxLength)
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}

public sealed class AbandonPomodoroSessionCommandHandler : ICommandHandler<AbandonPomodoroSessionCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AbandonPomodoroSessionCommandHandler> _logger;

    public AbandonPomodoroSessionCommandHandler(IUnitOfWork unitOfWork, ILogger<AbandonPomodoroSessionCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> HandleAsync(AbandonPomodoroSessionCommand command, CancellationToken cancellationToken = default)
    {
        var session = await _unitOfWork.Pomodoros.GetByIdAsync(command.SessionId);
        if (session == null || session.UserId != command.UserId)
        {
            return Result.Failure("Pomodoro session was not found.");
        }

        session.Abandon(command.ActualDurationSeconds, command.Notes);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Pomodoro session {SessionId} marked as abandoned", session.Id);
        return Result.Success();
    }
}
