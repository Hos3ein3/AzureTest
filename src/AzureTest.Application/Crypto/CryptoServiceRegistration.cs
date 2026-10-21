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
using AzureTest.Application.Crypto.Queries.GetCryptoDexScreener;
using AzureTest.Application.Crypto.Queries.GetCryptoExchangeFlow;
using AzureTest.Application.Crypto.Queries.GetCryptoFuturesFunding;
using AzureTest.Application.Crypto.Queries.GetCryptoMarketDataHub;
using AzureTest.Application.Crypto.Queries.GetCryptoMarketOverview;
using AzureTest.Application.Crypto.Queries.GetCryptoOnChainHub;
using AzureTest.Application.Crypto.Queries.GetCryptoOrderBook;
using AzureTest.Application.Crypto.Queries.GetCryptoTokenUnlocks;
using AzureTest.Application.Crypto.Queries.GetCryptoPortfolio;
using AzureTest.Application.Crypto.Queries.GetCryptoRiskOverview;
using AzureTest.Application.Crypto.Queries.GetCryptoScreener;
using AzureTest.Application.Crypto.Queries.GetCryptoSettings;
using AzureTest.Application.Crypto.Queries.GetCryptoStrategies;
using AzureTest.Application.Crypto.Queries.GetCryptoTaxReport;
using AzureTest.Application.Crypto.Queries.GetCryptoTradeTerminal;
using AzureTest.Application.Crypto.Services;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Crypto;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace AzureTest.Application.Crypto;

