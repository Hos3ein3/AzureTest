

using System;

namespace AzureTest.Domain.Documents;

public class DebtPerson
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
}
