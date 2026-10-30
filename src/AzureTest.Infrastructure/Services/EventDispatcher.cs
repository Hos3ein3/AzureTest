using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.ExceptionServices;
using AzureTest.Application.Messaging;
using AzureTest.Domain.Primitives;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AzureTest.Infrastructure.Services;

public sealed class EventDispatcher : IEventDispatcher
{
    private sealed record HandlerInfo(Type HandlerType, MethodInfo HandleMethod);

    private static readonly ConcurrentDictionary<Type, HandlerInfo> Cache = new();

    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EventDispatcher> _logger;

    public EventDispatcher(IServiceProvider serviceProvider, ILogger<EventDispatcher> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class
        => PublishObjectAsync(@event, cancellationToken);

    public Task PublishAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
        => PublishObjectAsync(domainEvent, cancellationToken);

    private async Task PublishObjectAsync(object @event, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(@event);

        var eventType = @event.GetType();
        var info = Cache.GetOrAdd(eventType, static t =>
        {
            var handlerType = typeof(IEventHandler<>).MakeGenericType(t);
            return new HandlerInfo(handlerType, handlerType.GetMethod("HandleAsync")!);
        });

        _logger.LogInformation("Publishing event {EventType}", eventType.Name);

        // One scope per handler: separate DbContext, no shared state.
        var handlerCount = 0;
        var errors = new List<Exception>();

        using (var probe = _serviceProvider.CreateScope())
        {
            handlerCount = probe.ServiceProvider.GetServices(info.HandlerType).Count();
        }

        for (var i = 0; i < handlerCount; i++)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var handler = scope.ServiceProvider.GetServices(info.HandlerType).ElementAt(i)!;
                await InvokeAsync(info, handler, @event, ct);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Handler failed for {EventType}", eventType.Name);
                errors.Add(ex);
            }
        }

        if (errors.Count == 1) ExceptionDispatchInfo.Capture(errors[0]).Throw();
        if (errors.Count > 1) throw new AggregateException(errors);
    }

    private static async Task InvokeAsync(HandlerInfo info, object handler, object @event, CancellationToken ct)
    {
        try
        {
            await (Task)info.HandleMethod.Invoke(handler, new[] { @event, ct })!;
        }
        catch (TargetInvocationException ex) when (ex.InnerException is not null)
        {
            ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
        }
    }
}