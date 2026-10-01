using AzureTest.Application.Abstractions;


using System;
using AzureTest.Application.Debts.Commands;
using AzureTest.Application.Messaging;

namespace AzureTest.Application.Debts;

public interface IDebtCommandUnit
{
    ICommandHandler<CreateDebtCommand, Result<Guid>> Create { get; }
    ICommandHandler<UpdateDebtCommand, Result<bool>> Update { get; }
    ICommandHandler<DeleteDebtCommand, Result<bool>> Delete { get; }
}
