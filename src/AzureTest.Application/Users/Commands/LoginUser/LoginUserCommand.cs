using AzureTest.Application.Messaging;

namespace AzureTest.Application.Users.Commands.LoginUser;

public record LoginUserCommand(string Email, string Password) : ICommand<LoginUserResult>;
