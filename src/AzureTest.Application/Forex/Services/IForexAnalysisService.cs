using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Contracts.DTOs.Forex;
using AzureTest.Domain.Entities.Forex.Enums;

namespace AzureTest.Application.Forex.Services;

public interface IForexAnalysisService
{
    Task<TechnicalOverviewDto> GetTechnicalOverviewAsync(string symbol, ForexTimeframe timeframe, CancellationToken cancellationToken = default);
    Task<FundamentalOverviewDto> GetFundamentalOverviewAsync(CancellationToken cancellationToken = default);
    Task<TechnicalIndicatorsDto> CalculateIndicatorsAsync(string symbol, ForexTimeframe timeframe, CancellationToken cancellationToken = default);
    Task<PivotPointsDto> CalculatePivotsAsync(string symbol, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CurrencyStrengthDto>> GetCurrencyStrengthAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PairCorrelationDto>> GetCorrelationMatrixAsync(CancellationToken cancellationToken = default);
    Task<MultiTimeframeSignalDto> GetMultiTimeframeSignalAsync(string symbol, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EconomicEventDto>> GetEconomicCalendarAsync(ForexEconomicEventFilter filter, CancellationToken cancellationToken = default);
    IReadOnlyList<CentralBankRateDto> GetCentralBankRates();
    IReadOnlyList<CotReportDto> GetCotReports();
    IReadOnlyList<ForexNewsDto> GetNewsFeed();
}
