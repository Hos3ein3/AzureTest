using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Trading;
using AzureTest.Application.Trading.Commands.SyncTradingPrices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AzureTest.Infrastructure.Jobs;

public class TradingPriceSyncJob : ITradingPriceSyncJob
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TradingPriceSyncJob> _logger;

    public TradingPriceSyncJob(IServiceProvider serviceProvider, ILogger<TradingPriceSyncJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        // High-frequency 5-second execution loop (12 cycles per minute)
        for (int cycle = 0; cycle < 12; cycle++)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var tradingCommands = scope.ServiceProvider.GetRequiredService<ITradingCommandUnit>();

                var result = await tradingCommands.SyncPricesAsync(new SyncTradingPricesCommand(), cancellationToken);
                if (result.IsSuccess && result.Value > 0)
                {
                    _logger.LogInformation("TradingPriceSyncJob [5s Cycle {Cycle}/12]: {Count} paper orders/positions automatically triggered or filled (SL/TP/Limits).", cycle + 1, result.Value);
                }

                var outboxJob = scope.ServiceProvider.GetService<IProcessTradingOutboxJob>();
                if (outboxJob != null)
                {
                    await outboxJob.ProcessAsync(cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during Paper Trading Price & SL/TP background synchronization job [Cycle {Cycle}].", cycle + 1);
            }

            if (cycle < 11)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }
    }
}
