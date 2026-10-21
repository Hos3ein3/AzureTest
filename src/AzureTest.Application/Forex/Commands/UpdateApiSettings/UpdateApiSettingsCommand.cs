using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Forex;
using AzureTest.Domain.Entities.Forex;
using FluentValidation;

namespace AzureTest.Application.Forex.Commands.UpdateApiSettings;

public sealed record UpdateApiSettingsCommand(ForexApiSettingsDto Settings) : ICommand<Result>;

public sealed class UpdateApiSettingsCommandValidator : AbstractValidator<UpdateApiSettingsCommand>
{
    public UpdateApiSettingsCommandValidator()
    {
        RuleFor(x => x.Settings.TwelveDataApiKey).MaximumLength(ForexApiSetting.Rules.KeyMaxLength);
        RuleFor(x => x.Settings.AlphaVantageApiKey).MaximumLength(ForexApiSetting.Rules.KeyMaxLength);
        RuleFor(x => x.Settings.FredApiKey).MaximumLength(ForexApiSetting.Rules.KeyMaxLength);
        RuleFor(x => x.Settings.FinnhubApiKey).MaximumLength(ForexApiSetting.Rules.KeyMaxLength);
        RuleFor(x => x.Settings.OandaApiKey).MaximumLength(ForexApiSetting.Rules.KeyMaxLength);
        RuleFor(x => x.Settings.PrimaryPriceProvider).MaximumLength(ForexApiSetting.Rules.ProviderMaxLength);
        RuleFor(x => x.Settings.SimulatedLatencyMs).InclusiveBetween(ForexApiSetting.Rules.SimulatedDelayMsMin, ForexApiSetting.Rules.SimulatedDelayMsMax);
    }
}

public sealed class UpdateApiSettingsCommandHandler : ICommandHandler<UpdateApiSettingsCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;

    public UpdateApiSettingsCommandHandler(IUnitOfWork unitOfWork, ICurrentUser currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Result> HandleAsync(UpdateApiSettingsCommand command, CancellationToken cancellationToken)
    {
        var existingList = await _unitOfWork.ForexApiSettings.FindAsync(s => s.UserId == _currentUser.Id);
        var existing = existingList.FirstOrDefault();

        var s = command.Settings;
        if (existing == null)
        {
            var created = ForexApiSetting.Create(
                _currentUser.Id,
                s.TwelveDataApiKey,
                s.AlphaVantageApiKey,
                s.FredApiKey,
                s.FinnhubApiKey,
                s.OandaApiKey,
                s.OandaAccountId,
                s.PrimaryPriceProvider,
                s.UseSimulatedExecutionBroker,
                s.SimulatedLatencyMs,
                s.EnableSafeRetries,
                s.EnableNewsFilterWarning
            );
            _unitOfWork.ForexApiSettings.Add(created);
        }
        else
        {
            existing.UpdateKeys(
                s.TwelveDataApiKey,
                s.AlphaVantageApiKey,
                s.FredApiKey,
                s.FinnhubApiKey,
                s.OandaApiKey,
                s.OandaAccountId,
                s.PrimaryPriceProvider,
                s.UseSimulatedExecutionBroker,
                s.SimulatedLatencyMs,
                s.EnableSafeRetries,
                s.EnableNewsFilterWarning
            );
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
