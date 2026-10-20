

using System;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Documents;

public class ExpenseCategory : ITenantEntity
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
    public Guid TenantId { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
}
