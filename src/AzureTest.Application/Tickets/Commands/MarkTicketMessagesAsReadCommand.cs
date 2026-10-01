using AzureTest.Application.Abstractions;

using System;

using AzureTest.Application.Messaging;

namespace AzureTest.Application.Tickets.Commands;

public record MarkTicketMessagesAsReadCommand(Guid TicketId) : ICommand<Result>;
