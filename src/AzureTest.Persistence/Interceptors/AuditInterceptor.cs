

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Domain.Primitives;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace AzureTest.Persistence.Interceptors;

/// <summary>
/// EF Core interceptor that automatically sets IAudit properties (CreatedOn, ModifiedOn, etc.)
/// and validates ConcurrencyStamp on save.
/// </summary>
public sealed class AuditInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            ApplyAuditAndValidate(eventData.Context);
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        if (eventData.Context is not null)
        {
            ApplyAuditAndValidate(eventData.Context);
        }

        return base.SavingChanges(eventData, result);
    }

    private static void ApplyAuditAndValidate(DbContext context)
    {
        var now = DateTime.UtcNow;

        foreach (var entry in context.ChangeTracker.Entries<IAudit>())
        {
            if (entry.State == EntityState.Added)
            {
                if (entry.Entity.CreatedOn == default || entry.Entity.CreatedOn.Kind != DateTimeKind.Utc)
                {
                    entry.Entity.CreatedOn = entry.Entity.CreatedOn == default 
                        ? now 
                        : DateTime.SpecifyKind(entry.Entity.CreatedOn, DateTimeKind.Utc);
                }
            }

            if (entry.State is EntityState.Modified or EntityState.Added)
            {
                entry.Entity.ModifiedOn = now;
            }
        }

        foreach (var entry in context.ChangeTracker.Entries<ISoftDelete>())
        {
            if (entry.State == EntityState.Deleted)
            {
                // Convert hard delete → soft delete
                entry.State = EntityState.Modified;
                entry.Entity.IsDeleted = true;
                entry.Entity.DeletedOn = now;
            }
        }

        ValidateConcurrencyStamps(context);
    }

    private static void ValidateConcurrencyStamps(DbContext context)
    {
        // Find entities that expose a ConcurrencyStamp property
        var stampedEntities = context.ChangeTracker
            .Entries()
            .Where(e => e.State is EntityState.Modified or EntityState.Deleted)
            .Where(e => e.Metadata.FindProperty("ConcurrencyStamp") != null)
            .ToList();

        foreach (var entry in stampedEntities)
        {
            var stampProperty = entry.Property("ConcurrencyStamp");

            // Rotate the stamp on every update to enable optimistic concurrency
            stampProperty.CurrentValue = Guid.NewGuid().ToString();
            stampProperty.IsModified = true;
        }
    }
}
