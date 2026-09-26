using System;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Messaging;
using AzureTest.Domain.Enums;

namespace AzureTest.Application.Tickets.Commands;

public record UpdateTicketStatusCommand(Guid TicketId, TicketStatus Status) : ICommand<Result>;
