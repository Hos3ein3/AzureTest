using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Expenses;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.Expenses.Queries;

// ── Query ─────────────────────────────────────────────────────────────────────

public sealed record GetExpenseByIdQuery(Guid Id) : IQuery<ExpenseDto?>;

// ── Handler ───────────────────────────────────────────────────────────────────

public sealed class GetExpenseByIdQueryHandler : BaseQueryHandler<GetExpenseByIdQuery, ExpenseDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INoSqlUnitOfWork _noSqlUnitOfWork;
    private readonly ICurrentUser _currentUser;

    public GetExpenseByIdQueryHandler(
        IUnitOfWork unitOfWork,
        INoSqlUnitOfWork noSqlUnitOfWork,
        ICurrentUser currentUser,
        ILogger<GetExpenseByIdQueryHandler> logger)
        : base(logger)
    {
        _unitOfWork = unitOfWork;
        _noSqlUnitOfWork = noSqlUnitOfWork;
        _currentUser = currentUser;
    }

    protected override async Task<ExpenseDto?> ExecuteAsync(GetExpenseByIdQuery query, CancellationToken cancellationToken)
    {
        var expense = await _unitOfWork.Expenses.GetByIdAsync(query.Id);

        if (expense == null || expense.IsDeleted || expense.UserId != _currentUser.Id)
        {
            return null;
        }

        var dto = new ExpenseDto
        {
            Id = expense.Id,
            Amount = expense.Amount,
            Date = expense.Date,
            Description = expense.Description,
            CategoryId = expense.CategoryId
        };

        if (dto.CategoryId.HasValue)
        {
            var category = await _noSqlUnitOfWork.ExpenseCategories.GetByIdAsync(dto.CategoryId.Value);
            if (category != null)
            {
                dto.CategoryName = category.Name;
            }
        }

        return dto;
    }
}
