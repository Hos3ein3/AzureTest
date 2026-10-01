using AzureTest.Application.Abstractions;

using System;

using AzureTest.Application.Messaging;
using AzureTest.Application.Tickets.Commands;

namespace AzureTest.Application.Tickets;

public class TicketCommandUnit : ITicketCommandUnit
{
    public ICommandHandler<CreateTicketCommand, Result<Guid>> CreateTicket { get; }
    public ICommandHandler<AddTicketMessageCommand, Result> AddTicketMessage { get; }
    public ICommandHandler<UpdateTicketStatusCommand, Result> UpdateTicketStatus { get; }
    public ICommandHandler<MarkTicketMessagesAsReadCommand, Result> MarkTicketMessagesAsRead { get; }

    public TicketCommandUnit(
        ICommandHandler<CreateTicketCommand, Result<Guid>> createTicket,
        ICommandHandler<AddTicketMessageCommand, Result> addTicketMessage,
        ICommandHandler<UpdateTicketStatusCommand, Result> updateTicketStatus,
        ICommandHandler<MarkTicketMessagesAsReadCommand, Result> markTicketMessagesAsRead)
    {
        CreateTicket = createTicket;
        AddTicketMessage = addTicketMessage;
        UpdateTicketStatus = updateTicketStatus;
        MarkTicketMessagesAsRead = markTicketMessagesAsRead;
    }
}
