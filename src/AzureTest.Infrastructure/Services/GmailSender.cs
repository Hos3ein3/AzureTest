using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Infrastructure.Configuration;

using MailKit.Net.Smtp;
using MailKit.Security;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using MimeKit;


namespace AzureTest.Infrastructure.Services;

public class GmailSender :IEmailSenderService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<GmailSender> _logger;
    public GmailSender(IOptions<EmailSettings> options, ILogger<GmailSender> logger)
    {
        _logger = logger;
        _settings = options.Value;
    }

    public async Task<Result> SendEmailAsync(string email, string subject, string htmlMessage)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_settings.FromName, _settings.Username));
            message.To.Add(MailboxAddress.Parse(email));
            message.Subject = subject;
            message.Body = new BodyBuilder { HtmlBody = htmlMessage }.ToMessageBody();
        
            using var client = new SmtpClient();
            await client.ConnectAsync(_settings.SmtpServer, _settings.Port, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_settings.Username, _settings.Password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
            return Result.Success();
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
           return Result.Failure(e.Message);
        }
        
    }
}