using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Expenses;
using AzureTest.Domain.Documents;
using AzureTest.Domain.Entities;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.Expenses.Commands;

// ── Command ───────────────────────────────────────────────────────────────────

public sealed record CreateExpenseCommand(CreateExpenseDto Dto) : ICommand<Guid>;

// ── Validator ─────────────────────────────────────────────────────────────────

public sealed class CreateExpenseCommandValidator : AbstractValidator<CreateExpenseCommand>
{
    public CreateExpenseCommandValidator()
    {
        RuleFor(x => x.Dto.Amount).GreaterThan(0).WithMessage("Amount must be greater than 0.");
        RuleFor(x => x.Dto.Date).NotEmpty().WithMessage("Date is required.");
        RuleFor(x => x.Dto.Description).NotEmpty().WithMessage("Description is required.");
        RuleFor(x => x.Dto)
            .Must(x => x.CategoryId.HasValue || !string.IsNullOrWhiteSpace(x.NewCategoryName))
            .WithMessage("Either CategoryId or NewCategoryName must be provided.");
    }
}

// ── Handler ───────────────────────────────────────────────────────────────────

public sealed class CreateExpenseCommandHandler : BaseCommandHandler<CreateExpenseCommand, Guid>
{
    private readonly INoSqlUnitOfWork _noSqlUnitOfWork;
    private readonly ICurrentUser _currentUser;

    public CreateExpenseCommandHandler(
        IUnitOfWork unitOfWork,
        INoSqlUnitOfWork noSqlUnitOfWork,
        ICurrentUser currentUser,
        ILogger<CreateExpenseCommandHandler> logger)
        : base(unitOfWork, logger)
    {
        _noSqlUnitOfWork = noSqlUnitOfWork;
        _currentUser = currentUser;
    }

    protected override async Task<Guid> ExecuteAsync(CreateExpenseCommand command, CancellationToken cancellationToken)
    {
        var dto = command.Dto;
        Guid? categoryId = dto.CategoryId;

        if (categoryId == null && !string.IsNullOrWhiteSpace(dto.NewCategoryName))
        {
            var existing = await _noSqlUnitOfWork.ExpenseCategories.GetByNameAsync(dto.NewCategoryName.Trim(), _currentUser.Id);
            if (existing != null)
            {
                categoryId = existing.Id;
            }
            else
            {
                var newCategory = new ExpenseCategory
                {
                    Name = dto.NewCategoryName.Trim(),
                    UserId = _currentUser.Id
                };
                await _noSqlUnitOfWork.ExpenseCategories.AddAsync(newCategory);
                categoryId = newCategory.Id;
            }
        }

        var expense = Expense.Create(
            _currentUser.Id,
            dto.Amount,
            dto.Date,
            dto.Description,
            categoryId
        );

        UnitOfWork.Expenses.Add(expense);

        return expense.Id;
    }
}
