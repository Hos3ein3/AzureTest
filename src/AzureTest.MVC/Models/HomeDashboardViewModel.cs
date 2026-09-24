using System.Collections.Generic;
using AzureTest.Contracts.DTOs.Resumes;

namespace AzureTest.MVC.Models;

public class HomeDashboardViewModel
{
    public ResumeAnalyticsDto Analytics { get; set; } = null!;
    public bool IsAdmin { get; set; }
    public string CurrentUserId { get; set; } = string.Empty;
    public string CurrentUserName { get; set; } = string.Empty;
    public Dictionary<string, string> AvailableUsers { get; set; } = new();
}
