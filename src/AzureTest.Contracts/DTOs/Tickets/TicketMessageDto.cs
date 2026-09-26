using System;

namespace AzureTest.Contracts.DTOs.Tickets;

public record TicketMessageDto(
    Guid Id,
    Guid UserId,
    string Content,
    bool IsReadByCreator,
    DateTimeOffset CreatedOn
);
