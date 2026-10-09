using System;

namespace AzureTest.Application.Debts.Commands;

public interface IDebtFormModel
{
    decimal Amount { get; }
    DateTime DateOfBorrow { get; }
    DateTime? DateOfReturn { get; }
    DateTime? PromissedReturnDate { get; }
    decimal Profit { get; }
    Guid? DebtPersonId { get; }
    string? NewDebtPersonName { get; }
    string? Description { get; }
    bool IsDone { get; }
}
