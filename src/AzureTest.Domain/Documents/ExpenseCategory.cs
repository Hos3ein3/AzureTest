using System;

namespace AzureTest.Domain.Documents;

public class ExpenseCategory
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
}
