using System;
using System.Collections.Generic;

namespace AzureTest.Domain.Documents;

public class HouseSearchHistory
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
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
