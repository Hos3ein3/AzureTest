using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Services;
using Microsoft.AspNetCore.Identity;

namespace AzureTest.Infrastructure.Services;

public class IdentityEmailSender<TUser> : IEmailSender<TUser> where TUser : class
{
    private readonly IEmailSenderService _sender;
    public IdentityEmailSender(IEmailSenderService sender) => _sender = sender;

    public Task SendConfirmationLinkAsync(TUser user, string email, string link) =>
        _sender.SendEmailAsync(email, "Confirm your email",
            $"Please confirm your account by <a href='{link}'>clicking here</a>.");

    public Task SendPasswordResetLinkAsync(TUser user, string email, string link) =>
        _sender.SendEmailAsync(email, "Reset your password",
            $"Reset your password by <a href='{link}'>clicking here</a>.");

    public Task SendPasswordResetCodeAsync(TUser user, string email, string code) =>
        _sender.SendEmailAsync(email, "Reset your password", $"Your reset code: {code}");
}