

using System;

namespace AzureTest.Contracts.DTOs.Debts;

public record DebtDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public decimal Amount { get; init; }
    public DateTime DateOfBorrow { get; init; }
    public DateTime? DateOfReturn { get; init; }
    public DateTime? PromissedReturnDate { get; init; }
    public decimal Profit { get; init; }
    public Guid DebtPersonId { get; init; }
    public string? DebtPersonName { get; init; }
    public string? Description { get; init; }
    public bool IsDone { get; init; }
    public DateTime CreatedOn { get; init; }
}
