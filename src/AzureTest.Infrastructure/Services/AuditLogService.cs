using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Domain.Documents.Audit;
using AzureTest.Infrastructure.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AzureTest.Infrastructure.Services;

/// <summary>
/// Service responsible for recording and retrieving immutable audit logs into MongoDB.
/// Enforces multi-tenancy rules:
/// - Standard users can only view logs matching their UserId / TenantId.
/// - Admins can view all platform logs (or actively filtered tenant logs).
/// </summary>
public class AuditLogService : IAuditLogService
{
    private readonly INoSqlUnitOfWork _noSqlUnitOfWork;
    private readonly ITenantProvider _tenantProvider;
    private readonly ICurrentUser _currentUser;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly InfrastructureFeatureOptions _featureOptions;
    private readonly ILogger<AuditLogService> _logger;

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        ReferenceHandler = ReferenceHandler.IgnoreCycles
    };

    public AuditLogService(
        INoSqlUnitOfWork noSqlUnitOfWork,
        ITenantProvider tenantProvider,
        ICurrentUser currentUser,
        IHttpContextAccessor httpContextAccessor,
        IOptions<InfrastructureFeatureOptions> featureOptions,
        ILogger<AuditLogService> logger)
    {
        _noSqlUnitOfWork = noSqlUnitOfWork;
        _tenantProvider = tenantProvider;
        _currentUser = currentUser;
        _httpContextAccessor = httpContextAccessor;
        _featureOptions = featureOptions.Value;
        _logger = logger;
    }

    public Task LogCommandAsync<TCommand>(TCommand command, long durationMs, bool isSuccess, string? error = null, CancellationToken cancellationToken = default)
    {
        var actionName = typeof(TCommand).Name;
        return LogAsync("Command", actionName, command, durationMs, isSuccess, error, cancellationToken);
    }

    public Task LogQueryAsync<TQuery>(TQuery query, long durationMs, bool isSuccess, string? error = null, CancellationToken cancellationToken = default)
    {
        var actionName = typeof(TQuery).Name;
        return LogAsync("Query", actionName, query, durationMs, isSuccess, error, cancellationToken);
    }

    public async Task LogAsync(
        string actionType,
        string actionName,
        object? payload,
        long durationMs,
        bool isSuccess,
        string? error = null,
        CancellationToken cancellationToken = default)
    {
        // 1. Check feature flag
        if (!_featureOptions.ImmutableAuditLog)
        {
            return;
        }

        // 2. Prevent recursion when fetching audit logs themselves
        if (!string.IsNullOrWhiteSpace(actionName) && actionName.Contains("AuditLog", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        try
        {
            var userId = _currentUser.Id;
            var tenantId = _tenantProvider.CurrentTenantId;

            // In impersonation mode, attribute the tenant workspace being managed
            var impersonatorAdminId = _tenantProvider.IsAdminImpersonating
                ? (Guid?)_tenantProvider.RealUserId
                : null;

            var userName = !string.IsNullOrWhiteSpace(_currentUser.FullName)
                ? _currentUser.FullName
                : _currentUser.Email;

            var ipAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();

            var sanitizedPayload = SanitizePayload(payload);

            var auditEntry = new AuditLogDocument
            {
                TenantId = tenantId,
                UserId = userId,
                UserName = userName,
                UserEmail = _currentUser.Email,
                ActionType = actionType,
                ActionName = actionName,
                Payload = sanitizedPayload,
                ExecutionDurationMs = Math.Max(0, durationMs),
                IsSuccess = isSuccess,
                ErrorMessage = error,
                IpAddress = ipAddress,
                ImpersonatedByAdminId = impersonatorAdminId,
                Timestamp = DateTime.UtcNow
            };

            await _noSqlUnitOfWork.AuditLogs.AddAsync(auditEntry);
            await _noSqlUnitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            // Audit logging must be fail-safe and never abort the caller's main business flow
            _logger.LogError(ex, "Failed to record audit log for {ActionType} {ActionName}: {Message}", actionType, actionName, ex.Message);
        }
    }

    public async Task<(IReadOnlyList<AuditLogDocument> Logs, long TotalCount)> GetLogsAsync(
        bool isAdmin,
        Guid currentUserId,
        Guid currentTenantId,
        string? actionType = null,
        string? searchTerm = null,
        int pageNumber = 1,
        int pageSize = 50,
        Guid? filterUserId = null,
        CancellationToken cancellationToken = default)
    {
        // Strict Security Isolation:
        // Non-admin users are strictly scoped to their own logs
        Guid? targetTenantId = null;
        Guid? targetUserId = null;

        if (!isAdmin)
        {
            targetUserId = currentUserId;
            if (currentTenantId != Guid.Empty)
            {
                targetTenantId = currentTenantId;
            }
        }
        else
        {
            if (filterUserId.HasValue && filterUserId.Value != Guid.Empty)
            {
                targetUserId = filterUserId.Value;
            }

            if (_tenantProvider.IsAdminImpersonating)
            {
                // When Admin is actively impersonating, show logs for the impersonated tenant
                targetTenantId = _tenantProvider.CurrentTenantId;
            }
        }

        var logs = await _noSqlUnitOfWork.AuditLogs.GetFilteredLogsAsync(
            targetTenantId,
            targetUserId,
            actionType,
            searchTerm,
            pageNumber,
            pageSize,
            cancellationToken);

        var total = await _noSqlUnitOfWork.AuditLogs.CountFilteredLogsAsync(
            targetTenantId,
            targetUserId,
            actionType,
            searchTerm,
            cancellationToken);

        return (logs, total);
    }

    public async Task<long> DeleteLogByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _noSqlUnitOfWork.AuditLogs.DeleteLogsAsync(id, null, null, null, cancellationToken);
    }

    public async Task<long> DeleteLogsByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _noSqlUnitOfWork.AuditLogs.DeleteLogsAsync(null, userId, null, null, cancellationToken);
    }

    public async Task<long> DeleteLogsByDateRangeAsync(DateTime? fromUtc, DateTime? toUtc, CancellationToken cancellationToken = default)
    {
        return await _noSqlUnitOfWork.AuditLogs.DeleteLogsAsync(null, null, fromUtc, toUtc, cancellationToken);
    }

    public async Task<long> PurgeAllLogsAsync(CancellationToken cancellationToken = default)
    {
        return await _noSqlUnitOfWork.AuditLogs.PurgeAllLogsAsync(cancellationToken);
    }

    private static string? SanitizePayload(object? payload)
    {
        if (payload == null)
            return null;

        try
        {
            var json = JsonSerializer.Serialize(payload, SerializerOptions);

            // Redact common sensitive credentials
            json = Regex.Replace(
                json,
                @"(""(?:password|confirmPassword|token|secret|key|authorization|cookie)"":\s*"")[^""]*("")",
                "$1***$2",
                RegexOptions.IgnoreCase);

            return json.Length > 2000
                ? string.Concat(json.AsSpan(0, 2000), "... [truncated]")
                : json;
        }
        catch
        {
            return payload.GetType().Name;
        }
    }
}
