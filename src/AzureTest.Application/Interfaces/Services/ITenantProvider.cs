using System;

namespace AzureTest.Application.Interfaces.Services;

/// <summary>
/// Provides access to the current tenant execution context.
/// Handles multi-tenancy state, admin tenant switching / impersonation, and feature flag status.
/// </summary>
public interface ITenantProvider
{
    /// <summary>
    /// Gets the active tenant identifier for the current request.
    /// Returns Guid.Empty when multi-tenancy is disabled in configuration.
    /// For normal users, always equals their authenticated UserId.
    /// For Admins, equals either their active impersonated tenant ID (cookie/header) or their own UserId.
    /// </summary>
    Guid CurrentTenantId { get; }

    /// <summary>
    /// Gets the real authenticated user identifier, regardless of whether impersonating another tenant.
    /// </summary>
    Guid RealUserId { get; }

    /// <summary>
    /// Returns true if the current request is an Administrator actively impersonating another tenant workspace.
    /// </summary>
    bool IsAdminImpersonating { get; }

    /// <summary>
    /// Returns true if multi-tenancy is enabled via FeatureManagement in application configuration.
    /// </summary>
    bool IsMultiTenancyEnabled { get; }
}
