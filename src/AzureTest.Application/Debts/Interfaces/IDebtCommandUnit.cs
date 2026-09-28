using System;
using AzureTest.Application.Debts.Commands;
using AzureTest.Application.Messaging;

namespace AzureTest.Application.Debts;

public interface IDebtCommandUnit
{
    ICommandHandler<CreateDebtCommand, Guid> Create { get; }
    ICommandHandler<UpdateDebtCommand, bool> Update { get; }
    ICommandHandler<DeleteDebtCommand, bool> Delete { get; }
}
