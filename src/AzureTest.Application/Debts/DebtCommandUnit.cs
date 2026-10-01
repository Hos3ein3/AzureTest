using AzureTest.Application.Abstractions;


using System;
using AzureTest.Application.Debts.Commands;
using AzureTest.Application.Debts.Interfaces;
using AzureTest.Application.Messaging;

namespace AzureTest.Application.Debts;

public sealed class DebtCommandUnit : IDebtCommandUnit
{
    public ICommandHandler<CreateDebtCommand, Result<Guid>> Create { get; }
    public ICommandHandler<UpdateDebtCommand, Result<bool>> Update { get; }
    public ICommandHandler<DeleteDebtCommand, Result<bool>> Delete { get; }

    public DebtCommandUnit(
        ICommandHandler<CreateDebtCommand, Result<Guid>> create,
        ICommandHandler<UpdateDebtCommand, Result<bool>> update,
        ICommandHandler<DeleteDebtCommand, Result<bool>> delete)
    {
        Create = create;
        Update = update;
        Delete = delete;
    }
}
