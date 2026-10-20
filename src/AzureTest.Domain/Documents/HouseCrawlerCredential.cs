using System;
using AzureTest.Domain.Enums;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Documents;

public class HouseCrawlerCredential : ITenantEntity
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
    public Guid TenantId { get; set; }
    public Guid UserId { get; set; }
    public HouseWebsite Website { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty; // Just a placeholder for encrypted password
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    public DateTime? LastUsedOn { get; set; }
}
