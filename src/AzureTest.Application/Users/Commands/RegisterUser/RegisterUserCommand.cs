using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Messaging;

namespace AzureTest.Application.Users.Commands.RegisterUser;

public record RegisterUserCommand(string Email, string Password, string FirstName, string LastName) : ICommand<(bool Success, string[] Errors)>;

public class RegisterUserCommandHandler : ICommandHandler<RegisterUserCommand, (bool Success, string[] Errors)>
{
    private readonly IUserManagerService _userManagerService;

    public RegisterUserCommandHandler(IUserManagerService userManagerService)
    {
        _userManagerService = userManagerService;
    }

    public async Task<(bool Success, string[] Errors)> HandleAsync(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        return await _userManagerService.RegisterUserAsync(request.Email, request.Password, request.FirstName, request.LastName);
    }
}
