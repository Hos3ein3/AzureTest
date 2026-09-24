using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Messaging;

namespace AzureTest.Application.Users.Commands.RefreshToken;

public record RefreshTokenResult(bool Success, string[]? Errors, string? Token, string? RefreshToken);

public record RefreshTokenCommand(string Token, string RefreshToken) : ICommand<RefreshTokenResult>;

public class RefreshTokenCommandHandler : ICommandHandler<RefreshTokenCommand, RefreshTokenResult>
{
    private readonly IUserManagerService _userManagerService;

    public RefreshTokenCommandHandler(IUserManagerService userManagerService)
    {
        _userManagerService = userManagerService;
    }

    public async Task<RefreshTokenResult> HandleAsync(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var resultDto = await _userManagerService.RefreshTokenAsync(request.Token, request.RefreshToken);
        return new RefreshTokenResult(resultDto.Success, resultDto.Errors, resultDto.Token, resultDto.RefreshToken);
    }
}
