using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.AuditLog;

public static class DeleteAuditLogsByUser
{
    public static async Task<IResult> HandleAsync(
        [FromRoute] Guid userId,
        [FromServices] IAuditLogService auditLogService,
        [FromServices] ICurrentUser currentUser,
        CancellationToken cancellationToken)
    {
        if (!currentUser.IsAdmin)
        {
            return Results.Forbid();
        }

        var deleted = await auditLogService.DeleteLogsByUserAsync(userId, cancellationToken);
        return Results.Ok(new
        {
            success = true,
            deletedCount = deleted,
            message = $"{deleted} audit log record(s) deleted for user."
        });
    }
}
