

namespace AzureTest.Infrastructure.Configuration;

public class InfrastructureFeatureOptions
{
    // HealthChecks Features
    public bool EnableUserAccessToDashboardSlugs { get; set; } = false;
    public bool EnableHealthCheck { get; set; } = true;
    public bool EnableHealthCheckUI { get; set; } = true;
    public bool EnableHealthChecksSql { get; set; } = true;
    public bool EnableHealthChecksCosmos { get; set; } = true;
    public bool EnableHealthChecksBlob { get; set; } = true;
    public bool EnableHealthCheckMonitor { get; set; } = true;

    // OpenTelemetry Features
    public bool EnableOpenTelemetry { get; set; } = true;
    public bool EnableAspNetCoreInstrumentation { get; set; } = true;
    public bool EnableRuntimeInstrumentation { get; set; } = true;
    public bool EnableProcessInstrumentation { get; set; } = true;

    // Logging Features
    public bool EnableSerilog { get; set; } = true;

    // Storage Features
    public bool EnableBlobStorage { get; set; } = false;
    public bool EnableAzureMonitorOpenTelemetry { get; set; } = true;
    public bool UseJWT { get; set; } = false;
    public bool UseSeq { get; set; } = false;

    // Multi-Tenancy & Impersonation Features
    public bool EnableMultiTenancy { get; set; } = false;
    public bool AllowImpersonationWrites { get; set; } = false;
    public bool ImmutableAuditLog { get; set; } = false;
    public bool EnableImmutableAuditLog { get => ImmutableAuditLog; set => ImmutableAuditLog = value; }

    // Background Jobs Features
    public bool EnableHangfire { get; set; } = true;

    // Mobile & Tablet Floating Navigation Features
    public bool MobileFloatingNavigation { get; set; } = true;
    public bool MobileFlaotingNavigation { get => MobileFloatingNavigation; set => MobileFloatingNavigation = value; }
    public bool TabletFloatingNavigation { get; set; } = true;
}
