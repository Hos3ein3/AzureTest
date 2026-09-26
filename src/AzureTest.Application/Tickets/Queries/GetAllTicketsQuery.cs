using System;
using System.Collections.Generic;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Tickets;

namespace AzureTest.Application.Tickets.Queries;

public record GetAllTicketsQuery(Guid? UserId = null) : IQuery<IReadOnlyList<TicketDto>>;
