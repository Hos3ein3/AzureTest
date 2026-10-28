using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Messaging;
using AzureTest.Domain.Entities.Trading.Enums;
using FluentValidation;

namespace AzureTest.Application.Trading.Commands.CancelTradingOrder;

public sealed record CancelTradingOrderCommand(
    Guid OrderId,
    Guid UserId,
    Guid TenantId) : ICommand<Result<Guid>>;

public sealed class CancelTradingOrderCommandValidator : AbstractValidator<CancelTradingOrderCommand>
{
    public CancelTradingOrderCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
    }
}

public sealed class CancelTradingOrderCommandHandler : ICommandHandler<CancelTradingOrderCommand, Result<Guid>>
{
    private readonly IUnitOfWork _uow;

    public CancelTradingOrderCommandHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Result<Guid>> HandleAsync(CancelTradingOrderCommand cmd, CancellationToken ct)
    {
        var order = await _uow.TradingOrders.GetByIdAsync(cmd.OrderId);
        if (order == null || order.Status != TradingOrderStatus.Pending || order.UserId != cmd.UserId || (cmd.TenantId != Guid.Empty && order.TenantId != cmd.TenantId))
        {
            return Result<Guid>.Failure("Pending order not found or already processed.");
        }

        order.Cancel();
        await _uow.SaveChangesAsync(ct);
        return Result<Guid>.Success(order.Id);
    }
}
