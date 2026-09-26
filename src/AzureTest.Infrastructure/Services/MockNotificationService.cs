using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace AzureTest.Infrastructure.Services;

public class MockNotificationService : INotificationService
{
    private readonly ILogger<MockNotificationService> _logger;

    public MockNotificationService(ILogger<MockNotificationService> logger)
    {
        _logger = logger;
    }

    // public Task SendNotificationAsync(string to, string subject, string body)
    // {
    //     _logger.LogInformation("Sending mock notification to {To} with subject {Subject}", to, subject);
    //     return Task.CompletedTask;
    // }
}
