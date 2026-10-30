using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.AuditLog;

public static class PurgeAuditLogs
{
    public static async Task<IResult> HandleAsync(
        [FromServices] IAuditLogService auditLogService,
        [FromServices] ICurrentUser currentUser,
        CancellationToken cancellationToken)
    {
        if (!currentUser.IsAdmin)
        {
            return Results.Forbid();
        }

        var deleted = await auditLogService.PurgeAllLogsAsync(cancellationToken);
        return Results.Ok(new
        {
            success = true,
            deletedCount = deleted,
            message = $"All audit log records purged successfully ({deleted} total)."
        });
    }
}
