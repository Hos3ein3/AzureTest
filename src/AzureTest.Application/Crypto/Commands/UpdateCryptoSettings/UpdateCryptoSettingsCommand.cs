using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Messaging;
using AzureTest.Domain.Entities.Crypto;
using AzureTest.Domain.Entities.Crypto.Enums;
using FluentValidation;

namespace AzureTest.Application.Crypto.Commands.UpdateCryptoSettings;

public sealed record UpdateCryptoSettingsCommand(
    Guid UserId,
    Guid TenantId,
    CryptoExchange Exchange,
    string ApiKey,
    string ApiSecret,
    string? Passphrase,
    bool IsTestnet) : ICommand<Result>;

public sealed class UpdateCryptoSettingsCommandValidator : AbstractValidator<UpdateCryptoSettingsCommand>
{
    public UpdateCryptoSettingsCommandValidator()
    {
        RuleFor(x => x.ApiKey)
            .NotEmpty()
            .MaximumLength(150);

        RuleFor(x => x.ApiSecret)
            .NotEmpty()
            .MaximumLength(150);
    }
}

public sealed class UpdateCryptoSettingsCommandHandler : ICommandHandler<UpdateCryptoSettingsCommand, Result>
{
    private readonly IUnitOfWork _uow;

    public UpdateCryptoSettingsCommandHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Result> HandleAsync(UpdateCryptoSettingsCommand cmd, CancellationToken ct)
    {
        // Simple deterministic obfuscation for demo key storage
        string encKey = Convert.ToBase64String(Encoding.UTF8.GetBytes(cmd.ApiKey));
        string encSecret = Convert.ToBase64String(Encoding.UTF8.GetBytes(cmd.ApiSecret));
        string? encPass = string.IsNullOrWhiteSpace(cmd.Passphrase) ? null : Convert.ToBase64String(Encoding.UTF8.GetBytes(cmd.Passphrase));

        var existingList = await _uow.CryptoApiSettings.FindAsync(x => x.UserId == cmd.UserId && x.Exchange == cmd.Exchange);
        var existing = existingList.FirstOrDefault();

        if (existing != null)
        {
            existing.UpdateKeys(cmd.ApiKey, encKey, encSecret, encPass, cmd.IsTestnet);
            existing.MarkConnectionSuccess();
        }
        else
        {
            var setting = CryptoApiSetting.Create(
                cmd.UserId,
                cmd.TenantId,
                cmd.Exchange,
                cmd.ApiKey,
                encKey,
                encSecret,
                encPass,
                cmd.IsTestnet);
            setting.MarkConnectionSuccess();
            _uow.CryptoApiSettings.Add(setting);
        }

        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
