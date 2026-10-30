using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Contracts.DTOs.AuditLogs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.AuditLog;

public static class DeleteAuditLogsByDateRange
{
    public static async Task<IResult> HandleAsync(
        [FromBody] DeleteAuditLogsByDateRangeRequestDto request,
        [FromServices] IAuditLogService auditLogService,
        [FromServices] ICurrentUser currentUser,
        CancellationToken cancellationToken)
    {
        if (!currentUser.IsAdmin)
        {
            return Results.Forbid();
        }

        DateTime? fromUtc = request.FromDate.HasValue
            ? DateTime.SpecifyKind(request.FromDate.Value.Date, DateTimeKind.Utc)
            : null;
        DateTime? toUtc = request.ToDate.HasValue
            ? DateTime.SpecifyKind(request.ToDate.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc)
            : null;

        var deleted = await auditLogService.DeleteLogsByDateRangeAsync(fromUtc, toUtc, cancellationToken);
        return Results.Ok(new
        {
            success = true,
            deletedCount = deleted,
            message = $"{deleted} audit log record(s) deleted within specified timeframe."
        });
    }
}
