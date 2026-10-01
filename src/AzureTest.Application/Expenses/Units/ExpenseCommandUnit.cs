using AzureTest.Application.Abstractions;


using System;
using AzureTest.Application.Expenses.Commands;
using AzureTest.Application.Messaging;

namespace AzureTest.Application.Expenses.Units;

public sealed class ExpenseCommandUnit : IExpenseCommandUnit
{
    public ICommandHandler<CreateExpenseCommand, Result<Guid>> Create { get; }
    public ICommandHandler<UpdateExpenseCommand, Result<bool>> Update { get; }
    public ICommandHandler<DeleteExpenseCommand, Result<bool>> Delete { get; }

    public ExpenseCommandUnit(
        ICommandHandler<CreateExpenseCommand, Result<Guid>> create,
        ICommandHandler<UpdateExpenseCommand, Result<bool>> update,
        ICommandHandler<DeleteExpenseCommand, Result<bool>> delete)
    {
        Create = create;
        Update = update;
        Delete = delete;
    }
}
