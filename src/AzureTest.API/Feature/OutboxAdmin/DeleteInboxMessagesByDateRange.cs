using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Contracts.DTOs.Outbox;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.OutboxAdmin;

public static class DeleteInboxMessagesByDateRange
{
    public static async Task<IResult> HandleAsync(
        [FromBody] DeleteDateRangeRequestDto request,
        [FromServices] IUnitOfWork uow,
        CancellationToken cancellationToken)
    {
        DateTime? fromUtc = request.FromDate.HasValue
            ? DateTime.SpecifyKind(request.FromDate.Value.Date, DateTimeKind.Utc)
            : null;
        DateTime? toUtc = request.ToDate.HasValue
            ? DateTime.SpecifyKind(request.ToDate.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc)
            : null;

        var count = await uow.InboxMessages.DeleteByDateRangeAsync(fromUtc, toUtc, cancellationToken);
        return Results.Ok(new
        {
            success = true,
            deletedCount = count,
            message = $"{count} Inbox message(s) deleted within the specified timeframe."
        });
    }
}
