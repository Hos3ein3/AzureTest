using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Forex.Commands.CancelOrder;
using AzureTest.Application.Forex.Commands.CloseTrade;
using AzureTest.Application.Forex.Commands.CreateBacktest;
using AzureTest.Application.Forex.Commands.PlaceOrder;
using AzureTest.Application.Forex.Commands.ResetDemoAccount;
using AzureTest.Application.Forex.Commands.SaveJournalEntry;
using AzureTest.Application.Forex.Commands.UpdateApiSettings;
using AzureTest.Application.Forex.Commands.UpdateTradeStops;
using AzureTest.Application.Forex.Queries.GetBacktestDetails;
using AzureTest.Application.Forex.Queries.GetForexAnalysis;
using AzureTest.Application.Forex.Queries.GetForexCandles;
using AzureTest.Application.Forex.Queries.GetForexJournal;
using AzureTest.Application.Forex.Queries.GetForexMarketOverview;
using AzureTest.Application.Forex.Queries.GetForexRiskAssessment;
using AzureTest.Application.Forex.Queries.GetForexSettings;
using AzureTest.Application.Forex.Queries.GetForexStrategies;
using AzureTest.Application.Forex.Queries.GetForexTradeDashboard;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Forex;

namespace AzureTest.Application.Forex;

public interface IForexCommandUnit
{
    ICommandHandler<PlaceOrderCommand, Result<Guid>> PlaceOrder { get; }
    ICommandHandler<CancelOrderCommand, Result> CancelOrder { get; }
    ICommandHandler<CloseTradeCommand, Result<decimal>> CloseTrade { get; }
    ICommandHandler<UpdateTradeStopsCommand, Result> UpdateTradeStops { get; }
    ICommandHandler<CreateBacktestCommand, Result<BacktestResultDto>> CreateBacktest { get; }
    ICommandHandler<SaveJournalEntryCommand, Result<Guid>> SaveJournalEntry { get; }
    ICommandHandler<UpdateApiSettingsCommand, Result> UpdateApiSettings { get; }
    ICommandHandler<ResetDemoAccountCommand, Result> ResetDemoAccount { get; }
}

public sealed class ForexCommandUnit : IForexCommandUnit
{
    public ICommandHandler<PlaceOrderCommand, Result<Guid>> PlaceOrder { get; }
    public ICommandHandler<CancelOrderCommand, Result> CancelOrder { get; }
    public ICommandHandler<CloseTradeCommand, Result<decimal>> CloseTrade { get; }
    public ICommandHandler<UpdateTradeStopsCommand, Result> UpdateTradeStops { get; }
    public ICommandHandler<CreateBacktestCommand, Result<BacktestResultDto>> CreateBacktest { get; }
    public ICommandHandler<SaveJournalEntryCommand, Result<Guid>> SaveJournalEntry { get; }
    public ICommandHandler<UpdateApiSettingsCommand, Result> UpdateApiSettings { get; }
    public ICommandHandler<ResetDemoAccountCommand, Result> ResetDemoAccount { get; }

    public ForexCommandUnit(
        ICommandHandler<PlaceOrderCommand, Result<Guid>> placeOrder,
        ICommandHandler<CancelOrderCommand, Result> cancelOrder,
        ICommandHandler<CloseTradeCommand, Result<decimal>> closeTrade,
        ICommandHandler<UpdateTradeStopsCommand, Result> updateTradeStops,
        ICommandHandler<CreateBacktestCommand, Result<BacktestResultDto>> createBacktest,
        ICommandHandler<SaveJournalEntryCommand, Result<Guid>> saveJournalEntry,
        ICommandHandler<UpdateApiSettingsCommand, Result> updateApiSettings,
        ICommandHandler<ResetDemoAccountCommand, Result> resetDemoAccount)
    {
        PlaceOrder = placeOrder;
        CancelOrder = cancelOrder;
        CloseTrade = closeTrade;
        UpdateTradeStops = updateTradeStops;
        CreateBacktest = createBacktest;
        SaveJournalEntry = saveJournalEntry;
        UpdateApiSettings = updateApiSettings;
        ResetDemoAccount = resetDemoAccount;
    }
}

