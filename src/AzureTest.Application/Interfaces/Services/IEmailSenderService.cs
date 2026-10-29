using AzureTest.Application.Abstractions;

namespace AzureTest.Application.Interfaces.Services;

public interface IEmailSenderService
{
    Task<Result> SendEmailAsync(string email, string subject, string htmlMessage);
}