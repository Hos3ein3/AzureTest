using System;
using System.Collections.Generic;

namespace AzureTest.Contracts.DTOs.UserPreferences;

public class UpdatePreferenceRequestDto
{
    public string? Language { get; set; }
    public string? ThemeColor { get; set; }
    public string? Currency { get; set; }
    public string? PreferredDashboard { get; set; }
    public bool? IsDarkMode { get; set; }
}

public class UserPreferenceResponseDto
{
    public Guid UserId { get; set; }
    public string Language { get; set; } = "en";
    public string ThemeColor { get; set; } = "slate";
    public string Currency { get; set; } = "USD";
    public string PreferredDashboard { get; set; } = string.Empty;
    public bool IsDarkMode { get; set; }
    public IEnumerable<UserPreferenceDashboardDto> Dashboards { get; set; } = [];
}

public class UserPreferenceDashboardDto
{
    public string Slug { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class UpdatePreferenceResultDto
{
    public bool Success { get; set; }
    public string Language { get; set; } = "en";
    public string ThemeColor { get; set; } = "slate";
    public string Currency { get; set; } = "USD";
    public string PreferredDashboard { get; set; } = string.Empty;
    public bool IsDarkMode { get; set; }
}
