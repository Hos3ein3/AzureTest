using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Contracts.DTOs.Forex;

namespace AzureTest.Application.Forex.Services;

public interface IForexRiskService
{
    LotSizeCalculationResultDto CalculateLotSize(LotSizeCalculationRequestDto request);
    NewsRiskFilterDto CheckNewsTradeFilter(IReadOnlyList<EconomicEventDto> upcomingEvents, int filterWindowMinutes = 30);
    IReadOnlyList<CurrencyExposureDto> CalculateCurrencyExposures(IReadOnlyList<ForexTradeDto> openTrades, decimal accountEquity);
    EsmaAfmComplianceDto GetRegulatoryComplianceProfile();
    ForexRiskOverviewDto GetRiskOverview(ForexAccountDto account, IReadOnlyList<ForexTradeDto> openTrades, IReadOnlyList<EconomicEventDto> upcomingEvents);
}
