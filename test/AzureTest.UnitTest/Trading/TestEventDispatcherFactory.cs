using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Messaging;
using AzureTest.Application.Trading.EventHandlers;
using AzureTest.Domain.Entities.Trading.Events;
using AzureTest.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace AzureTest.UnitTest.Trading;

internal static class TestEventDispatcherFactory
{
    public static IEventDispatcher Create(
        IUnitOfWork uow,
        IAppNotificationService? notifications = null)
    {
        var services = new ServiceCollection();

        services.AddLogging();
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton(uow);
        services.AddSingleton(notifications ?? Mock.Of<IAppNotificationService>());

        services.AddScoped<IEventHandler<TradingPositionClosedDomainEvent>, TradingPositionClosedDomainEventHandler>();
        services.AddScoped<IEventHandler<TradingPositionLiquidatedDomainEvent>, TradingPositionLiquidatedDomainEventHandler>();
        services.AddScoped<IEventHandler<TradingOrderFilledDomainEvent>, TradingOrderFilledDomainEventHandler>();
        services.AddScoped<IEventDispatcher, EventDispatcher>();

        return services.BuildServiceProvider().GetRequiredService<IEventDispatcher>();
    }
}