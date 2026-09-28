using AzureTest.Application.Expenses.Queries;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Common;
using AzureTest.Contracts.DTOs.Expenses;

namespace AzureTest.Application.Expenses.Units;

public interface IExpenseQueryUnit
{
    IQueryHandler<GetAllExpensesQuery, PagedResult<ExpenseDto>> GetAll { get; }
    IQueryHandler<GetExpenseByIdQuery, ExpenseDto?> GetById { get; }
}
