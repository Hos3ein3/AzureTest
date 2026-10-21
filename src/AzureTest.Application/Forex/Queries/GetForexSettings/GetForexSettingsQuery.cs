using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Forex;
using FluentValidation;

namespace AzureTest.Application.Forex.Queries.GetForexSettings;

public sealed record GetForexSettingsQuery : IQuery<Result<ForexApiSettingsDto>>;

public sealed class GetForexSettingsQueryValidator : AbstractValidator<GetForexSettingsQuery>
{
    public GetForexSettingsQueryValidator()
    {
    }
}

public sealed class GetForexSettingsQueryHandler : IQueryHandler<GetForexSettingsQuery, Result<ForexApiSettingsDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;

    public GetForexSettingsQueryHandler(IUnitOfWork unitOfWork, ICurrentUser currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Result<ForexApiSettingsDto>> HandleAsync(GetForexSettingsQuery query, CancellationToken cancellationToken)
    {
        var settingsList = await _unitOfWork.ForexApiSettings.FindAsync(s => s.UserId == _currentUser.Id);
        var s = settingsList.FirstOrDefault();

        if (s == null)
        {
            return Result<ForexApiSettingsDto>.Success(new ForexApiSettingsDto(
                "", "", "", "", "", "", "Frankfurter", true, 150, true, true
            ));
        }

        return Result<ForexApiSettingsDto>.Success(new ForexApiSettingsDto(
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
        ));
    }
}
