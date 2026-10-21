using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Messaging;
using AzureTest.Domain.Entities.Forex.Enums;
using FluentValidation;

namespace AzureTest.Application.Forex.Commands.CancelOrder;

public sealed record CancelOrderCommand(Guid OrderId, string? Reason = null) : ICommand<Result>;

public sealed class CancelOrderCommandValidator : AbstractValidator<CancelOrderCommand>
{
    public CancelOrderCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
    }
}

public sealed class CancelOrderCommandHandler : ICommandHandler<CancelOrderCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;

    public CancelOrderCommandHandler(IUnitOfWork unitOfWork, ICurrentUser currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Result> HandleAsync(CancelOrderCommand command, CancellationToken cancellationToken)
    {
        var order = await _unitOfWork.ForexOrders.GetByIdAsync(command.OrderId);
        if (order == null) return Result.Failure("Order not found.");
        if (order.UserId != _currentUser.Id) return Result.Failure("Unauthorized access to order.");

        if (order.Status != ForexOrderStatus.Pending)
        {
            return Result.Failure("Only pending orders can be cancelled.");
        }

        order.MarkCancelled(command.Reason ?? "Cancelled by user");
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
