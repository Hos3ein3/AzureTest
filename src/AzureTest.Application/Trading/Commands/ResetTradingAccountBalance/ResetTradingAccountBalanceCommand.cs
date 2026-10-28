using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Messaging;
using AzureTest.Domain.Entities.Trading;
using AzureTest.Domain.Entities.Trading.Enums;
using FluentValidation;

namespace AzureTest.Application.Trading.Commands.ResetTradingAccountBalance;

public sealed record ResetTradingAccountBalanceCommand(
    Guid UserId,
    Guid TenantId,
    decimal NewBalance) : ICommand<Result<Guid>>;

public sealed class ResetTradingAccountBalanceCommandValidator : AbstractValidator<ResetTradingAccountBalanceCommand>
{
    public ResetTradingAccountBalanceCommandValidator()
    {
        RuleFor(x => x.NewBalance)
            .GreaterThan(0m)
            .WithMessage("Paper trading balance must be greater than $0.00 (e.g., $10, $100, $1,000).")
            .LessThanOrEqualTo(TradingAccount.Rules.MaxBalance);
    }
}

public sealed class ResetTradingAccountBalanceCommandHandler : ICommandHandler<ResetTradingAccountBalanceCommand, Result<Guid>>
{
    private readonly IUnitOfWork _uow;

    public ResetTradingAccountBalanceCommandHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Result<Guid>> HandleAsync(ResetTradingAccountBalanceCommand cmd, CancellationToken ct)
    {
        var accounts = await _uow.TradingAccounts.GetAllAsync();
        var account = accounts.FirstOrDefault(a => a.UserId == cmd.UserId && (cmd.TenantId == Guid.Empty || a.TenantId == cmd.TenantId));
        if (account == null)
        {
            account = TradingAccount.Create(cmd.UserId, cmd.TenantId, "Primary Paper Trading Account", "USD", cmd.NewBalance);
            _uow.TradingAccounts.Add(account);
        }
        else
        {
            account.ResetBalance(cmd.NewBalance);
        }

        // Permanently delete all position records (open, closed, liquidated) scoped to this user and tenant
        var positions = (await _uow.TradingPositions.GetAllAsync())
            .Where(p => p.UserId == cmd.UserId && (cmd.TenantId == Guid.Empty || p.TenantId == cmd.TenantId))
            .ToList();

        foreach (var p in positions)
        {
            _uow.TradingPositions.Remove(p);
        }

        // Permanently delete all order records (pending, filled, cancelled) scoped to this user and tenant
        var orders = (await _uow.TradingOrders.GetAllAsync())
            .Where(o => o.UserId == cmd.UserId && (cmd.TenantId == Guid.Empty || o.TenantId == cmd.TenantId))
            .ToList();

        foreach (var o in orders)
        {
            _uow.TradingOrders.Remove(o);
        }

        await _uow.SaveChangesAsync(ct);
        return Result<Guid>.Success(account.Id);
    }
}
