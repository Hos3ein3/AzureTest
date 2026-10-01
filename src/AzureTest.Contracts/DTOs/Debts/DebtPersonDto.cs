

using System;

namespace AzureTest.Contracts.DTOs.Debts;

public record DebtPersonDto
{
    public Guid Id { get; init; }
    public string UserId { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
}
