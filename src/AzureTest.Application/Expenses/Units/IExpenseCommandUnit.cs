using AzureTest.Application.Abstractions;


using System;
using AzureTest.Application.Expenses.Commands;
using AzureTest.Application.Messaging;

namespace AzureTest.Application.Expenses.Units;

public interface IExpenseCommandUnit
{
    ICommandHandler<CreateExpenseCommand, Result<Guid>> Create { get; }
    ICommandHandler<UpdateExpenseCommand, Result<bool>> Update { get; }
    ICommandHandler<DeleteExpenseCommand, Result<bool>> Delete { get; }
}
