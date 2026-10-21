using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Messaging;
using AzureTest.Domain.Entities.Crypto;
using AzureTest.Domain.Entities.Crypto.Enums;
using FluentValidation;

namespace AzureTest.Application.Crypto.Commands.CreateCryptoAlert;

public sealed record CreateCryptoAlertCommand(
    Guid UserId,
    Guid TenantId,
    string Symbol,
    CryptoAlertType AlertType,
    decimal TargetValue,
    string? Message = null,
    string? Channel = null) : ICommand<Result<Guid>>
{
    public static class Mapping
    {
        public static CryptoAlert ToDomain(CreateCryptoAlertCommand cmd) =>
            CryptoAlert.Create(cmd.UserId, cmd.TenantId, cmd.Symbol, cmd.AlertType, cmd.TargetValue, cmd.Message, cmd.Channel);
    }
}

public sealed class CreateCryptoAlertCommandValidator : AbstractValidator<CreateCryptoAlertCommand>
{
    public CreateCryptoAlertCommandValidator()
    {
        RuleFor(x => x.Symbol)
            .NotEmpty()
            .MaximumLength(CryptoAlert.Rules.SymbolMaxLength);

        RuleFor(x => x.TargetValue)
            .GreaterThan(0m);
    }
}

public sealed class CreateCryptoAlertCommandHandler : ICommandHandler<CreateCryptoAlertCommand, Result<Guid>>
{
    private readonly IUnitOfWork _uow;

    public CreateCryptoAlertCommandHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Result<Guid>> HandleAsync(CreateCryptoAlertCommand cmd, CancellationToken ct)
    {
        var alert = CreateCryptoAlertCommand.Mapping.ToDomain(cmd);
        _uow.CryptoAlerts.Add(alert);
        await _uow.SaveChangesAsync(ct);
        return Result<Guid>.Success(alert.Id);
    }
}
