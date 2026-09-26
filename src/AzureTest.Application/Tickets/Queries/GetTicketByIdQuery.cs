using System;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Tickets;

namespace AzureTest.Application.Tickets.Queries;

public record GetTicketByIdQuery(Guid Id, Guid? UserId = null) : IQuery<TicketDto?>;
