using System;
using AzureTest.Application.Expenses.Commands;
using AzureTest.Application.Messaging;

namespace AzureTest.Application.Expenses.Units;

public sealed class ExpenseCommandUnit : IExpenseCommandUnit
{
    public ICommandHandler<CreateExpenseCommand, Guid> Create { get; }
    public ICommandHandler<UpdateExpenseCommand, bool> Update { get; }
    public ICommandHandler<DeleteExpenseCommand, bool> Delete { get; }

    public ExpenseCommandUnit(
        ICommandHandler<CreateExpenseCommand, Guid> create,
        ICommandHandler<UpdateExpenseCommand, bool> update,
        ICommandHandler<DeleteExpenseCommand, bool> delete)
    {
        Create = create;
        Update = update;
        Delete = delete;
    }
}
