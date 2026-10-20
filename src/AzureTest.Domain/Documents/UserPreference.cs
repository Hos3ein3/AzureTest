

using System;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Documents;

public class UserPreference : ITenantEntity
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
    public Guid TenantId { get; set; }
    public Guid UserId { get; set; }
    public string Language { get; set; } = string.Empty;
    public string ThemeColor { get; set; } = string.Empty;
    public string Currency { get; set; } = "USD";
    public string PreferredDashboard { get; set; } = string.Empty;
    public bool IsDarkMode { get; set; } = false;
}