public static class CryptoServiceRegistration
{
    public static IServiceCollection AddCryptoModule(this IServiceCollection services)
    {
        // Core Services
        services.AddHttpClient<ICryptoMarketDataService, CryptoMarketDataService>();
        services.AddScoped<ICryptoAnalysisService, CryptoAnalysisService>();
        services.AddScoped<ICryptoStrategyService, CryptoStrategyService>();
        services.AddScoped<ICryptoRiskService, CryptoRiskService>();
        services.AddScoped<ICryptoExecutionService, CryptoExecutionService>();

        // Command Handlers
        services.AddScoped<ICommandHandler<PlaceCryptoOrderCommand, Result<Guid>>, PlaceCryptoOrderCommandHandler>();
        services.AddScoped<ICommandHandler<CancelCryptoOrderCommand, Result>, CancelCryptoOrderCommandHandler>();
        services.AddScoped<ICommandHandler<CloseCryptoPositionCommand, Result<decimal>>, CloseCryptoPositionCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateCryptoStopsCommand, Result>, UpdateCryptoStopsCommandHandler>();
        services.AddScoped<ICommandHandler<TriggerKillSwitchCommand, Result<int>>, TriggerKillSwitchCommandHandler>();
        services.AddScoped<ICommandHandler<ResetCryptoAccountCommand, Result>, ResetCryptoAccountCommandHandler>();
        services.AddScoped<ICommandHandler<RunCryptoBacktestCommand, Result<BacktestResultDto>>, RunCryptoBacktestCommandHandler>();
        services.AddScoped<ICommandHandler<CreateCryptoAlertCommand, Result<Guid>>, CreateCryptoAlertCommandHandler>();
        services.AddScoped<ICommandHandler<DeleteCryptoAlertCommand, Result>, DeleteCryptoAlertCommandHandler>();
        services.AddScoped<ICommandHandler<SaveCryptoJournalCommand, Result<Guid>>, SaveCryptoJournalCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateCryptoSettingsCommand, Result>, UpdateCryptoSettingsCommandHandler>();

        // Command Validators
        services.AddScoped<IValidator<PlaceCryptoOrderCommand>, PlaceCryptoOrderCommandValidator>();
        services.AddScoped<IValidator<CancelCryptoOrderCommand>, CancelCryptoOrderCommandValidator>();
        services.AddScoped<IValidator<CloseCryptoPositionCommand>, CloseCryptoPositionCommandValidator>();
        services.AddScoped<IValidator<UpdateCryptoStopsCommand>, UpdateCryptoStopsCommandValidator>();
        services.AddScoped<IValidator<TriggerKillSwitchCommand>, TriggerKillSwitchCommandValidator>();
        services.AddScoped<IValidator<ResetCryptoAccountCommand>, ResetCryptoAccountCommandValidator>();
        services.AddScoped<IValidator<RunCryptoBacktestCommand>, RunCryptoBacktestCommandValidator>();
        services.AddScoped<IValidator<CreateCryptoAlertCommand>, CreateCryptoAlertCommandValidator>();
        services.AddScoped<IValidator<DeleteCryptoAlertCommand>, DeleteCryptoAlertCommandValidator>();
        services.AddScoped<IValidator<SaveCryptoJournalCommand>, SaveCryptoJournalCommandValidator>();
        services.AddScoped<IValidator<UpdateCryptoSettingsCommand>, UpdateCryptoSettingsCommandValidator>();

        // Query Handlers
        services.AddScoped<IQueryHandler<GetCryptoMarketOverviewQuery, Result<CryptoMarketOverviewDto>>, GetCryptoMarketOverviewQueryHandler>();
        services.AddScoped<IQueryHandler<GetCryptoCandlesQuery, Result<IReadOnlyList<CryptoCandleDto>>>, GetCryptoCandlesQueryHandler>();
        services.AddScoped<IQueryHandler<GetCryptoOrderBookQuery, Result<CryptoOrderBookDto>>, GetCryptoOrderBookQueryHandler>();
        services.AddScoped<IQueryHandler<GetCryptoFuturesFundingQuery, Result<CryptoFuturesFundingDto>>, GetCryptoFuturesFundingQueryHandler>();
        services.AddScoped<IQueryHandler<GetCryptoAnalysisQuery, Result<CryptoAnalysisDto>>, GetCryptoAnalysisQueryHandler>();
        services.AddScoped<IQueryHandler<GetCryptoScreenerQuery, Result<IReadOnlyList<CryptoScreenerItemDto>>>, GetCryptoScreenerQueryHandler>();
        services.AddScoped<IQueryHandler<GetCryptoStrategiesQuery, Result<(IReadOnlyList<CryptoStrategyDto> Strategies, IReadOnlyList<BacktestResultDto> RecentBacktests)>>, GetCryptoStrategiesQueryHandler>();
        services.AddScoped<IQueryHandler<GetCryptoTradeTerminalQuery, Result<TradeTerminalDashboardDto>>, GetCryptoTradeTerminalQueryHandler>();
        services.AddScoped<IQueryHandler<GetCryptoRiskOverviewQuery, Result<CryptoRiskOverviewDto>>, GetCryptoRiskOverviewQueryHandler>();
        services.AddScoped<IQueryHandler<GetCryptoPortfolioQuery, Result<(CryptoAccountDto Account, IReadOnlyList<CryptoPositionDto> OpenPositions, IReadOnlyList<CryptoPositionDto> ClosedTrades, IReadOnlyList<CryptoJournalEntryDto> JournalEntries)>>, GetCryptoPortfolioQueryHandler>();
        services.AddScoped<IQueryHandler<GetCryptoTaxReportQuery, Result<CryptoTaxReportDto>>, GetCryptoTaxReportQueryHandler>();
        services.AddScoped<IQueryHandler<GetCryptoAlertsQuery, Result<IReadOnlyList<CryptoAlertDto>>>, GetCryptoAlertsQueryHandler>();
        services.AddScoped<IQueryHandler<GetCryptoSettingsQuery, Result<IReadOnlyList<CryptoApiSettingsDto>>>, GetCryptoSettingsQueryHandler>();
        services.AddScoped<IQueryHandler<GetCryptoMarketDataHubQuery, Result<CryptoMarketDataHubDto>>, GetCryptoMarketDataHubQueryHandler>();
        services.AddScoped<IQueryHandler<GetCryptoDerivativesHubQuery, Result<CryptoDerivativesHubDto>>, GetCryptoDerivativesHubQueryHandler>();
        services.AddScoped<IQueryHandler<GetCryptoOnChainHubQuery, Result<CryptoOnChainHubDto>>, GetCryptoOnChainHubQueryHandler>();
        services.AddScoped<IQueryHandler<GetCryptoExchangeFlowQuery, Result<CryptoExchangeFlowAnalysisHubDto>>, GetCryptoExchangeFlowQueryHandler>();
        services.AddScoped<IQueryHandler<GetCryptoTokenUnlocksQuery, Result<CryptoTokenUnlocksHubDto>>, GetCryptoTokenUnlocksQueryHandler>();
        services.AddScoped<IQueryHandler<GetCryptoDexScreenerQuery, Result<CryptoDexScreenerHubDto>>, GetCryptoDexScreenerQueryHandler>();

        // Query Validators
        services.AddScoped<IValidator<GetCryptoMarketOverviewQuery>, GetCryptoMarketOverviewQueryValidator>();
        services.AddScoped<IValidator<GetCryptoCandlesQuery>, GetCryptoCandlesQueryValidator>();
        services.AddScoped<IValidator<GetCryptoOrderBookQuery>, GetCryptoOrderBookQueryValidator>();
        services.AddScoped<IValidator<GetCryptoFuturesFundingQuery>, GetCryptoFuturesFundingQueryValidator>();
        services.AddScoped<IValidator<GetCryptoAnalysisQuery>, GetCryptoAnalysisQueryValidator>();
        services.AddScoped<IValidator<GetCryptoScreenerQuery>, GetCryptoScreenerQueryValidator>();
        services.AddScoped<IValidator<GetCryptoStrategiesQuery>, GetCryptoStrategiesQueryValidator>();
        services.AddScoped<IValidator<GetCryptoTradeTerminalQuery>, GetCryptoTradeTerminalQueryValidator>();
        services.AddScoped<IValidator<GetCryptoRiskOverviewQuery>, GetCryptoRiskOverviewQueryValidator>();
        services.AddScoped<IValidator<GetCryptoPortfolioQuery>, GetCryptoPortfolioQueryValidator>();
        services.AddScoped<IValidator<GetCryptoTaxReportQuery>, GetCryptoTaxReportQueryValidator>();
        services.AddScoped<IValidator<GetCryptoAlertsQuery>, GetCryptoAlertsQueryValidator>();
        services.AddScoped<IValidator<GetCryptoSettingsQuery>, GetCryptoSettingsQueryValidator>();
        services.AddScoped<IValidator<GetCryptoMarketDataHubQuery>, GetCryptoMarketDataHubQueryValidator>();
        services.AddScoped<IValidator<GetCryptoDerivativesHubQuery>, GetCryptoDerivativesHubQueryValidator>();
        services.AddScoped<IValidator<GetCryptoOnChainHubQuery>, GetCryptoOnChainHubQueryValidator>();
        services.AddScoped<IValidator<GetCryptoExchangeFlowQuery>, GetCryptoExchangeFlowQueryValidator>();
        services.AddScoped<IValidator<GetCryptoTokenUnlocksQuery>, GetCryptoTokenUnlocksQueryValidator>();
        services.AddScoped<IValidator<GetCryptoDexScreenerQuery>, GetCryptoDexScreenerQueryValidator>();

        // Units
        services.AddScoped<ICryptoCommandUnit, CryptoCommandUnit>();
        services.AddScoped<ICryptoQueryUnit, CryptoQueryUnit>();

        return services;
    }
}
