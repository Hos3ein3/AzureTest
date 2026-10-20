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

namespace AzureTest.Application.Pomodoro.Commands.CreatePomodoroSession;

public sealed record CreatePomodoroSessionCommand(Guid UserId, CreatePomodoroSessionDto Dto) : ICommand<Result<Guid>>
{
    public static class Mapping
    {
        public static PomodoroSession ToDomain(CreatePomodoroSessionCommand cmd)
        {
            return PomodoroSession.Create(
                userId: cmd.UserId,
                title: cmd.Dto.Title,
                category: cmd.Dto.Category,
                sessionType: cmd.Dto.SessionType,
                plannedDurationSeconds: cmd.Dto.PlannedDurationSeconds,
                actualDurationSeconds: cmd.Dto.ActualDurationSeconds,
                startedAtUtc: cmd.Dto.StartedAtUtc,
                endedAtUtc: cmd.Dto.EndedAtUtc,
                status: cmd.Dto.Status,
                interruptionCount: cmd.Dto.InterruptionCount,
                notes: cmd.Dto.Notes,
                usedSkin: cmd.Dto.UsedSkin);
        }
    }
}

public sealed class CreatePomodoroSessionCommandValidator : AbstractValidator<CreatePomodoroSessionCommand>
{
    public CreatePomodoroSessionCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Dto).NotNull();
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

public sealed class CreatePomodoroSessionCommandHandler : ICommandHandler<CreatePomodoroSessionCommand, Result<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreatePomodoroSessionCommandHandler> _logger;

    public CreatePomodoroSessionCommandHandler(IUnitOfWork unitOfWork, ILogger<CreatePomodoroSessionCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<Guid>> HandleAsync(CreatePomodoroSessionCommand command, CancellationToken cancellationToken = default)
    {
        var session = CreatePomodoroSessionCommand.Mapping.ToDomain(command);

        _unitOfWork.Pomodoros.Add(session);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Pomodoro session {SessionId} created for user {UserId} with status {Status}", session.Id, command.UserId, session.Status);

        return Result<Guid>.Success(session.Id);
    }
}
