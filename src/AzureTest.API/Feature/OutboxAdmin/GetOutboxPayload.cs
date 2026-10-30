using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Contracts.DTOs.Outbox;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.OutboxAdmin;

public static class GetOutboxPayload
{
    public static async Task<IResult> HandleAsync(
        [FromRoute] Guid id,
        [FromServices] IUnitOfWork uow,
        CancellationToken cancellationToken)
    {
        var message = await uow.OutboxMessages.GetByIdAsync(id, cancellationToken);
        if (message == null)
        {
            return Results.NotFound(new { error = "Outbox message not found." });
        }

        var response = new OutboxPayloadResponseDto(
            message.Id,
            message.EventType,
            message.OccurredOnUtc,
            message.ProcessedOnUtc,
            message.RetryCount,
            message.Error,
            message.Payload
        );

        return Results.Ok(response);
    }
}
