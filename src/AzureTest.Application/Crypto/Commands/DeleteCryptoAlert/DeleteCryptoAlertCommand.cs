using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Messaging;
using FluentValidation;

namespace AzureTest.Application.Crypto.Commands.DeleteCryptoAlert;

public sealed record DeleteCryptoAlertCommand(Guid AlertId) : ICommand<Result>;

public sealed class DeleteCryptoAlertCommandValidator : AbstractValidator<DeleteCryptoAlertCommand>
{
    public DeleteCryptoAlertCommandValidator()
    {
        RuleFor(x => x.AlertId).NotEmpty();
    }
}

public sealed class DeleteCryptoAlertCommandHandler : ICommandHandler<DeleteCryptoAlertCommand, Result>
{
    private readonly IUnitOfWork _uow;

    public DeleteCryptoAlertCommandHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Result> HandleAsync(DeleteCryptoAlertCommand cmd, CancellationToken ct)
    {
        var alert = await _uow.CryptoAlerts.GetByIdAsync(cmd.AlertId);
        if (alert == null)
        {
            return Result.Failure("Alert not found.");
        }

        alert.IsDeleted = true;
        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
