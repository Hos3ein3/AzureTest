using System.Threading;
using System.Threading.Tasks;

namespace AzureTest.Application.Messaging;

/// <summary>
/// Marker interface for all event handlers.
/// </summary>
public interface IEventHandler
{
}

/// <summary>
/// Defines a handler for a specific event of type <typeparamref name="TEvent"/>.
/// </summary>
public interface IEventHandler<in TEvent> : IEventHandler where TEvent : class
{
    Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default);
}

/// <summary>
/// Abstract base class for domain and application event handlers.
/// </summary>
public abstract class EventHandler<TEvent> : IEventHandler<TEvent> where TEvent : class
{
    public abstract Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default);
}
