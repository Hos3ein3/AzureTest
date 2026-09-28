using System;
using AzureTest.Application.Debts.Commands;
using AzureTest.Application.Debts.Interfaces;
using AzureTest.Application.Messaging;

namespace AzureTest.Application.Debts;

public sealed class DebtCommandUnit : IDebtCommandUnit
{
    public ICommandHandler<CreateDebtCommand, Guid> Create { get; }
    public ICommandHandler<UpdateDebtCommand, bool> Update { get; }
    public ICommandHandler<DeleteDebtCommand, bool> Delete { get; }

    public DebtCommandUnit(
        ICommandHandler<CreateDebtCommand, Guid> create,
        ICommandHandler<UpdateDebtCommand, bool> update,
        ICommandHandler<DeleteDebtCommand, bool> delete)
    {
        Create = create;
        Update = update;
        Delete = delete;
    }
}
