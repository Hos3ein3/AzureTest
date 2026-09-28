using AzureTest.Application.Expenses.Queries;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Common;
using AzureTest.Contracts.DTOs.Expenses;

namespace AzureTest.Application.Expenses.Units;

public sealed class ExpenseQueryUnit : IExpenseQueryUnit
{
    public IQueryHandler<GetAllExpensesQuery, PagedResult<ExpenseDto>> GetAll { get; }
    public IQueryHandler<GetExpenseByIdQuery, ExpenseDto?> GetById { get; }

    public ExpenseQueryUnit(
        IQueryHandler<GetAllExpensesQuery, PagedResult<ExpenseDto>> getAll,
        IQueryHandler<GetExpenseByIdQuery, ExpenseDto?> getById)
    {
        GetAll = getAll;
        GetById = getById;
    }
}
