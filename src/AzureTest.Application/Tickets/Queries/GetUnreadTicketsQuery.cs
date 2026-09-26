using System;
using System.Collections.Generic;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Tickets;

namespace AzureTest.Application.Tickets.Queries;

public record GetUnreadTicketsQuery(Guid UserId) : IQuery<IReadOnlyList<TicketDto>>;
