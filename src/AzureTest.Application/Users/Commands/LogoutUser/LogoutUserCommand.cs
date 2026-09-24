using System.Threading;
using System.Threading.Tasks;

using AzureTest.Application.Messaging;

namespace AzureTest.Application.Users.Commands.LogoutUser;

public record LogoutUserCommand() : ICommand<bool>;

public class LogoutUserCommandHandler : ICommandHandler<LogoutUserCommand, bool>
{
    public Task<bool> HandleAsync(LogoutUserCommand request, CancellationToken cancellationToken)
    {
        // For JWT, logout is typically handled client-side by discarding the token,
        // or server-side by token invalidation/blacklist. Returning true for now.
        return Task.FromResult(true);
    }
}
