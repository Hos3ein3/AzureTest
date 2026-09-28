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
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.Expenses.Commands;

// ── Command ───────────────────────────────────────────────────────────────────

public sealed record UpdateExpenseCommand(UpdateExpenseDto Dto) : ICommand<bool>;

// ── Validator ─────────────────────────────────────────────────────────────────

public sealed class UpdateExpenseCommandValidator : AbstractValidator<UpdateExpenseCommand>
{
    public UpdateExpenseCommandValidator()
    {
        RuleFor(x => x.Dto.Id).NotEmpty().WithMessage("Id is required.");
        RuleFor(x => x.Dto.Amount).GreaterThan(0).WithMessage("Amount must be greater than 0.");
        RuleFor(x => x.Dto.Date).NotEmpty().WithMessage("Date is required.");
        RuleFor(x => x.Dto.Description).NotEmpty().WithMessage("Description is required.");
        RuleFor(x => x.Dto)
            .Must(x => x.CategoryId.HasValue || !string.IsNullOrWhiteSpace(x.NewCategoryName))
            .WithMessage("Either CategoryId or NewCategoryName must be provided.");
    }
}

// ── Handler ───────────────────────────────────────────────────────────────────

public sealed class UpdateExpenseCommandHandler : BaseCommandHandler<UpdateExpenseCommand, bool>
{
    private readonly INoSqlUnitOfWork _noSqlUnitOfWork;
    private readonly ICurrentUser _currentUser;

    public UpdateExpenseCommandHandler(
        IUnitOfWork unitOfWork,
        INoSqlUnitOfWork noSqlUnitOfWork,
        ICurrentUser currentUser,
        ILogger<UpdateExpenseCommandHandler> logger)
        : base(unitOfWork, logger)
    {
        _noSqlUnitOfWork = noSqlUnitOfWork;
        _currentUser = currentUser;
    }

    protected override async Task<bool> ExecuteAsync(UpdateExpenseCommand command, CancellationToken cancellationToken)
    {
        var dto = command.Dto;
        
        var expense = await UnitOfWork.Expenses.GetByIdAsync(dto.Id);
        if (expense == null || expense.UserId != _currentUser.Id)
        {
            return false;
        }

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

        expense.UpdateDetails(dto.Amount, dto.Date, dto.Description, categoryId);

        UnitOfWork.Expenses.Update(expense);
        return true;
    }
}
