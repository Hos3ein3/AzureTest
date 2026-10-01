using AzureTest.Application.Abstractions;


using FluentValidation;
using AzureTest.Application.Messaging;
using System.Threading.Tasks;
using System.Threading;

namespace AzureTest.Application.Users.Commands.LogoutUser;

public record LogoutUserCommand() : ICommand<Result<bool>>;
public class LogoutUserCommandHandler : ICommandHandler<LogoutUserCommand, Result<bool>>
{
    public Task<Result<bool>> HandleAsync(LogoutUserCommand request, CancellationToken cancellationToken)
    {
        // For JWT, logout is typically handled client-side by discarding the token,
        // or server-side by token invalidation/blacklist. Returning true for now.
        return Task.FromResult(Result.Success(true));
    }
}

public sealed class LogoutUserCommandValidator : AbstractValidator<LogoutUserCommand>
{
    public LogoutUserCommandValidator()
    {

    }
}
