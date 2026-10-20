using System;
using System.Collections.Generic;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Documents;

public class HouseSearchHistory : ITenantEntity
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
    public Guid TenantId { get; set; }
    public Guid UserId { get; set; }
    public string City { get; set; } = string.Empty;
    public string MinPrice { get; set; } = string.Empty;
    public string MaxPrice { get; set; } = string.Empty;
    public string Bedrooms { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public DateTime SearchDate { get; set; } = DateTime.UtcNow;
    // Preferences for future searches
    public bool EmailAlertsEnabled { get; set; }
}
