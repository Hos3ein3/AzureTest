using AzureTest.Application.Abstractions;

using System;

using AzureTest.Application.Messaging;
using AzureTest.Application.Tickets.Commands;

namespace AzureTest.Application.Tickets;

public interface ITicketCommandUnit
{
    ICommandHandler<CreateTicketCommand, Result<Guid>> CreateTicket { get; }
    ICommandHandler<AddTicketMessageCommand, Result> AddTicketMessage { get; }
    ICommandHandler<UpdateTicketStatusCommand, Result> UpdateTicketStatus { get; }
    ICommandHandler<MarkTicketMessagesAsReadCommand, Result> MarkTicketMessagesAsRead { get; }
}
