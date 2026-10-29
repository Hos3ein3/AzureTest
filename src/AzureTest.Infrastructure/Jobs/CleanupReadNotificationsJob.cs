using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace AzureTest.Infrastructure.Jobs;

public class CleanupReadNotificationsJob : ICleanupReadNotificationsJob
{
    private readonly INoSqlUnitOfWork _noSqlUow;
    private readonly ILogger<CleanupReadNotificationsJob> _logger;

    public CleanupReadNotificationsJob(
        INoSqlUnitOfWork noSqlUow,
        ILogger<CleanupReadNotificationsJob> logger)
    {
        _noSqlUow = noSqlUow ?? throw new ArgumentNullException(nameof(noSqlUow));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var cutoffUtc = DateTime.UtcNow.AddDays(-10);
            var deletedCount = await _noSqlUow.NotificationHistories.DeleteReadOlderThanAsync(cutoffUtc, cancellationToken);

            if (deletedCount > 0)
            {
                _logger.LogInformation("CleanupReadNotificationsJob: Successfully purged {Count} read notification(s) older than 10 days (cutoff: {Cutoff:u}).",
                    deletedCount, cutoffUtc);
            }
            else
            {
                _logger.LogDebug("CleanupReadNotificationsJob: No read notifications older than 10 days to purge.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during CleanupReadNotificationsJob execution.");
        }
    }
}
