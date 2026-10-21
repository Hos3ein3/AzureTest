using System;
using System.Collections.Generic;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Crypto.Commands.CancelCryptoOrder;
using AzureTest.Application.Crypto.Commands.CloseCryptoPosition;
using AzureTest.Application.Crypto.Commands.CreateCryptoAlert;
using AzureTest.Application.Crypto.Commands.DeleteCryptoAlert;
using AzureTest.Application.Crypto.Commands.PlaceCryptoOrder;
using AzureTest.Application.Crypto.Commands.ResetCryptoAccount;
using AzureTest.Application.Crypto.Commands.RunCryptoBacktest;
using AzureTest.Application.Crypto.Commands.SaveCryptoJournal;
using AzureTest.Application.Crypto.Commands.TriggerKillSwitch;
using AzureTest.Application.Crypto.Commands.UpdateCryptoSettings;
using AzureTest.Application.Crypto.Commands.UpdateCryptoStops;
using AzureTest.Application.Crypto.Queries.GetCryptoAlerts;
using AzureTest.Application.Crypto.Queries.GetCryptoAnalysis;
using AzureTest.Application.Crypto.Queries.GetCryptoCandles;
using AzureTest.Application.Crypto.Queries.GetCryptoDerivativesHub;
using AzureTest.Application.Crypto.Queries.GetCryptoFuturesFunding;
using AzureTest.Application.Crypto.Queries.GetCryptoMarketDataHub;
using AzureTest.Application.Crypto.Queries.GetCryptoMarketOverview;
using AzureTest.Application.Crypto.Queries.GetCryptoOrderBook;
using AzureTest.Application.Crypto.Queries.GetCryptoPortfolio;
using AzureTest.Application.Crypto.Queries.GetCryptoRiskOverview;
using AzureTest.Application.Crypto.Queries.GetCryptoScreener;
using AzureTest.Application.Crypto.Queries.GetCryptoSettings;
using AzureTest.Application.Crypto.Queries.GetCryptoStrategies;
using AzureTest.Application.Crypto.Queries.GetCryptoTaxReport;
using AzureTest.Application.Crypto.Queries.GetCryptoTradeTerminal;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Crypto;

namespace AzureTest.Application.Crypto;

public interface ICryptoCommandUnit
{
    ICommandHandler<PlaceCryptoOrderCommand, Result<Guid>> PlaceOrder { get; }
    ICommandHandler<CancelCryptoOrderCommand, Result> CancelOrder { get; }
    ICommandHandler<CloseCryptoPositionCommand, Result<decimal>> ClosePosition { get; }
    ICommandHandler<UpdateCryptoStopsCommand, Result> UpdateStops { get; }
    ICommandHandler<TriggerKillSwitchCommand, Result<int>> TriggerKillSwitch { get; }
    ICommandHandler<ResetCryptoAccountCommand, Result> ResetAccount { get; }
    ICommandHandler<RunCryptoBacktestCommand, Result<BacktestResultDto>> RunBacktest { get; }
    ICommandHandler<CreateCryptoAlertCommand, Result<Guid>> CreateAlert { get; }
    ICommandHandler<DeleteCryptoAlertCommand, Result> DeleteAlert { get; }
    ICommandHandler<SaveCryptoJournalCommand, Result<Guid>> SaveJournal { get; }
    ICommandHandler<UpdateCryptoSettingsCommand, Result> UpdateSettings { get; }
}

public sealed class CryptoCommandUnit : ICryptoCommandUnit
{
    public ICommandHandler<PlaceCryptoOrderCommand, Result<Guid>> PlaceOrder { get; }
    public ICommandHandler<CancelCryptoOrderCommand, Result> CancelOrder { get; }
    public ICommandHandler<CloseCryptoPositionCommand, Result<decimal>> ClosePosition { get; }
    public ICommandHandler<UpdateCryptoStopsCommand, Result> UpdateStops { get; }
    public ICommandHandler<TriggerKillSwitchCommand, Result<int>> TriggerKillSwitch { get; }
    public ICommandHandler<ResetCryptoAccountCommand, Result> ResetAccount { get; }
    public ICommandHandler<RunCryptoBacktestCommand, Result<BacktestResultDto>> RunBacktest { get; }
    public ICommandHandler<CreateCryptoAlertCommand, Result<Guid>> CreateAlert { get; }
    public ICommandHandler<DeleteCryptoAlertCommand, Result> DeleteAlert { get; }
    public ICommandHandler<SaveCryptoJournalCommand, Result<Guid>> SaveJournal { get; }
    public ICommandHandler<UpdateCryptoSettingsCommand, Result> UpdateSettings { get; }

