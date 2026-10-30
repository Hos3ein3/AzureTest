using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Contracts.DTOs.Outbox;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.OutboxAdmin;

public static class DeleteInboxMessage
{
    public static async Task<IResult> HandleAsync(
        [FromBody] DeleteInboxMessageRequestDto request,
        [FromServices] IUnitOfWork uow,
        CancellationToken cancellationToken)
    {
        var deleted = await uow.InboxMessages.DeleteAsync(request.Id, cancellationToken);
        if (deleted)
        {
            return Results.Ok(new { success = true, message = "Inbox message deleted successfully." });
        }

        return Results.NotFound(new { success = false, message = "Inbox message could not be found or deleted." });
    }
}
