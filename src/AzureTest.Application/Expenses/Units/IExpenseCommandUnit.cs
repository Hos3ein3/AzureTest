using System;
using AzureTest.Application.Expenses.Commands;
using AzureTest.Application.Messaging;

namespace AzureTest.Application.Expenses.Units;

public interface IExpenseCommandUnit
{
    ICommandHandler<CreateExpenseCommand, Guid> Create { get; }
    ICommandHandler<UpdateExpenseCommand, bool> Update { get; }
    ICommandHandler<DeleteExpenseCommand, bool> Delete { get; }
}
