using System;
using System.Collections.Generic;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Messaging;
using AzureTest.Application.Trading.Commands.CancelTradingOrder;
using AzureTest.Application.Trading.Commands.CloseTradingPosition;
using AzureTest.Application.Trading.Commands.ExecuteTradingJsonBatch;
using AzureTest.Application.Trading.Commands.PlaceTradingOrder;
using AzureTest.Application.Trading.Commands.ResetTradingAccountBalance;
using AzureTest.Application.Trading.Commands.SyncTradingPrices;
using AzureTest.Application.Trading.Commands.ToggleTradingWatchlist;
using AzureTest.Application.Trading.Queries.GetTradingDashboard;
using AzureTest.Application.Trading.Queries.GetTradingQuotes;
using AzureTest.Application.Trading.Services;
using AzureTest.Contracts.DTOs.Trading;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace AzureTest.Application.Trading;

public static class TradingServiceRegistration
{
    public static IServiceCollection AddTradingModule(this IServiceCollection services)
    {
        // Core Market Data Service
        services.AddSingleton<ITradingMarketDataService, TradingMarketDataService>();

        // Command Handlers
        services.AddScoped<ICommandHandler<PlaceTradingOrderCommand, Result<Guid>>, PlaceTradingOrderCommandHandler>();
        services.AddScoped<ICommandHandler<CloseTradingPositionCommand, Result<Guid>>, CloseTradingPositionCommandHandler>();
        services.AddScoped<ICommandHandler<CancelTradingOrderCommand, Result<Guid>>, CancelTradingOrderCommandHandler>();
        services.AddScoped<ICommandHandler<ResetTradingAccountBalanceCommand, Result<Guid>>, ResetTradingAccountBalanceCommandHandler>();
        services.AddScoped<ICommandHandler<ToggleTradingWatchlistCommand, Result<bool>>, ToggleTradingWatchlistCommandHandler>();
        services.AddScoped<ICommandHandler<ExecuteTradingJsonBatchCommand, Result<IReadOnlyList<TradingActionResultDto>>>, ExecuteTradingJsonBatchCommandHandler>();
        services.AddScoped<ICommandHandler<SyncTradingPricesCommand, Result<int>>, SyncTradingPricesCommandHandler>();

        // Command Validators
        services.AddScoped<IValidator<PlaceTradingOrderCommand>, PlaceTradingOrderCommandValidator>();
        services.AddScoped<IValidator<CloseTradingPositionCommand>, CloseTradingPositionCommandValidator>();
        services.AddScoped<IValidator<CancelTradingOrderCommand>, CancelTradingOrderCommandValidator>();
        services.AddScoped<IValidator<ResetTradingAccountBalanceCommand>, ResetTradingAccountBalanceCommandValidator>();
        services.AddScoped<IValidator<ToggleTradingWatchlistCommand>, ToggleTradingWatchlistCommandValidator>();
        services.AddScoped<IValidator<ExecuteTradingJsonBatchCommand>, ExecuteTradingJsonBatchCommandValidator>();
        services.AddScoped<IValidator<SyncTradingPricesCommand>, SyncTradingPricesCommandValidator>();

        // Query Handlers
        services.AddScoped<IQueryHandler<GetTradingDashboardQuery, Result<TradingDashboardDto>>, GetTradingDashboardQueryHandler>();
        services.AddScoped<IQueryHandler<GetTradingQuotesQuery, Result<IReadOnlyList<TradingQuoteDto>>>, GetTradingQuotesQueryHandler>();

        // Units
        services.AddScoped<ITradingCommandUnit, TradingCommandUnit>();
        services.AddScoped<ITradingQueryUnit, TradingQueryUnit>();

        return services;
    }
}
