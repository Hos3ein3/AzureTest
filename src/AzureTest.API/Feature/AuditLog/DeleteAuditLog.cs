using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.AuditLog;

public static class DeleteAuditLog
{
    public static async Task<IResult> HandleAsync(
        [FromRoute] Guid id,
        [FromServices] IAuditLogService auditLogService,
        [FromServices] ICurrentUser currentUser,
        CancellationToken cancellationToken)
    {
        if (!currentUser.IsAdmin)
        {
            return Results.Forbid();
        }

        var deleted = await auditLogService.DeleteLogByIdAsync(id, cancellationToken);
        if (deleted > 0)
        {
            return Results.Ok(new { success = true, message = "Audit log record deleted successfully." });
        }

        return Results.NotFound(new { success = false, message = "Audit log record could not be found or deleted." });
    }
}
