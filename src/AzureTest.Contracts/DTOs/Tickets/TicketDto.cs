

using System;
using System.Collections.Generic;
using AzureTest.Domain.Enums;

namespace AzureTest.Contracts.DTOs.Tickets;

public record TicketDto(
    Guid Id,
    string Subject,
    string Content,
    TicketPriority Priority,
    TicketStatus Status,
    Guid UserId,
    DateTimeOffset CreatedOn,
    List<TicketMessageDto> Messages
);
