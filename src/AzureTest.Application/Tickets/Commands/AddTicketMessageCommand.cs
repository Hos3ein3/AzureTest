using AzureTest.Application.Abstractions;

using System;

using AzureTest.Application.Messaging;

namespace AzureTest.Application.Tickets.Commands;

public record AddTicketMessageCommand(Guid TicketId, string Content) : ICommand<Result>;
