using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Messaging;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.Pomodoro.Commands.DeletePomodoroSession;

public sealed record DeletePomodoroSessionCommand(Guid UserId, Guid SessionId) : ICommand<Result>;

public sealed class DeletePomodoroSessionCommandValidator : AbstractValidator<DeletePomodoroSessionCommand>
{
    public DeletePomodoroSessionCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.SessionId).NotEmpty();
    }
}

public sealed class DeletePomodoroSessionCommandHandler : ICommandHandler<DeletePomodoroSessionCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeletePomodoroSessionCommandHandler> _logger;

    public DeletePomodoroSessionCommandHandler(IUnitOfWork unitOfWork, ILogger<DeletePomodoroSessionCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> HandleAsync(DeletePomodoroSessionCommand command, CancellationToken cancellationToken = default)
    {
        var session = await _unitOfWork.Pomodoros.GetByIdAsync(command.SessionId);
        if (session == null || session.UserId != command.UserId)
        {
            return Result.Failure("Pomodoro session was not found.");
        }

        session.SoftDelete(command.UserId.ToString());
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Pomodoro session {SessionId} soft-deleted", session.Id);
        return Result.Success();
    }
}
