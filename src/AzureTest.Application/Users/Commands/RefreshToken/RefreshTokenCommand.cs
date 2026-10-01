using AzureTest.Application.Abstractions;


using FluentValidation;
using AzureTest.Contracts.DTOs.Users;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Messaging;
using System.Threading.Tasks;
using System.Threading;

namespace AzureTest.Application.Users.Commands.RefreshToken;

public record RefreshTokenCommand(string Token, string RefreshToken) : ICommand<Result<RefreshTokenResult>>;
public class RefreshTokenCommandHandler : ICommandHandler<RefreshTokenCommand, Result<RefreshTokenResult>>
{
    private readonly IUserManagerService _userManagerService;
    public RefreshTokenCommandHandler(IUserManagerService userManagerService)
    {
        _userManagerService = userManagerService;
    }
    public async Task<Result<RefreshTokenResult>> HandleAsync(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var resultDto = await _userManagerService.RefreshTokenAsync(request.Token, request.RefreshToken);
        return new RefreshTokenResult(resultDto.Success, resultDto.Errors, resultDto.Token, resultDto.RefreshToken);
    }
}

public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(v => v.Token).NotEmpty();
        RuleFor(v => v.RefreshToken).NotEmpty();
    }
}
