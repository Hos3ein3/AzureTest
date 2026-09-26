using System;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Messaging;

namespace AzureTest.Application.Tickets.Commands;

public record MarkTicketMessagesAsReadCommand(Guid TicketId) : ICommand<Result>;
