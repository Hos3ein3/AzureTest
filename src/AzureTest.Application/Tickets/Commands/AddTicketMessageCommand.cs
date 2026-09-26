using System;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Messaging;

namespace AzureTest.Application.Tickets.Commands;

public record AddTicketMessageCommand(Guid TicketId, string Content) : ICommand<Result>;
