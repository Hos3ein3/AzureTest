using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Forex.Services;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Forex;
using AzureTest.Domain.Entities.Forex;
using AzureTest.Domain.Entities.Forex.Enums;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.Forex.Commands.PlaceOrder;

public sealed record PlaceOrderCommand(
    string Symbol,
    ForexTradeDirection Direction,
    ForexOrderType OrderType,
    decimal LotSize,
    decimal RequestedPrice,
    decimal? StopLoss,
    decimal? TakeProfit,
    string? IdempotencyKey = null
) : ICommand<Result<Guid>>
{
    public static class Mapping
    {
        public static ForexOrderDto ToDto(ForexOrder o) =>
            new(o.Id, o.AccountId, o.Symbol, o.Direction, o.OrderType, o.LotSize, o.RequestedPrice, o.FillPrice, o.StopLoss, o.TakeProfit, o.SlippagePips, o.Status, o.IdempotencyKey, o.RejectionReason, o.CreatedOn, o.FilledAtUtc);
    }
}

public sealed class PlaceOrderCommandValidator : AbstractValidator<PlaceOrderCommand>
{
    public PlaceOrderCommandValidator()
    {
        RuleFor(x => x.Symbol)
            .NotEmpty()
            .MinimumLength(ForexOrder.Rules.SymbolMinLength)
            .MaximumLength(ForexOrder.Rules.SymbolMaxLength);

        RuleFor(x => x.LotSize)
            .GreaterThanOrEqualTo(ForexOrder.Rules.LotSizeMin)
            .LessThanOrEqualTo(ForexOrder.Rules.LotSizeMax)
            .PrecisionScale(ForexOrder.Rules.LotSizePrecision, ForexOrder.Rules.LotSizeScale, false);

        RuleFor(x => x.Direction)
            .IsInEnum();

        RuleFor(x => x.OrderType)
            .IsInEnum();
    }
}

public sealed class PlaceOrderCommandHandler : ICommandHandler<PlaceOrderCommand, Result<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IForexExecutionService _executionService;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<PlaceOrderCommandHandler> _logger;

    public PlaceOrderCommandHandler(
        IUnitOfWork unitOfWork,
        IForexExecutionService executionService,
        ICurrentUser currentUser,
        ILogger<PlaceOrderCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _executionService = executionService;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<Result<Guid>> HandleAsync(PlaceOrderCommand command, CancellationToken cancellationToken)
    {
        var accounts = await _unitOfWork.ForexAccounts.FindAsync(a => a.UserId == _currentUser.Id);
        var account = accounts.FirstOrDefault();
        if (account == null)
        {
            account = ForexAccount.Create(_currentUser.Id, "Demo Primary", "USD", 10_000m, 30, true);
            _unitOfWork.ForexAccounts.Add(account);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        var execResult = await _executionService.ExecuteOrderAsync(
            account,
            command.Symbol,
            command.Direction,
            command.OrderType,
            command.LotSize,
            command.RequestedPrice,
            command.StopLoss,
            command.TakeProfit,
            command.IdempotencyKey ?? Guid.NewGuid().ToString("N"),
            cancellationToken
        );

        if (execResult.IsFailure)
        {
            return Result<Guid>.Failure(execResult.Error ?? "Failed to execute order.");
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(execResult.Value.Id);
    }
}
