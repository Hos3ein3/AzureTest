using System;

namespace AzureTest.Domain.Primitives;

/// <summary>
/// Defines an entity or document scoped to a specific tenant workspace.
/// Used by EF Core global query filters and MongoDB document repositories
/// to provide transparent multi-tenant data isolation.
/// </summary>
public interface ITenantEntity
{
    Guid TenantId { get; set; }
}
