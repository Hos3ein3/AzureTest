using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Auth.Services;
using AzureTest.Application.Messaging;
using FluentValidation;

namespace AzureTest.Application.Auth.Commands.Logout;

public sealed record LogoutCommand(Guid UserId) : ICommand<Result>;

public sealed class LogoutCommandValidator : AbstractValidator<LogoutCommand>
{
    public LogoutCommandValidator()
    {
        // UserId may be Guid.Empty if unauthenticated
    }
}

public sealed class LogoutCommandHandler : ICommandHandler<LogoutCommand, Result>
{
    private readonly IAuthSessionService _sessionService;

    public LogoutCommandHandler(IAuthSessionService sessionService)
    {
        _sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
    }

    public async Task<Result> HandleAsync(LogoutCommand request, CancellationToken cancellationToken = default)
    {
        if (request.UserId != Guid.Empty)
        {
            await _sessionService.InvalidateUserSessionAsync(request.UserId, cancellationToken);
        }

        return Result.Success();
    }
}
