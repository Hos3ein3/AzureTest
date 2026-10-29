using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Messaging;
using AzureTest.Domain.Primitives;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AzureTest.Infrastructure.Jobs;

public interface IProcessTradingOutboxJob
{
    Task ProcessAsync(CancellationToken cancellationToken = default);
}

public sealed class ProcessTradingOutboxJob : IProcessTradingOutboxJob
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ProcessTradingOutboxJob> _logger;

    public ProcessTradingOutboxJob(IServiceProvider serviceProvider, ILogger<ProcessTradingOutboxJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task ProcessAsync(CancellationToken cancellationToken = default)
    {
        using var scope = _serviceProvider.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var dispatcher = scope.ServiceProvider.GetRequiredService<IEventDispatcher>();

        var messages = await uow.OutboxMessages.GetUnprocessedMessagesAsync(batchSize: 50, cancellationToken);
        if (messages.Count == 0) return;

        _logger.LogInformation("ProcessTradingOutboxJob: Processing {Count} outbox messages.", messages.Count);

        foreach (var message in messages)
        {
            if (cancellationToken.IsCancellationRequested) break;

            try
            {
                var eventType = Type.GetType(message.EventType);
                if (eventType == null)
                {
                    // Fallback: search loaded domain assemblies by simple type name
                    foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        eventType = asm.GetType(message.EventType);
                        if (eventType != null) break;
                    }
                }

                if (eventType == null)
                {
                    _logger.LogError("ProcessTradingOutboxJob: Could not resolve event type {EventType} for OutboxMessage {Id}.", message.EventType, message.Id);
                    message.Error = $"Could not resolve Type: {message.EventType}";
                    message.RetryCount++;
                    uow.OutboxMessages.Update(message);
                    continue;
                }

                var domainEvent = JsonSerializer.Deserialize(message.Payload, eventType);
                if (domainEvent is IDomainEvent validEvent)
                {
                    await dispatcher.PublishAsync(validEvent, cancellationToken);
                }
                else if (domainEvent != null)
                {
                    await dispatcher.PublishAsync((dynamic)domainEvent, cancellationToken);
                }

                message.ProcessedOnUtc = DateTime.UtcNow;
                message.Error = null;
                uow.OutboxMessages.Update(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ProcessTradingOutboxJob: Failed processing OutboxMessage {Id}. Retry: {RetryCount}", message.Id, message.RetryCount);
                message.Error = ex.Message;
                message.RetryCount++;
                uow.OutboxMessages.Update(message);
            }
        }

        await uow.SaveChangesAsync(cancellationToken);
    }
}
