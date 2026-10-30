using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.OutboxAdmin;

public static class RetryOutboxMessage
{
    public static async Task<IResult> HandleAsync(
        [FromRoute] Guid id,
        [FromServices] IUnitOfWork uow,
        CancellationToken cancellationToken)
    {
        var message = await uow.OutboxMessages.GetByIdAsync(id, cancellationToken);
        if (message == null)
        {
            return Results.NotFound(new { success = false, message = "Outbox message not found." });
        }

        message.RetryCount = 0;
        message.Error = null;
        message.ProcessedOnUtc = null;
        uow.OutboxMessages.Update(message);
        await uow.SaveChangesAsync(cancellationToken);

        return Results.Ok(new
        {
            success = true,
            message = $"Outbox message {id} reset for background re-dispatch."
        });
    }
}
