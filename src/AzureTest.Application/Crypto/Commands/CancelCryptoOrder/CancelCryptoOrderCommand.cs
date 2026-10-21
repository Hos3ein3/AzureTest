using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Messaging;
using AzureTest.Domain.Entities.Crypto.Enums;
using FluentValidation;

namespace AzureTest.Application.Crypto.Commands.CancelCryptoOrder;

public sealed record CancelCryptoOrderCommand(Guid OrderId) : ICommand<Result>;

public sealed class CancelCryptoOrderCommandValidator : AbstractValidator<CancelCryptoOrderCommand>
{
    public CancelCryptoOrderCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
    }
}

public sealed class CancelCryptoOrderCommandHandler : ICommandHandler<CancelCryptoOrderCommand, Result>
{
    private readonly IUnitOfWork _uow;

    public CancelCryptoOrderCommandHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Result> HandleAsync(CancelCryptoOrderCommand cmd, CancellationToken ct)
    {
        var order = await _uow.CryptoOrders.GetByIdAsync(cmd.OrderId);
        if (order == null)
        {
            return Result.Failure("Order not found.");
        }

        if (order.Status != CryptoOrderStatus.Pending)
        {
            return Result.Failure("Only pending orders can be cancelled.");
        }

        order.Cancel();
        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
