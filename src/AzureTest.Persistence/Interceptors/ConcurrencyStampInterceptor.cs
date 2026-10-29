using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Domain.Primitives;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace AzureTest.Persistence.Interceptors;

/// <summary>
/// EF Core interceptor that validates and rotates optimistic concurrency stamps on save
/// for entities implementing IConcurrencyStamp or exposing a ConcurrencyStamp property.
/// </summary>
public sealed class ConcurrencyStampInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            ApplyConcurrencyStamps(eventData.Context);
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        if (eventData.Context is not null)
        {
            ApplyConcurrencyStamps(eventData.Context);
        }

        return base.SavingChanges(eventData, result);
    }

    private static void ApplyConcurrencyStamps(DbContext context)
    {
        // 1. Process entities explicitly implementing IConcurrencyStamp
        foreach (var entry in context.ChangeTracker.Entries<IConcurrencyStamp>())
        {
            if (entry.State == EntityState.Added)
            {
                if (entry.Entity.ConcurrencyStamp == Guid.Empty)
                {
                    entry.Entity.ConcurrencyStamp = Guid.NewGuid();
                }
            }
            else if (entry.State is EntityState.Modified or EntityState.Deleted)
            {
                entry.Entity.ConcurrencyStamp = Guid.NewGuid();
                entry.Property(nameof(IConcurrencyStamp.ConcurrencyStamp)).IsModified = true;
            }
        }

        // 2. Also process any other entities with a ConcurrencyStamp property (e.g. Identity models where type is string)
        var otherStampedEntities = context.ChangeTracker
            .Entries()
            .Where(e => !(e.Entity is IConcurrencyStamp) &&
                        e.State is EntityState.Modified or EntityState.Deleted &&
                        e.Metadata.FindProperty("ConcurrencyStamp") != null)
            .ToList();

        foreach (var entry in otherStampedEntities)
        {
            var stampProperty = entry.Property("ConcurrencyStamp");
            if (stampProperty.Metadata.ClrType == typeof(Guid))
            {
                stampProperty.CurrentValue = Guid.NewGuid();
            }
            else if (stampProperty.Metadata.ClrType == typeof(string))
            {
                stampProperty.CurrentValue = Guid.NewGuid().ToString();
            }
            stampProperty.IsModified = true;
        }
    }
}
