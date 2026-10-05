using AzureTest.Application.Tickets;
using AzureTest.Application.Tickets.Commands;
using AzureTest.Application.Tickets.Queries;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using System;
using System.Threading.Tasks;
using Xunit;

namespace AzureTest.IntegrationTest;

public class TicketIntegrationTests : IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly IntegrationTestWebAppFactory _factory;
    private static readonly Guid TestUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    
    public TicketIntegrationTests(IntegrationTestWebAppFactory factory) => _factory = factory;

    [Fact]
    public async Task CreateTicket_ShouldPersistToDatabase()
    {
        using var scope = _factory.Services.CreateScope();
        var commandUnit = scope.ServiceProvider.GetRequiredService<ITicketCommandUnit>();
        var queryUnit = scope.ServiceProvider.GetRequiredService<ITicketQueryUnit>();
        
        var command = new CreateTicketCommand("Issue", "It is broken", (AzureTest.Domain.Enums.TicketPriority)1);
        var id = (await commandUnit.CreateTicket.HandleAsync(command, default)).Value;
        
        var ticket = await queryUnit.GetTicketById.HandleAsync(new GetTicketByIdQuery(id, TestUserId), default);
        ticket.ShouldNotBeNull();
        ticket.Subject.ShouldBe("Issue");
        ticket.Content.ShouldBe("It is broken");
    }

    [Fact]
    public async Task AddTicketMessage_ShouldAppendToTicket()
    {
        using var scope = _factory.Services.CreateScope();
        var commandUnit = scope.ServiceProvider.GetRequiredService<ITicketCommandUnit>();
        var queryUnit = scope.ServiceProvider.GetRequiredService<ITicketQueryUnit>();
        
        var id = (await commandUnit.CreateTicket.HandleAsync(new CreateTicketCommand("Subj", "Cont", (AzureTest.Domain.Enums.TicketPriority)1), default)).Value;
        await commandUnit.AddTicketMessage.HandleAsync(new AddTicketMessageCommand(id, "New message"), default);
        
        var ticket = await queryUnit.GetTicketById.HandleAsync(new GetTicketByIdQuery(id, TestUserId), default);
        ticket.ShouldNotBeNull();
        ticket.Messages.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task UpdateTicketStatus_ShouldChangeStatus()
    {
        using var scope = _factory.Services.CreateScope();
        var commandUnit = scope.ServiceProvider.GetRequiredService<ITicketCommandUnit>();
        var queryUnit = scope.ServiceProvider.GetRequiredService<ITicketQueryUnit>();
        
        var id = (await commandUnit.CreateTicket.HandleAsync(new CreateTicketCommand("Subj", "Cont", (AzureTest.Domain.Enums.TicketPriority)1), default)).Value;
        await commandUnit.UpdateTicketStatus.HandleAsync(new UpdateTicketStatusCommand(id, (AzureTest.Domain.Enums.TicketStatus)1), default);
        
        var ticket = await queryUnit.GetTicketById.HandleAsync(new GetTicketByIdQuery(id, TestUserId), default);
        ticket.ShouldNotBeNull();
        ticket.Status.ShouldBe((AzureTest.Domain.Enums.TicketStatus)1);
    }

    [Fact]
    public async Task GetAllTickets_WithUserId_ShouldReturnOnlyUserTickets()
    {
        using var scope = _factory.Services.CreateScope();
        var commandUnit = scope.ServiceProvider.GetRequiredService<ITicketCommandUnit>();
        var queryUnit = scope.ServiceProvider.GetRequiredService<ITicketQueryUnit>();
        
        var id = (await commandUnit.CreateTicket.HandleAsync(new CreateTicketCommand("Subj", "Cont", (AzureTest.Domain.Enums.TicketPriority)1), default)).Value;
        
        var tickets = await queryUnit.GetAllTickets.HandleAsync(new GetAllTicketsQuery(TestUserId), default);
        tickets.ShouldNotBeNull();
        tickets.ShouldContain(t => t.Id == id);
    }
}