    public CryptoCommandUnit(
        ICommandHandler<PlaceCryptoOrderCommand, Result<Guid>> placeOrder,
        ICommandHandler<CancelCryptoOrderCommand, Result> cancelOrder,
        ICommandHandler<CloseCryptoPositionCommand, Result<decimal>> closePosition,
        ICommandHandler<UpdateCryptoStopsCommand, Result> updateStops,
        ICommandHandler<TriggerKillSwitchCommand, Result<int>> triggerKillSwitch,
        ICommandHandler<ResetCryptoAccountCommand, Result> resetAccount,
        ICommandHandler<RunCryptoBacktestCommand, Result<BacktestResultDto>> runBacktest,
        ICommandHandler<CreateCryptoAlertCommand, Result<Guid>> createAlert,
        ICommandHandler<DeleteCryptoAlertCommand, Result> deleteAlert,
        ICommandHandler<SaveCryptoJournalCommand, Result<Guid>> saveJournal,
        ICommandHandler<UpdateCryptoSettingsCommand, Result> updateSettings)
    {
        PlaceOrder = placeOrder;
        CancelOrder = cancelOrder;
        ClosePosition = closePosition;
        UpdateStops = updateStops;
        TriggerKillSwitch = triggerKillSwitch;
        ResetAccount = resetAccount;
        RunBacktest = runBacktest;
        CreateAlert = createAlert;
        DeleteAlert = deleteAlert;
        SaveJournal = saveJournal;
        UpdateSettings = updateSettings;
    }
}

public interface ICryptoQueryUnit
{
    IQueryHandler<GetCryptoMarketOverviewQuery, Result<CryptoMarketOverviewDto>> GetMarketOverview { get; }
    IQueryHandler<GetCryptoCandlesQuery, Result<IReadOnlyList<CryptoCandleDto>>> GetCandles { get; }
    IQueryHandler<GetCryptoOrderBookQuery, Result<CryptoOrderBookDto>> GetOrderBook { get; }
    IQueryHandler<GetCryptoFuturesFundingQuery, Result<CryptoFuturesFundingDto>> GetFuturesFunding { get; }
    IQueryHandler<GetCryptoAnalysisQuery, Result<CryptoAnalysisDto>> GetAnalysis { get; }
    IQueryHandler<GetCryptoScreenerQuery, Result<IReadOnlyList<CryptoScreenerItemDto>>> GetScreener { get; }
    IQueryHandler<GetCryptoStrategiesQuery, Result<(IReadOnlyList<CryptoStrategyDto> Strategies, IReadOnlyList<BacktestResultDto> RecentBacktests)>> GetStrategies { get; }
    IQueryHandler<GetCryptoTradeTerminalQuery, Result<TradeTerminalDashboardDto>> GetTradeTerminal { get; }
    IQueryHandler<GetCryptoRiskOverviewQuery, Result<CryptoRiskOverviewDto>> GetRiskOverview { get; }
    IQueryHandler<GetCryptoPortfolioQuery, Result<(CryptoAccountDto Account, IReadOnlyList<CryptoPositionDto> OpenPositions, IReadOnlyList<CryptoPositionDto> ClosedTrades, IReadOnlyList<CryptoJournalEntryDto> JournalEntries)>> GetPortfolio { get; }
    IQueryHandler<GetCryptoTaxReportQuery, Result<CryptoTaxReportDto>> GetTaxReport { get; }
    IQueryHandler<GetCryptoAlertsQuery, Result<IReadOnlyList<CryptoAlertDto>>> GetAlerts { get; }
    IQueryHandler<GetCryptoSettingsQuery, Result<IReadOnlyList<CryptoApiSettingsDto>>> GetSettings { get; }
    IQueryHandler<GetCryptoMarketDataHubQuery, Result<CryptoMarketDataHubDto>> GetMarketDataHub { get; }
    IQueryHandler<GetCryptoDerivativesHubQuery, Result<CryptoDerivativesHubDto>> GetDerivativesHub { get; }
}

