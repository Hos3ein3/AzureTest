using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Domain.Primitives;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace AzureTest.Persistence.Interceptors;

/// <summary>
/// EF Core interceptor that automatically stamps TenantId on newly added ITenantEntity entries
/// using the ITenantProvider. Completely bypassed when multi-tenancy is disabled in configuration.
/// </summary>
public sealed class TenantSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly ITenantProvider _tenantProvider;

    public TenantSaveChangesInterceptor(ITenantProvider tenantProvider)
    {
        _tenantProvider = tenantProvider;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            ApplyTenant(eventData.Context);
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        if (eventData.Context is not null)
        {
            ApplyTenant(eventData.Context);
        }

        return base.SavingChanges(eventData, result);
    }

    private void ApplyTenant(DbContext context)
    {
        if (!_tenantProvider.IsMultiTenancyEnabled)
            return;

        var tenantId = _tenantProvider.CurrentTenantId;
        if (tenantId == Guid.Empty)
            return;

        foreach (var entry in context.ChangeTracker.Entries<ITenantEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                if (entry.Entity.TenantId == Guid.Empty)
                {
                    entry.Entity.TenantId = tenantId;
                }

                // If Admin is impersonating another tenant:
                // Align UserId with the active tenant workspace so the tenant owner can see and own the data!
                if (_tenantProvider.IsAdminImpersonating)
                {
                    var userIdProp = entry.Metadata.FindProperty("UserId");
                    if (userIdProp != null && userIdProp.ClrType == typeof(Guid))
                    {
                        var currentUserId = (Guid?)entry.Property("UserId").CurrentValue;
                        if (currentUserId == null || currentUserId == Guid.Empty || currentUserId == _tenantProvider.RealUserId)
                        {
                            entry.Property("UserId").CurrentValue = tenantId;
                        }
                    }

                    var createdByProp = entry.Metadata.FindProperty("CreatedBy");
                    if (createdByProp != null && createdByProp.ClrType == typeof(string))
                    {
                        var currentCreatedBy = (string?)entry.Property("CreatedBy").CurrentValue;
                        if (string.IsNullOrWhiteSpace(currentCreatedBy) || currentCreatedBy == tenantId.ToString())
                        {
                            entry.Property("CreatedBy").CurrentValue = _tenantProvider.RealUserId.ToString();
                        }
                    }
                }
            }
        }
    }
}
