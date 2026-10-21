using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Messaging;
using AzureTest.Domain.Entities.Crypto;
using FluentValidation;

namespace AzureTest.Application.Crypto.Commands.ResetCryptoAccount;

public sealed record ResetCryptoAccountCommand(Guid AccountId, decimal InitialBalance = 10000m) : ICommand<Result>;

public sealed class ResetCryptoAccountCommandValidator : AbstractValidator<ResetCryptoAccountCommand>
{
    public ResetCryptoAccountCommandValidator()
    {
        RuleFor(x => x.AccountId).NotEmpty();
        RuleFor(x => x.InitialBalance).GreaterThan(CryptoAccount.Rules.MinBalance);
    }
}

public sealed class ResetCryptoAccountCommandHandler : ICommandHandler<ResetCryptoAccountCommand, Result>
{
    private readonly IUnitOfWork _uow;

    public ResetCryptoAccountCommandHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Result> HandleAsync(ResetCryptoAccountCommand cmd, CancellationToken ct)
    {
        var account = await _uow.CryptoAccounts.GetByIdAsync(cmd.AccountId);
        if (account == null)
        {
            return Result.Failure("Account not found.");
        }

        account.ResetAccount(cmd.InitialBalance);
        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