public sealed class CryptoQueryUnit : ICryptoQueryUnit
{
    public IQueryHandler<GetCryptoMarketOverviewQuery, Result<CryptoMarketOverviewDto>> GetMarketOverview { get; }
    public IQueryHandler<GetCryptoCandlesQuery, Result<IReadOnlyList<CryptoCandleDto>>> GetCandles { get; }
    public IQueryHandler<GetCryptoOrderBookQuery, Result<CryptoOrderBookDto>> GetOrderBook { get; }
    public IQueryHandler<GetCryptoFuturesFundingQuery, Result<CryptoFuturesFundingDto>> GetFuturesFunding { get; }
    public IQueryHandler<GetCryptoAnalysisQuery, Result<CryptoAnalysisDto>> GetAnalysis { get; }
    public IQueryHandler<GetCryptoScreenerQuery, Result<IReadOnlyList<CryptoScreenerItemDto>>> GetScreener { get; }
    public IQueryHandler<GetCryptoStrategiesQuery, Result<(IReadOnlyList<CryptoStrategyDto> Strategies, IReadOnlyList<BacktestResultDto> RecentBacktests)>> GetStrategies { get; }
    public IQueryHandler<GetCryptoTradeTerminalQuery, Result<TradeTerminalDashboardDto>> GetTradeTerminal { get; }
    public IQueryHandler<GetCryptoRiskOverviewQuery, Result<CryptoRiskOverviewDto>> GetRiskOverview { get; }
    public IQueryHandler<GetCryptoPortfolioQuery, Result<(CryptoAccountDto Account, IReadOnlyList<CryptoPositionDto> OpenPositions, IReadOnlyList<CryptoPositionDto> ClosedTrades, IReadOnlyList<CryptoJournalEntryDto> JournalEntries)>> GetPortfolio { get; }
    public IQueryHandler<GetCryptoTaxReportQuery, Result<CryptoTaxReportDto>> GetTaxReport { get; }
    public IQueryHandler<GetCryptoAlertsQuery, Result<IReadOnlyList<CryptoAlertDto>>> GetAlerts { get; }
    public IQueryHandler<GetCryptoSettingsQuery, Result<IReadOnlyList<CryptoApiSettingsDto>>> GetSettings { get; }
    public IQueryHandler<GetCryptoMarketDataHubQuery, Result<CryptoMarketDataHubDto>> GetMarketDataHub { get; }
    public IQueryHandler<GetCryptoDerivativesHubQuery, Result<CryptoDerivativesHubDto>> GetDerivativesHub { get; }

    public CryptoQueryUnit(
        IQueryHandler<GetCryptoMarketOverviewQuery, Result<CryptoMarketOverviewDto>> getMarketOverview,
        IQueryHandler<GetCryptoCandlesQuery, Result<IReadOnlyList<CryptoCandleDto>>> getCandles,
        IQueryHandler<GetCryptoOrderBookQuery, Result<CryptoOrderBookDto>> getOrderBook,
        IQueryHandler<GetCryptoFuturesFundingQuery, Result<CryptoFuturesFundingDto>> getFuturesFunding,
        IQueryHandler<GetCryptoAnalysisQuery, Result<CryptoAnalysisDto>> getAnalysis,
        IQueryHandler<GetCryptoScreenerQuery, Result<IReadOnlyList<CryptoScreenerItemDto>>> getScreener,
        IQueryHandler<GetCryptoStrategiesQuery, Result<(IReadOnlyList<CryptoStrategyDto> Strategies, IReadOnlyList<BacktestResultDto> RecentBacktests)>> getStrategies,
        IQueryHandler<GetCryptoTradeTerminalQuery, Result<TradeTerminalDashboardDto>> getTradeTerminal,
        IQueryHandler<GetCryptoRiskOverviewQuery, Result<CryptoRiskOverviewDto>> getRiskOverview,
        IQueryHandler<GetCryptoPortfolioQuery, Result<(CryptoAccountDto Account, IReadOnlyList<CryptoPositionDto> OpenPositions, IReadOnlyList<CryptoPositionDto> ClosedTrades, IReadOnlyList<CryptoJournalEntryDto> JournalEntries)>> getPortfolio,
        IQueryHandler<GetCryptoTaxReportQuery, Result<CryptoTaxReportDto>> getTaxReport,
        IQueryHandler<GetCryptoAlertsQuery, Result<IReadOnlyList<CryptoAlertDto>>> getAlerts,
        IQueryHandler<GetCryptoSettingsQuery, Result<IReadOnlyList<CryptoApiSettingsDto>>> getSettings,
        IQueryHandler<GetCryptoMarketDataHubQuery, Result<CryptoMarketDataHubDto>> getMarketDataHub,
        IQueryHandler<GetCryptoDerivativesHubQuery, Result<CryptoDerivativesHubDto>> getDerivativesHub)
    {
        GetMarketOverview = getMarketOverview;
        GetCandles = getCandles;
        GetOrderBook = getOrderBook;
        GetFuturesFunding = getFuturesFunding;
        GetAnalysis = getAnalysis;
        GetScreener = getScreener;
        GetStrategies = getStrategies;
        GetTradeTerminal = getTradeTerminal;
        GetRiskOverview = getRiskOverview;
        GetPortfolio = getPortfolio;
        GetTaxReport = getTaxReport;
        GetAlerts = getAlerts;
        GetSettings = getSettings;
        GetMarketDataHub = getMarketDataHub;
        GetDerivativesHub = getDerivativesHub;
    }
}

