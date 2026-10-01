using AzureTest.Application.Abstractions;


using System;
using AzureTest.Application.Messaging;
using AzureTest.Domain.Enums;

namespace AzureTest.Application.Tickets.Commands;

public record CreateTicketCommand(string Subject, string Content, TicketPriority Priority) : ICommand<Result<Guid>>;
