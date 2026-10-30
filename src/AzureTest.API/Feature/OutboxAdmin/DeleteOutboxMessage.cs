using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Contracts.DTOs.Outbox;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.OutboxAdmin;

public static class DeleteOutboxMessage
{
    public static async Task<IResult> HandleAsync(
        [FromBody] DeleteOutboxMessageRequestDto request,
        [FromServices] IUnitOfWork uow,
        CancellationToken cancellationToken)
    {
        var deleted = await uow.OutboxMessages.DeleteAsync(request.Id, cancellationToken);
        if (deleted)
        {
            return Results.Ok(new { success = true, message = "Outbox message deleted successfully." });
        }

        return Results.NotFound(new { success = false, message = "Outbox message could not be found or deleted." });
    }
}
