namespace AzureTest.Infrastructure.Configuration;

public class InfrastructureFeatureOptions
{
    // ── Authentication, Identity & Security ─────────────────────────────────
    public bool UseJWT { get; set; } = false;
    public bool UseGoogleOAuthAuthentication { get; set; }
    public bool UseGoogleAuthenticationOpenIDConnect { get; set; }
    public bool EnableLogoutOtherDevices { get; set; } = true;
    public bool EnableDirectUserPermissions { get; set; } = true;

    // ── Multi-Tenancy & Impersonation ────────────────────────────────────────
    public bool EnableMultiTenancy { get; set; } = false;
    public bool AllowImpersonationWrites { get; set; } = false;
    public bool ImmutableAuditLog { get; set; } = false;
    public bool EnableImmutableAuditLog { get => ImmutableAuditLog; set => ImmutableAuditLog = value; }

    // ── Dashboard & Access ───────────────────────────────────────────────────
    public bool EnableUserAccessToDashboardSlugs { get; set; } = false;

    // ── Health Checks ────────────────────────────────────────────────────────
    public bool EnableHealthCheck { get; set; } = true;
    public bool EnableHealthCheckUI { get; set; } = true;
    public bool EnableHealthChecksSql { get; set; } = true;
    public bool EnableHealthChecksCosmos { get; set; } = true;
    public bool EnableHealthChecksBlob { get; set; } = true;
    public bool EnableHealthCheckMonitor { get; set; } = true;

    // ── Background Jobs & Messaging ──────────────────────────────────────────
    public bool EnableHangfire { get; set; } = true;
    public bool EnableRabbitMQ { get; set; } = false;
    public bool EnableQuartz { get; set; } = false;

    // ── Logging & Telemetry ──────────────────────────────────────────────────
    public bool EnableSerilog { get; set; } = true;
    public bool UseSeq { get; set; } = false;
    public bool EnableOpenTelemetry { get; set; } = true;
    public bool EnableAspNetCoreInstrumentation { get; set; } = true;
    public bool EnableRuntimeInstrumentation { get; set; } = true;
    public bool EnableProcessInstrumentation { get; set; } = true;
    public bool EnableAzureMonitorOpenTelemetry { get; set; } = true;

    // ── Storage ──────────────────────────────────────────────────────────────
    public bool EnableBlobStorage { get; set; } = false;

    // ── Mobile & Tablet UI Navigation ────────────────────────────────────────
    public bool MobileFloatingNavigation { get; set; } = true;
    public bool MobileFlaotingNavigation { get => MobileFloatingNavigation; set => MobileFloatingNavigation = value; }
    public bool TabletFloatingNavigation { get; set; } = true;
}