public interface IForexQueryUnit
{
    IQueryHandler<GetForexMarketOverviewQuery, Result<MarketOverviewDto>> GetMarketOverview { get; }
    IQueryHandler<GetForexCandlesQuery, Result<IReadOnlyList<ForexCandleDto>>> GetCandles { get; }
    IQueryHandler<GetForexAnalysisQuery, Result<(TechnicalOverviewDto Technical, FundamentalOverviewDto Fundamental)>> GetAnalysis { get; }
    IQueryHandler<GetForexStrategiesQuery, Result<(IReadOnlyList<ForexStrategyDto> Strategies, IReadOnlyList<BacktestResultDto> RecentBacktests)>> GetStrategies { get; }
    IQueryHandler<GetBacktestDetailsQuery, Result<BacktestResultDto>> GetBacktestDetails { get; }
    IQueryHandler<GetForexTradeDashboardQuery, Result<TradeTerminalDashboardDto>> GetTradeDashboard { get; }
    IQueryHandler<GetForexRiskAssessmentQuery, Result<ForexRiskOverviewDto>> GetRiskAssessment { get; }
    IQueryHandler<GetForexJournalQuery, Result<(IReadOnlyList<ForexJournalEntryDto> Entries, TaxReportSummaryDto TaxSummary)>> GetJournal { get; }
    IQueryHandler<GetForexSettingsQuery, Result<ForexApiSettingsDto>> GetSettings { get; }
}

public sealed class ForexQueryUnit : IForexQueryUnit
{
    public IQueryHandler<GetForexMarketOverviewQuery, Result<MarketOverviewDto>> GetMarketOverview { get; }
    public IQueryHandler<GetForexCandlesQuery, Result<IReadOnlyList<ForexCandleDto>>> GetCandles { get; }
    public IQueryHandler<GetForexAnalysisQuery, Result<(TechnicalOverviewDto Technical, FundamentalOverviewDto Fundamental)>> GetAnalysis { get; }
    public IQueryHandler<GetForexStrategiesQuery, Result<(IReadOnlyList<ForexStrategyDto> Strategies, IReadOnlyList<BacktestResultDto> RecentBacktests)>> GetStrategies { get; }
    public IQueryHandler<GetBacktestDetailsQuery, Result<BacktestResultDto>> GetBacktestDetails { get; }
    public IQueryHandler<GetForexTradeDashboardQuery, Result<TradeTerminalDashboardDto>> GetTradeDashboard { get; }
    public IQueryHandler<GetForexRiskAssessmentQuery, Result<ForexRiskOverviewDto>> GetRiskAssessment { get; }
    public IQueryHandler<GetForexJournalQuery, Result<(IReadOnlyList<ForexJournalEntryDto> Entries, TaxReportSummaryDto TaxSummary)>> GetJournal { get; }
    public IQueryHandler<GetForexSettingsQuery, Result<ForexApiSettingsDto>> GetSettings { get; }

    public ForexQueryUnit(
        IQueryHandler<GetForexMarketOverviewQuery, Result<MarketOverviewDto>> getMarketOverview,
        IQueryHandler<GetForexCandlesQuery, Result<IReadOnlyList<ForexCandleDto>>> getCandles,
        IQueryHandler<GetForexAnalysisQuery, Result<(TechnicalOverviewDto Technical, FundamentalOverviewDto Fundamental)>> getAnalysis,
        IQueryHandler<GetForexStrategiesQuery, Result<(IReadOnlyList<ForexStrategyDto> Strategies, IReadOnlyList<BacktestResultDto> RecentBacktests)>> getStrategies,
        IQueryHandler<GetBacktestDetailsQuery, Result<BacktestResultDto>> getBacktestDetails,
        IQueryHandler<GetForexTradeDashboardQuery, Result<TradeTerminalDashboardDto>> getTradeDashboard,
        IQueryHandler<GetForexRiskAssessmentQuery, Result<ForexRiskOverviewDto>> getRiskAssessment,
        IQueryHandler<GetForexJournalQuery, Result<(IReadOnlyList<ForexJournalEntryDto> Entries, TaxReportSummaryDto TaxSummary)>> getJournal,
        IQueryHandler<GetForexSettingsQuery, Result<ForexApiSettingsDto>> getSettings)
    {
        GetMarketOverview = getMarketOverview;
        GetCandles = getCandles;
        GetAnalysis = getAnalysis;
        GetStrategies = getStrategies;
        GetBacktestDetails = getBacktestDetails;
        GetTradeDashboard = getTradeDashboard;
        GetRiskAssessment = getRiskAssessment;
        GetJournal = getJournal;
        GetSettings = getSettings;
    }
}
