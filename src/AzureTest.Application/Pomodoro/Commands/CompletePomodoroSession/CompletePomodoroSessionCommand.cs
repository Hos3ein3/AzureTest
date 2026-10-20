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

namespace AzureTest.Application.Pomodoro.Commands.CompletePomodoroSession;

public sealed record CompletePomodoroSessionCommand(Guid UserId, CompletePomodoroSessionDto Dto) : ICommand<Result>;

public sealed class CompletePomodoroSessionCommandValidator : AbstractValidator<CompletePomodoroSessionCommand>
{
    public CompletePomodoroSessionCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Dto).NotNull();
        RuleFor(x => x.Dto.Id).NotEmpty();
        RuleFor(x => x.Dto.ActualDurationSeconds)
            .InclusiveBetween(PomodoroSession.Rules.ActualDurationSecondsMin, PomodoroSession.Rules.ActualDurationSecondsMax);

        RuleFor(x => x.Dto.Notes)
            .MaximumLength(PomodoroSession.Rules.NotesMaxLength)
            .When(x => !string.IsNullOrEmpty(x.Dto.Notes));
    }
}

public sealed class CompletePomodoroSessionCommandHandler : ICommandHandler<CompletePomodoroSessionCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CompletePomodoroSessionCommandHandler> _logger;

    public CompletePomodoroSessionCommandHandler(IUnitOfWork unitOfWork, ILogger<CompletePomodoroSessionCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> HandleAsync(CompletePomodoroSessionCommand command, CancellationToken cancellationToken = default)
    {
        var session = await _unitOfWork.Pomodoros.GetByIdAsync(command.Dto.Id);
        if (session == null || session.UserId != command.UserId)
        {
            return Result.Failure("Pomodoro session was not found.");
        }

        session.Complete(command.Dto.ActualDurationSeconds, command.Dto.Notes);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Pomodoro session {SessionId} completed successfully", session.Id);
        return Result.Success();
    }
}
