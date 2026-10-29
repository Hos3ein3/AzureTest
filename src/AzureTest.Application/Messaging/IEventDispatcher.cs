using System.Threading;
using System.Threading.Tasks;
using AzureTest.Domain.Primitives;

namespace AzureTest.Application.Messaging;

public interface IEventDispatcher
{
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : class;
    Task PublishAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default);
}
