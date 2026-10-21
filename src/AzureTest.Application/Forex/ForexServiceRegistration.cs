using System;
using System.Collections.Generic;
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
using AzureTest.Application.Forex.Services;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Forex;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace AzureTest.Application.Forex;

public static class ForexServiceRegistration
{
    public static IServiceCollection AddForexModule(this IServiceCollection services)
    {
        // Core Forex Services
        services.AddHttpClient<IForexMarketDataService, ForexMarketDataService>();
        services.AddScoped<IForexAnalysisService, ForexAnalysisService>();
        services.AddScoped<IForexStrategyService, ForexStrategyService>();
        services.AddScoped<IForexRiskService, ForexRiskService>();
        services.AddScoped<IForexExecutionService, ForexExecutionService>();

        // Command Handlers
        services.AddScoped<ICommandHandler<PlaceOrderCommand, Result<Guid>>, PlaceOrderCommandHandler>();
        services.AddScoped<ICommandHandler<CancelOrderCommand, Result>, CancelOrderCommandHandler>();
        services.AddScoped<ICommandHandler<CloseTradeCommand, Result<decimal>>, CloseTradeCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateTradeStopsCommand, Result>, UpdateTradeStopsCommandHandler>();
        services.AddScoped<ICommandHandler<CreateBacktestCommand, Result<BacktestResultDto>>, CreateBacktestCommandHandler>();
        services.AddScoped<ICommandHandler<SaveJournalEntryCommand, Result<Guid>>, SaveJournalEntryCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateApiSettingsCommand, Result>, UpdateApiSettingsCommandHandler>();
        services.AddScoped<ICommandHandler<ResetDemoAccountCommand, Result>, ResetDemoAccountCommandHandler>();

        // Command Validators
        services.AddScoped<IValidator<PlaceOrderCommand>, PlaceOrderCommandValidator>();
        services.AddScoped<IValidator<CancelOrderCommand>, CancelOrderCommandValidator>();
        services.AddScoped<IValidator<CloseTradeCommand>, CloseTradeCommandValidator>();
        services.AddScoped<IValidator<UpdateTradeStopsCommand>, UpdateTradeStopsCommandValidator>();
        services.AddScoped<IValidator<CreateBacktestCommand>, CreateBacktestCommandValidator>();
        services.AddScoped<IValidator<SaveJournalEntryCommand>, SaveJournalEntryCommandValidator>();
        services.AddScoped<IValidator<UpdateApiSettingsCommand>, UpdateApiSettingsCommandValidator>();
        services.AddScoped<IValidator<ResetDemoAccountCommand>, ResetDemoAccountCommandValidator>();

        // Query Handlers
        services.AddScoped<IQueryHandler<GetForexMarketOverviewQuery, Result<MarketOverviewDto>>, GetForexMarketOverviewQueryHandler>();
        services.AddScoped<IQueryHandler<GetForexCandlesQuery, Result<IReadOnlyList<ForexCandleDto>>>, GetForexCandlesQueryHandler>();
        services.AddScoped<IQueryHandler<GetForexAnalysisQuery, Result<(TechnicalOverviewDto Technical, FundamentalOverviewDto Fundamental)>>, GetForexAnalysisQueryHandler>();
        services.AddScoped<IQueryHandler<GetForexStrategiesQuery, Result<(IReadOnlyList<ForexStrategyDto> Strategies, IReadOnlyList<BacktestResultDto> RecentBacktests)>>, GetForexStrategiesQueryHandler>();
        services.AddScoped<IQueryHandler<GetBacktestDetailsQuery, Result<BacktestResultDto>>, GetBacktestDetailsQueryHandler>();
        services.AddScoped<IQueryHandler<GetForexTradeDashboardQuery, Result<TradeTerminalDashboardDto>>, GetForexTradeDashboardQueryHandler>();
        services.AddScoped<IQueryHandler<GetForexRiskAssessmentQuery, Result<ForexRiskOverviewDto>>, GetForexRiskAssessmentQueryHandler>();
        services.AddScoped<IQueryHandler<GetForexJournalQuery, Result<(IReadOnlyList<ForexJournalEntryDto> Entries, TaxReportSummaryDto TaxSummary)>>, GetForexJournalQueryHandler>();
        services.AddScoped<IQueryHandler<GetForexSettingsQuery, Result<ForexApiSettingsDto>>, GetForexSettingsQueryHandler>();

        // Query Validators
        services.AddScoped<IValidator<GetForexMarketOverviewQuery>, GetForexMarketOverviewQueryValidator>();
        services.AddScoped<IValidator<GetForexCandlesQuery>, GetForexCandlesQueryValidator>();
        services.AddScoped<IValidator<GetForexAnalysisQuery>, GetForexAnalysisQueryValidator>();
        services.AddScoped<IValidator<GetForexStrategiesQuery>, GetForexStrategiesQueryValidator>();
        services.AddScoped<IValidator<GetBacktestDetailsQuery>, GetBacktestDetailsQueryValidator>();
        services.AddScoped<IValidator<GetForexTradeDashboardQuery>, GetForexTradeDashboardQueryValidator>();
        services.AddScoped<IValidator<GetForexRiskAssessmentQuery>, GetForexRiskAssessmentQueryValidator>();
        services.AddScoped<IValidator<GetForexJournalQuery>, GetForexJournalQueryValidator>();
        services.AddScoped<IValidator<GetForexSettingsQuery>, GetForexSettingsQueryValidator>();

        // Units
        services.AddScoped<IForexCommandUnit, ForexCommandUnit>();
        services.AddScoped<IForexQueryUnit, ForexQueryUnit>();

        return services;
    }
}
