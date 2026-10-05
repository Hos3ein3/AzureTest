using AzureTest.Application.Events;
using AzureTest.Application.Events.Commands;
using AzureTest.Application.Events.Queries;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using System;
using System.Threading.Tasks;
using Xunit;

namespace AzureTest.IntegrationTest;

public class EventIntegrationTests : IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly IntegrationTestWebAppFactory _factory;
    private static readonly Guid TestUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    
    public EventIntegrationTests(IntegrationTestWebAppFactory factory) => _factory = factory;

    [Fact]
    public async Task CreateEvent_ShouldPersistToDatabase()
    {
        using var scope = _factory.Services.CreateScope();
        var commandUnit = scope.ServiceProvider.GetRequiredService<IEventCommandUnit>();
        var queryUnit = scope.ServiceProvider.GetRequiredService<IEventQueryUnit>();
        
        var command = new CreateEventCommand("Party", "Fun", DateTime.UtcNow, DateTime.UtcNow.AddHours(2), false, "Red");
        var id = (await commandUnit.CreateEvent.HandleAsync(command, default)).Value;
        
        var ev = await queryUnit.GetEventById.HandleAsync(new GetEventByIdQuery(id, TestUserId), default);
        ev.ShouldNotBeNull();
        ev.Title.ShouldBe("Party");
    }

    [Fact]
    public async Task UpdateEvent_ShouldModifyFields()
    {
        using var scope = _factory.Services.CreateScope();
        var commandUnit = scope.ServiceProvider.GetRequiredService<IEventCommandUnit>();
        var queryUnit = scope.ServiceProvider.GetRequiredService<IEventQueryUnit>();
        
        var id = (await commandUnit.CreateEvent.HandleAsync(new CreateEventCommand("Party", "Fun", DateTime.UtcNow, DateTime.UtcNow.AddHours(2), false, "Red"), default)).Value;
        await commandUnit.UpdateEvent.HandleAsync(new UpdateEventCommand(id, "Bigger Party", "More Fun", DateTime.UtcNow, DateTime.UtcNow.AddHours(3), false, "Blue"), default);
        
        var ev = await queryUnit.GetEventById.HandleAsync(new GetEventByIdQuery(id, TestUserId), default);
        ev.ShouldNotBeNull();
        ev.Title.ShouldBe("Bigger Party");
        ev.Color.ShouldBe("Blue");
    }

    [Fact]
    public async Task DeleteEvent_ShouldRemoveFromDatabase()
    {
        using var scope = _factory.Services.CreateScope();
        var commandUnit = scope.ServiceProvider.GetRequiredService<IEventCommandUnit>();
        var queryUnit = scope.ServiceProvider.GetRequiredService<IEventQueryUnit>();
        
        var id = (await commandUnit.CreateEvent.HandleAsync(new CreateEventCommand("Party", "Fun", DateTime.UtcNow, DateTime.UtcNow.AddHours(2), false, "Red"), default)).Value;
        await commandUnit.DeleteEvent.HandleAsync(new DeleteEventCommand(id), default);
        
        var ev = await queryUnit.GetEventById.HandleAsync(new GetEventByIdQuery(id, TestUserId), default);
        ev.ShouldBeNull();
    }
}
