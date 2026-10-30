using System;
using Asp.Versioning;
using Asp.Versioning.Builder;
using AzureTest.API.Feature.Auth;
using AzureTest.API.Feature.Crypto;
using AzureTest.API.Feature.Forex;
using AzureTest.API.Feature.Notification;
using AzureTest.API.Feature.Profile;
using AzureTest.API.Feature.Trading;
using AzureTest.API.Feature.UserPreference;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace AzureTest.API.Feature;

public static class ApiRoutes
{
    public static class Auth
    {
        public const string Login = "auth/login";
        public const string Register = "auth/register";
        public const string Logout = "auth/logout";
        public const string RefreshToken = "auth/refresh-token";
        public const string ForgotPassword = "auth/forgot-password";
        public const string ResetPassword = "auth/reset-password";
        public const string ConfirmEmail = "auth/confirm-email";
        public const string ChangePassword = "auth/change-password";
        public const string Google = "auth/google";
    }

    public static class Trading
    {
        public const string Dashboard = "trading/dashboard";
        public const string Terminal = "trading/terminal";
        public const string Analytics = "trading/analytics";
        public const string History = "trading/history";
        public const string Quote = "trading/quote";
        public const string Quotes = "trading/quotes";
        public const string LiveStatus = "trading/live-status";
        public const string Search = "trading/search";
        public const string PlaceOrder = "trading/order";
        public const string ClosePosition = "trading/position/close";
        public const string ModifyPosition = "trading/position/modify";
        public const string CancelOrder = "trading/order/cancel";
        public const string ResetBalance = "trading/account/reset";
        public const string WatchlistToggle = "trading/watchlist/toggle";
        public const string ExecuteJson = "trading/execute";
        public const string LeverageBracket = "trading/leverage-bracket";
    }

    public static class Forex
    {
        public const string Settings = "forex/settings";
        public const string MarketOverview = "forex/market";
        public const string Chart = "forex/market/chart";
        public const string Sessions = "forex/market/sessions";
        public const string Strategies = "forex/strategies";
        public const string BacktestExecute = "forex/strategies/backtest";
        public const string BacktestResults = "forex/strategies/backtest/results/{id:guid}";
        public const string Analysis = "forex/analysis";
        public const string Technical = "forex/analysis/technical";
        public const string Strength = "forex/analysis/strength";
        public const string Fundamental = "forex/analysis/fundamental";
        public const string Journal = "forex/portfolio/journal";
        public const string SaveJournalEntry = "forex/portfolio/journal";
        public const string ExportTax = "forex/portfolio/tax/export";
        public const string RiskOverview = "forex/risk";
        public const string CalculateLotSize = "forex/risk/calculate-lot-size";
        public const string TradeDashboard = "forex/trade";
        public const string PlaceOrder = "forex/trade/order";
        public const string CloseTrade = "forex/trade/close";
        public const string CancelOrder = "forex/trade/cancel";
        public const string UpdateStops = "forex/trade/stops";
        public const string PartialClose = "forex/trade/partial-close";
        public const string MoveBreakEven = "forex/trade/break-even";
        public const string KillSwitch = "forex/trade/kill-switch";
        public const string ResetAccount = "forex/trade/reset-account";
        public const string Webhook = "forex/trade/webhook";
    }

    public static class Crypto
    {
        public const string MarketOverview = "crypto/market";
        public const string Chart = "crypto/market/chart";
        public const string OrderBook = "crypto/market/orderbook";
        public const string Futures = "crypto/market/futures";
        public const string OrderFlow = "crypto/market/orderflow";
        public const string OnChain = "crypto/market/onchain";
        public const string ExchangeFlow = "crypto/market/exchangeflow";
        public const string TokenUnlocks = "crypto/market/tokenunlocks";
        public const string DexScreener = "crypto/market/dexscreener";

        public const string Alerts = "crypto/alerts";
        public const string CreateAlert = "crypto/alerts";
        public const string DeleteAlert = "crypto/alerts/{alertId:guid}";
        public const string SendTestAlert = "crypto/alerts/{alertId:guid}/test";

        public const string Settings = "crypto/settings";
        public const string UpdateSettings = "crypto/settings";
        public const string TestConnection = "crypto/settings/test-connection";

        public const string TradeTerminal = "crypto/trade";
        public const string PlaceOrder = "crypto/trade/order";
        public const string ClosePosition = "crypto/trade/position/close";
        public const string CancelOrder = "crypto/trade/order/cancel";
        public const string UpdateStops = "crypto/trade/stops";
        public const string PlaceSmartOrder = "crypto/trade/smart-order";
        public const string Arbitrage = "crypto/trade/arbitrage";
        public const string ExecuteArbitrage = "crypto/trade/arbitrage/execute";

        public const string Strategies = "crypto/strategies";
        public const string BacktestSimulation = "crypto/strategies/simulation";
        public const string ExecuteBacktest = "crypto/strategies/backtest";

        public const string Portfolio = "crypto/portfolio";
        public const string Journal = "crypto/portfolio/journal";
        public const string SaveJournal = "crypto/portfolio/journal";
        public const string TaxReport = "crypto/portfolio/tax";
        public const string ExportTaxCsv = "crypto/portfolio/tax/export";
        public const string ResetAccount = "crypto/portfolio/account/reset";

        public const string RiskDashboard = "crypto/risk";
        public const string KillSwitch = "crypto/risk/kill-switch";
        public const string UpdateLimits = "crypto/risk/limits";
        public const string TopUpMargin = "crypto/risk/margin/top-up";
        public const string TrimPosition = "crypto/risk/position/trim";
        public const string CalculateAtr = "crypto/risk/atr";

        public const string AnalysisTechnical = "crypto/analysis/technical";
        public const string MarketStructure = "crypto/analysis/market-structure";
        public const string VolumeProfile = "crypto/analysis/volume-profile";
        public const string MarketRegime = "crypto/analysis/market-regime";
        public const string Sentiment = "crypto/analysis/sentiment";
        public const string Screener = "crypto/analysis/screener";
        public const string OnChainAnalysis = "crypto/analysis/onchain";
    }

    public static class Notification
    {
        public const string UserNotifications = "notifications/user";
        public const string MarkRead = "notifications/mark-read";
        public const string SubscribePush = "notifications/subscribe-push";
        public const string VapidPublicKey = "notifications/vapid-public-key";
        public const string SendTest = "notifications/send-test";
        public const string Delete = "notifications/delete";
        public const string DeleteByDateRange = "notifications/delete-by-date-range";
    }

    public static class Profile
    {
        public const string Get = "profile";
        public const string Update = "profile";
    }

    public static class UserPreference
    {
        public const string Get = "user-preference";
        public const string Update = "user-preference";
    }

    public static class OutboxAdmin
    {
        public const string Outbox = "outbox-admin/outbox";
        public const string Inbox = "outbox-admin/inbox";
        public const string Payload = "outbox-admin/outbox/{id:guid}/payload";
        public const string Retry = "outbox-admin/outbox/{id:guid}/retry";
        public const string Delete = "outbox-admin/outbox/delete";
        public const string DeleteByDateRange = "outbox-admin/outbox/delete-by-date-range";
        public const string DeleteInbox = "outbox-admin/inbox/delete";
        public const string DeleteInboxByDateRange = "outbox-admin/inbox/delete-by-date-range";
    }

    public static class AuditLog
    {
        public const string Logs = "audit-log";
        public const string Delete = "audit-log/{id:guid}";
        public const string DeleteByUser = "audit-log/user/{userId:guid}";
        public const string DeleteByDateRange = "audit-log/delete-by-date-range";
        public const string PurgeAll = "audit-log/purge";
    }

    private static RouteHandlerBuilder WithApiDoc(
        this RouteHandlerBuilder builder,
        string groupName,
        string displayName,
        string description)
    {
        return builder
            .WithGroupName(groupName)
            .WithTags(groupName)
            .WithDisplayName(displayName)
            .WithSummary(displayName)
            .WithDescription(description);
    }

    public static IEndpointRouteBuilder MapApiRoutes(this IEndpointRouteBuilder app)
    {
        var apiVersionSet = app.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1, 0))
            .ReportApiVersions()
            .Build();

        var apiGroup = app.MapGroup("api/v{version:apiVersion}")
            .WithApiVersionSet(apiVersionSet);

        // ── Auth Endpoints ───────────────────────────────────────────────────────────
        app.MapPost("/api/auth/google", GoogleAuth.HandleDirectAsync)
            .AllowAnonymous()
            .WithApiDoc(nameof(Auth), "Google Authentication Direct", "Validate Google IdToken, link or create user, and issue JWT tokens.");

        apiGroup.MapPost(Auth.Login, Login.HandleAsync)
            .AllowAnonymous()
            .WithApiDoc(nameof(Auth), "User Login", "Authenticate user credentials and return JWT access and refresh tokens.");

        apiGroup.MapPost(Auth.Register, Register.HandleAsync)
            .AllowAnonymous()
            .WithApiDoc(nameof(Auth), "User Registration", "Register a new user account.");

        apiGroup.MapPost(Auth.Google, GoogleAuth.HandleAsync)
            .AllowAnonymous()
            .WithApiDoc(nameof(Auth), "Google Authentication", "Sign in or register using Google credentials and receive JWT access tokens.");

        apiGroup.MapPost(Auth.RefreshToken, RefreshToken.HandleAsync)
            .AllowAnonymous()
            .WithApiDoc(nameof(Auth), "Refresh JWT Token", "Exchange a valid refresh token for a new JWT access and refresh token pair.");

        apiGroup.MapPost(Auth.ForgotPassword, ForgotPassword.HandleAsync)
            .AllowAnonymous()
            .WithApiDoc(nameof(Auth), "Forgot Password", "Request a password reset link sent via email.");

        apiGroup.MapPost(Auth.ResetPassword, ResetPassword.HandleAsync)
            .AllowAnonymous()
            .WithApiDoc(nameof(Auth), "Reset Password", "Reset user password with email verification token.");

        apiGroup.MapGet(Auth.ConfirmEmail, ConfirmEmail.HandleAsync)
            .AllowAnonymous()
            .WithApiDoc(nameof(Auth), "Confirm Email", "Confirm user account email address.");

        apiGroup.MapPost(Auth.Logout, Logout.HandleAsync)
            .RequireAuthorization()
            .WithApiDoc(nameof(Auth), "User Logout", "Revoke active user session and refresh tokens.");

        apiGroup.MapPost(Auth.ChangePassword, ChangePassword.HandleAsync)
            .RequireAuthorization()
            .WithApiDoc(nameof(Auth), "Change Password", "Change authenticated user password.");

        // ── Trading Endpoints ────────────────────────────────────────────────────────
        var trading = apiGroup.MapGroup("").RequireAuthorization();

        trading.MapGet(Trading.Dashboard, GetTradingDashboard.HandleAsync)
            .WithApiDoc(nameof(Trading), "Get Trading Dashboard", "Retrieve summary metrics including balance, open positions, PnL, and recent orders.");

        trading.MapGet(Trading.Terminal, GetTradingTerminal.HandleAsync)
            .WithApiDoc(nameof(Trading), "Get Trading Terminal", "Retrieve trading terminal view and market data.");

        trading.MapGet(Trading.Analytics, GetTradingAnalytics.HandleAsync)
            .WithApiDoc(nameof(Trading), "Get Trading Analytics", "Retrieve performance analytics, win/loss ratio, and drawdown charts.");

        trading.MapGet(Trading.History, GetTradingHistory.HandleAsync)
            .WithApiDoc(nameof(Trading), "Get Trading History", "Retrieve historical order and execution logs with optional filtering.");

        trading.MapGet(Trading.Quote, GetTradingQuote.HandleAsync)
            .WithApiDoc(nameof(Trading), "Get Trading Quote", "Retrieve current market quote and live price for a specific asset symbol.");

        trading.MapGet(Trading.Quotes, GetTradingQuotes.HandleAsync)
            .WithApiDoc(nameof(Trading), "Get All Trading Quotes", "Retrieve market quotes for all active instruments.");

        trading.MapGet(Trading.LiveStatus, GetTradingLiveStatus.HandleAsync)
            .WithApiDoc(nameof(Trading), "Get Trading Live Status", "Retrieve live connection state and real-time market data feed health.");

        trading.MapGet(Trading.Search, SearchTradingAssets.HandleAsync)
            .WithApiDoc(nameof(Trading), "Search Trading Assets", "Search available traded assets and instruments by query string.");

        trading.MapGet(Trading.LeverageBracket, GetTradingLeverageBracket.HandleAsync)
            .WithApiDoc(nameof(Trading), "Get Trading Leverage Bracket", "Retrieve dynamic leverage brackets and maximum allowed leverage for a specific instrument from Binance or fallback.");

        trading.MapPost(Trading.PlaceOrder, PlaceTradingOrder.HandleAsync)
            .WithApiDoc(nameof(Trading), "Place Trading Order", "Submit a new market or limit trading order for execution.");

        trading.MapPost(Trading.ClosePosition, CloseTradingPosition.HandleAsync)
            .WithApiDoc(nameof(Trading), "Close Trading Position", "Close an open trading position at current market price.");

        trading.MapPost(Trading.ModifyPosition, ModifyTradingPosition.HandleAsync)
            .WithApiDoc(nameof(Trading), "Modify Trading Position", "Modify take profit, stop loss, or trailing stop levels on an open position.");

        trading.MapPost(Trading.CancelOrder, CancelTradingOrder.HandleAsync)
            .WithApiDoc(nameof(Trading), "Cancel Trading Order", "Cancel a pending trading order before execution.");

        trading.MapPost(Trading.ResetBalance, ResetTradingBalance.HandleAsync)
            .WithApiDoc(nameof(Trading), "Reset Trading Balance", "Reset paper trading virtual account balance to initial default.");

        trading.MapPost(Trading.WatchlistToggle, ToggleTradingWatchlist.HandleAsync)
            .WithApiDoc(nameof(Trading), "Toggle Watchlist Asset", "Add or remove an asset symbol from user's active trading watchlist.");

        trading.MapPost(Trading.ExecuteJson, ExecuteTradingJson.HandleAsync)
            .WithApiDoc(nameof(Trading), "Execute Trading JSON Command", "Execute dynamic trading JSON commands and batch instructions.");

        // ── Forex Endpoints ──────────────────────────────────────────────────────────
        var forex = apiGroup.MapGroup("").RequireAuthorization();

        forex.MapGet(Forex.Settings, GetForexSettings.HandleAsync)
            .WithApiDoc(nameof(Forex), "Get Forex Settings", "Retrieve current Forex account settings and trading environment preferences.");

        forex.MapPost(Forex.Settings, UpdateForexSettings.HandleAsync)
            .WithApiDoc(nameof(Forex), "Update Forex Settings", "Update Forex account settings, leverage rules, and risk thresholds.");

        forex.MapGet(Forex.MarketOverview, GetForexMarketOverview.HandleAsync)
            .WithApiDoc(nameof(Forex), "Get Forex Market Overview", "Retrieve Forex market overview, currency pair heatmaps, and price changes.");

        forex.MapGet(Forex.Chart, GetForexChart.HandleAsync)
            .WithApiDoc(nameof(Forex), "Get Forex Candle Chart", "Retrieve historical OHLC candlestick data for specified currency pair and timeframe.");

        forex.MapGet(Forex.Sessions, GetForexSessions.HandleAsync)
            .WithApiDoc(nameof(Forex), "Get Forex Market Sessions", "Retrieve status, overlapping hours, and volume volatility for global Forex trading sessions.");

        forex.MapGet(Forex.Strategies, GetForexStrategies.HandleAsync)
            .WithApiDoc(nameof(Forex), "Get Forex Strategies", "Retrieve list of configured Forex trading algorithms and strategy definitions.");

        forex.MapPost(Forex.BacktestExecute, ExecuteForexBacktest.HandleAsync)
            .WithApiDoc(nameof(Forex), "Execute Forex Backtest", "Run historical backtesting simulation for a Forex trading strategy.");

        forex.MapGet(Forex.BacktestResults, GetForexBacktestResults.HandleAsync)
            .WithApiDoc(nameof(Forex), "Get Forex Backtest Results", "Retrieve detailed backtest metrics, equity curve, trade log, and profit factor by ID.");

        forex.MapGet(Forex.Analysis, GetForexAnalysis.HandleAsync)
            .WithApiDoc(nameof(Forex), "Get Forex Analysis", "Retrieve aggregated multi-timeframe technical and trend analysis for a currency pair.");

        forex.MapGet(Forex.Technical, GetForexTechnical.HandleAsync)
            .WithApiDoc(nameof(Forex), "Get Forex Technical Indicators", "Retrieve RSI, MACD, Moving Averages, and ATR technical indicators for a Forex pair.");

        forex.MapGet(Forex.Strength, GetForexStrength.HandleAsync)
            .WithApiDoc(nameof(Forex), "Get Currency Strength Matrix", "Retrieve real-time relative currency strength matrix across major global currencies.");

        forex.MapGet(Forex.Fundamental, GetForexFundamental.HandleAsync)
            .WithApiDoc(nameof(Forex), "Get Forex Fundamental Data", "Retrieve economic calendar releases, central bank interest rates, and macro data.");

        forex.MapGet(Forex.Journal, GetForexJournal.HandleAsync)
            .WithApiDoc(nameof(Forex), "Get Forex Trading Journal", "Retrieve personal Forex trade journal entries, notes, and tags.");

        forex.MapPost(Forex.SaveJournalEntry, SaveForexJournalEntry.HandleAsync)
            .WithApiDoc(nameof(Forex), "Save Forex Journal Entry", "Create or update a Forex trading journal entry.");

        forex.MapGet(Forex.ExportTax, ExportForexTax.HandleAsync)
            .WithApiDoc(nameof(Forex), "Export Forex Tax Report", "Generate and export annual Forex trading tax and PnL report.");

        forex.MapGet(Forex.RiskOverview, GetForexRiskOverview.HandleAsync)
            .WithApiDoc(nameof(Forex), "Get Forex Risk Overview", "Retrieve current account exposure, drawdown limits, and margin utilization.");

        forex.MapPost(Forex.CalculateLotSize, CalculateForexLotSize.Handle)
            .WithApiDoc(nameof(Forex), "Calculate Forex Lot Size", "Calculate recommended position lot size based on account balance, risk percent, and stop loss pips.");

        forex.MapGet(Forex.TradeDashboard, GetForexTradeDashboard.HandleAsync)
            .WithApiDoc(nameof(Forex), "Get Forex Trade Dashboard", "Retrieve active Forex trade management dashboard and open positions.");

        forex.MapPost(Forex.PlaceOrder, PlaceForexOrder.HandleAsync)
            .WithApiDoc(nameof(Forex), "Place Forex Order", "Submit a new Forex trade order.");

        forex.MapPost(Forex.CloseTrade, CloseForexTrade.HandleAsync)
            .WithApiDoc(nameof(Forex), "Close Forex Trade", "Close an active Forex trade position.");

        forex.MapPost(Forex.CancelOrder, CancelForexOrder.HandleAsync)
            .WithApiDoc(nameof(Forex), "Cancel Forex Order", "Cancel a pending Forex limit or stop order.");

        forex.MapPost(Forex.UpdateStops, UpdateForexStops.HandleAsync)
            .WithApiDoc(nameof(Forex), "Update Forex Stop Loss / Take Profit", "Update stop-loss and take-profit price levels on an open Forex trade.");

        forex.MapPost(Forex.PartialClose, PartialCloseForexTrade.HandleAsync)
            .WithApiDoc(nameof(Forex), "Partial Close Forex Trade", "Close a partial volume of an active Forex trade position.");

        forex.MapPost(Forex.MoveBreakEven, MoveForexBreakEven.HandleAsync)
            .WithApiDoc(nameof(Forex), "Move Forex Trade to Break-Even", "Adjust stop-loss level to the trade entry price to eliminate downside risk.");

        forex.MapPost(Forex.KillSwitch, ActivateForexKillSwitch.HandleAsync)
            .WithApiDoc(nameof(Forex), "Activate Forex Kill-Switch", "Immediately emergency close all open Forex positions and cancel pending orders.");

        forex.MapPost(Forex.ResetAccount, ResetForexAccount.HandleAsync)
            .WithApiDoc(nameof(Forex), "Reset Forex Account", "Reset Forex practice trading account balance and trade history.");

        apiGroup.MapPost(Forex.Webhook, ProcessForexWebhook.HandleAsync)
            .AllowAnonymous()
            .WithApiDoc(nameof(Forex), "Process Forex Webhook", "Process external TradingView or webhook signals for automated trade execution.");

        // ── Crypto Endpoints ─────────────────────────────────────────────────────────
        var crypto = apiGroup.MapGroup("").RequireAuthorization();

        crypto.MapGet(Crypto.MarketOverview, GetCryptoMarketOverview.HandleAsync)
            .WithApiDoc(nameof(Crypto), "Get Crypto Market Overview", "Retrieve cryptocurrency market overview, top gainers, losers, and 24h market cap.");

        crypto.MapGet(Crypto.Chart, GetCryptoChart.HandleAsync)
            .WithApiDoc(nameof(Crypto), "Get Crypto Candle Chart", "Retrieve candlestick OHLC data for a crypto symbol and timeframe.");

        crypto.MapGet(Crypto.OrderBook, GetCryptoOrderBook.HandleAsync)
            .WithApiDoc(nameof(Crypto), "Get Crypto Order Book", "Retrieve Level 2 real-time order book bids and asks for a crypto pair.");

        crypto.MapGet(Crypto.Futures, GetCryptoFutures.HandleAsync)
            .WithApiDoc(nameof(Crypto), "Get Crypto Futures & Derivatives Hub", "Retrieve funding rates, open interest, and futures market overview.");

        crypto.MapGet(Crypto.OrderFlow, GetCryptoOrderFlow.HandleAsync)
            .WithApiDoc(nameof(Crypto), "Get Crypto Order Flow & Delta", "Retrieve order flow analysis, volume delta, and liquidity depth.");

        crypto.MapGet(Crypto.OnChain, GetCryptoOnChain.HandleAsync)
            .WithApiDoc(nameof(Crypto), "Get Crypto On-Chain Metrics", "Retrieve on-chain transaction volume, whale transfers, and active wallet metrics.");

        crypto.MapGet(Crypto.ExchangeFlow, GetCryptoExchangeFlow.HandleAsync)
            .WithApiDoc(nameof(Crypto), "Get Crypto Exchange Flows", "Retrieve net exchange inflows and outflows for major cryptocurrencies.");

        crypto.MapGet(Crypto.TokenUnlocks, GetCryptoTokenUnlocks.HandleAsync)
            .WithApiDoc(nameof(Crypto), "Get Crypto Token Unlocks", "Retrieve upcoming vesting schedules and token unlock events.");

        crypto.MapGet(Crypto.DexScreener, GetCryptoDexScreener.HandleAsync)
            .WithApiDoc(nameof(Crypto), "Get DEX Screener Trends", "Retrieve trending DEX liquidity pools, new pairs, and price spikes.");

        crypto.MapGet(Crypto.Alerts, GetCryptoAlerts.HandleAsync)
            .WithApiDoc(nameof(Crypto), "Get Crypto Price Alerts", "Retrieve user-configured price and volume alerts for cryptocurrencies.");

        crypto.MapPost(Crypto.CreateAlert, CreateCryptoAlert.HandleAsync)
            .WithApiDoc(nameof(Crypto), "Create Crypto Price Alert", "Create a new price or percentage change alert trigger.");

        crypto.MapDelete(Crypto.DeleteAlert, DeleteCryptoAlert.HandleAsync)
            .WithApiDoc(nameof(Crypto), "Delete Crypto Price Alert", "Delete an existing price alert by ID.");

        crypto.MapPost(Crypto.SendTestAlert, SendTestCryptoAlert.Handle)
            .WithApiDoc(nameof(Crypto), "Send Test Crypto Alert", "Send a test notification for a configured crypto alert.");

        crypto.MapGet(Crypto.Settings, GetCryptoSettings.HandleAsync)
            .WithApiDoc(nameof(Crypto), "Get Crypto Settings", "Retrieve crypto trading API keys and platform preferences.");

        crypto.MapPost(Crypto.UpdateSettings, UpdateCryptoSettings.HandleAsync)
            .WithApiDoc(nameof(Crypto), "Update Crypto Settings", "Update crypto exchange API credentials and settings.");

        crypto.MapPost(Crypto.TestConnection, TestCryptoConnection.Handle)
            .WithApiDoc(nameof(Crypto), "Test Crypto API Connection", "Verify connectivity and credentials with external crypto exchanges.");

        crypto.MapGet(Crypto.TradeTerminal, GetCryptoTradeTerminal.HandleAsync)
            .WithApiDoc(nameof(Crypto), "Get Crypto Trade Terminal", "Retrieve crypto trading terminal data and active order states.");

        crypto.MapPost(Crypto.PlaceOrder, PlaceCryptoOrder.HandleAsync)
            .WithApiDoc(nameof(Crypto), "Place Crypto Order", "Submit a new spot or futures crypto trade order.");

        crypto.MapPost(Crypto.ClosePosition, CloseCryptoPosition.HandleAsync)
            .WithApiDoc(nameof(Crypto), "Close Crypto Position", "Close an active crypto position at market price.");

        crypto.MapPost(Crypto.CancelOrder, CancelCryptoOrder.HandleAsync)
            .WithApiDoc(nameof(Crypto), "Cancel Crypto Order", "Cancel a pending crypto order.");

        crypto.MapPost(Crypto.UpdateStops, UpdateCryptoStops.HandleAsync)
            .WithApiDoc(nameof(Crypto), "Update Crypto Stop Loss / Take Profit", "Update SL/TP trigger prices on an open crypto position.");

        crypto.MapPost(Crypto.PlaceSmartOrder, PlaceCryptoSmartOrder.HandleAsync)
            .WithApiDoc(nameof(Crypto), "Place Crypto Smart Order", "Place advanced smart order with trailing stops and scaled targets.");

        crypto.MapGet(Crypto.Arbitrage, GetCryptoArbitrage.HandleAsync)
            .WithApiDoc(nameof(Crypto), "Get Crypto Arbitrage Opportunities", "Scan and identify cross-exchange spot and triangular arbitrage opportunities.");

        crypto.MapPost(Crypto.ExecuteArbitrage, ExecuteCryptoArbitrage.Handle)
            .WithApiDoc(nameof(Crypto), "Execute Crypto Arbitrage", "Execute automated cross-exchange arbitrage trade.");

        crypto.MapGet(Crypto.Strategies, GetCryptoStrategies.HandleAsync)
            .WithApiDoc(nameof(Crypto), "Get Crypto Strategies", "Retrieve configured automated crypto trading strategies.");

        crypto.MapGet(Crypto.BacktestSimulation, GetCryptoBacktestSimulation.Handle)
            .WithApiDoc(nameof(Crypto), "Get Crypto Backtest Simulation", "Inspect crypto strategy simulation configurations and models.");

        crypto.MapPost(Crypto.ExecuteBacktest, ExecuteCryptoBacktest.HandleAsync)
            .WithApiDoc(nameof(Crypto), "Execute Crypto Backtest", "Run historical backtest simulation on crypto pairs.");

        crypto.MapGet(Crypto.Portfolio, GetCryptoPortfolio.HandleAsync)
            .WithApiDoc(nameof(Crypto), "Get Crypto Portfolio", "Retrieve comprehensive crypto portfolio holdings, valuation, and allocation breakdown.");

        crypto.MapGet(Crypto.Journal, GetCryptoJournal.HandleAsync)
            .WithApiDoc(nameof(Crypto), "Get Crypto Trade Journal", "Retrieve crypto trading journal entries and notes.");

        crypto.MapPost(Crypto.SaveJournal, SaveCryptoJournal.HandleAsync)
            .WithApiDoc(nameof(Crypto), "Save Crypto Trade Journal Entry", "Create or update a crypto trade journal record.");

        crypto.MapGet(Crypto.TaxReport, GetCryptoTaxReport.HandleAsync)
            .WithApiDoc(nameof(Crypto), "Get Crypto Tax Report", "Retrieve realized capital gains and crypto tax calculation summary.");

        crypto.MapGet(Crypto.ExportTaxCsv, ExportCryptoTaxCsv.HandleAsync)
            .WithApiDoc(nameof(Crypto), "Export Crypto Tax CSV", "Export crypto tax report as a downloadable CSV file.");

        crypto.MapPost(Crypto.ResetAccount, ResetCryptoAccount.HandleAsync)
            .WithApiDoc(nameof(Crypto), "Reset Crypto Account", "Reset virtual crypto trading balance and history.");

        crypto.MapGet(Crypto.RiskDashboard, GetCryptoRiskDashboard.HandleAsync)
            .WithApiDoc(nameof(Crypto), "Get Crypto Risk Dashboard", "Retrieve crypto portfolio Value-at-Risk (VaR), liquidation distances, and leverage metrics.");

        crypto.MapPost(Crypto.KillSwitch, ActivateCryptoKillSwitch.HandleAsync)
            .WithApiDoc(nameof(Crypto), "Activate Crypto Kill-Switch", "Emergency close all crypto positions and cancel all pending orders.");

        crypto.MapPost(Crypto.UpdateLimits, UpdateCryptoRiskLimits.HandleAsync)
            .WithApiDoc(nameof(Crypto), "Update Crypto Risk Limits", "Configure maximum leverage and drawdown risk thresholds.");

        crypto.MapPost(Crypto.TopUpMargin, TopUpCryptoMargin.HandleAsync)
            .WithApiDoc(nameof(Crypto), "Top-Up Crypto Margin", "Allocate additional margin collateral to an open position.");

        crypto.MapPost(Crypto.TrimPosition, TrimCryptoPosition.HandleAsync)
            .WithApiDoc(nameof(Crypto), "Trim Crypto Position", "Partially reduce size of an open crypto position.");

        crypto.MapPost(Crypto.CalculateAtr, CalculateCryptoAtr.Handle)
            .WithApiDoc(nameof(Crypto), "Calculate Crypto ATR", "Calculate Average True Range (ATR) volatility for dynamic stop sizing.");

        crypto.MapGet(Crypto.AnalysisTechnical, GetCryptoAnalysisTechnical.HandleAsync)
            .WithApiDoc(nameof(Crypto), "Get Crypto Technical Analysis", "Retrieve comprehensive technical indicators for a crypto asset.");

        crypto.MapGet(Crypto.MarketStructure, GetCryptoMarketStructure.HandleAsync)
            .WithApiDoc(nameof(Crypto), "Get Crypto Market Structure", "Identify support/resistance levels, order blocks, and market structure shifts.");

        crypto.MapGet(Crypto.VolumeProfile, GetCryptoVolumeProfile.HandleAsync)
            .WithApiDoc(nameof(Crypto), "Get Crypto Volume Profile", "Retrieve Volume Profile and Point of Control (POC) for specified asset.");

        crypto.MapGet(Crypto.MarketRegime, GetCryptoMarketRegime.HandleAsync)
            .WithApiDoc(nameof(Crypto), "Get Crypto Market Regime", "Detect current market regime (trending, ranging, volatile, consolidating).");

        crypto.MapGet(Crypto.Sentiment, GetCryptoSentiment.HandleAsync)
            .WithApiDoc(nameof(Crypto), "Get Crypto Market Sentiment", "Retrieve Fear & Greed index, social sentiment, and funding rate bias.");

        crypto.MapGet(Crypto.Screener, GetCryptoScreener.HandleAsync)
            .WithApiDoc(nameof(Crypto), "Get Crypto Screener", "Screen cryptocurrencies based on RSI, volume, price momentum, and trend filters.");

        crypto.MapGet(Crypto.OnChainAnalysis, GetCryptoOnChainAnalysis.HandleAsync)
            .WithApiDoc(nameof(Crypto), "Get Deep On-Chain Analysis", "Retrieve in-depth blockchain analytics and network health indicators.");

        // ── Notification Endpoints ───────────────────────────────────────────────────
        var notifications = apiGroup.MapGroup("").RequireAuthorization();

        notifications.MapGet(Notification.UserNotifications, Feature.Notification.GetUserNotifications.HandleAsync)
            .WithApiDoc(nameof(Notification), "Get User Notifications", "Retrieve list of recent notifications for the authenticated user.");

        notifications.MapPost(Notification.MarkRead, Feature.Notification.MarkNotificationAsRead.HandleAsync)
            .WithApiDoc(nameof(Notification), "Mark Notification As Read", "Mark a single notification or all notifications as read for current user.");

        notifications.MapPost(Notification.SubscribePush, Feature.Notification.SubscribeWebPush.HandleAsync)
            .WithApiDoc(nameof(Notification), "Subscribe Web Push", "Register a browser web push notification subscription.");

        apiGroup.MapGet(Notification.VapidPublicKey, Feature.Notification.GetVapidPublicKey.Handle)
            .AllowAnonymous()
            .WithApiDoc(nameof(Notification), "Get VAPID Public Key", "Retrieve the public VAPID key used for web push client registration.");

        notifications.MapPost(Notification.SendTest, Feature.Notification.SendTestNotification.HandleAsync)
            .WithApiDoc(nameof(Notification), "Send Test Notification", "Dispatch a test notification to verify delivery.");

        notifications.MapPost(Notification.Delete, Feature.Notification.DeleteNotification.HandleAsync)
            .WithApiDoc(nameof(Notification), "Delete Notification", "Delete a specific notification from user's history.");

        notifications.MapPost(Notification.DeleteByDateRange, Feature.Notification.DeleteNotificationsByDateRange.HandleAsync)
            .WithApiDoc(nameof(Notification), "Delete Notifications By Date Range", "Bulk delete user notifications within a specified date range.");

        // ── Profile Endpoints ────────────────────────────────────────────────────────
        var profile = apiGroup.MapGroup("").RequireAuthorization();

        profile.MapGet(Profile.Get, Feature.Profile.GetProfile.HandleAsync)
            .WithApiDoc(nameof(Profile), "Get User Profile", "Retrieve current authenticated user's profile details.");

        profile.MapPost(Profile.Update, Feature.Profile.UpdateProfile.HandleAsync)
            .DisableAntiforgery()
            .WithApiDoc(nameof(Profile), "Update User Profile", "Update user profile details and upload new avatar photo.");

        // ── User Preference Endpoints ────────────────────────────────────────────────
        var userPreference = apiGroup.MapGroup("").RequireAuthorization();

        userPreference.MapGet(UserPreference.Get, Feature.UserPreference.GetUserPreference.HandleAsync)
            .WithApiDoc(nameof(UserPreference), "Get User Preferences", "Retrieve user UI preferences (theme, language, currency, dashboard).");

        userPreference.MapPost(UserPreference.Update, Feature.UserPreference.UpdateUserPreference.HandleAsync)
            .WithApiDoc(nameof(UserPreference), "Update User Preferences", "Update user preferences (theme, dark mode, language, currency, dashboard).");

        // ── Outbox Admin Endpoints ───────────────────────────────────────────────────
        var outboxAdmin = apiGroup.MapGroup("").RequireAuthorization();

        outboxAdmin.MapGet(OutboxAdmin.Outbox, Feature.OutboxAdmin.GetOutboxMessages.HandleAsync)
            .WithApiDoc("OutboxAdmin", "Get Outbox Messages", "Retrieve and filter transactional Outbox event messages.");

        outboxAdmin.MapGet(OutboxAdmin.Inbox, Feature.OutboxAdmin.GetInboxMessages.HandleAsync)
            .WithApiDoc("OutboxAdmin", "Get Inbox Messages", "Retrieve and filter deduplicated Inbox event messages.");

        outboxAdmin.MapGet(OutboxAdmin.Payload, Feature.OutboxAdmin.GetOutboxPayload.HandleAsync)
            .WithApiDoc("OutboxAdmin", "Get Outbox Payload", "Retrieve the JSON payload for a specific Outbox message.");

        outboxAdmin.MapPost(OutboxAdmin.Retry, Feature.OutboxAdmin.RetryOutboxMessage.HandleAsync)
            .WithApiDoc("OutboxAdmin", "Retry Outbox Message", "Reset an Outbox message error and retry count for re-dispatch.");

        outboxAdmin.MapPost(OutboxAdmin.Delete, Feature.OutboxAdmin.DeleteOutboxMessage.HandleAsync)
            .WithApiDoc("OutboxAdmin", "Delete Outbox Message", "Delete a specific transactional Outbox message.");

        outboxAdmin.MapPost(OutboxAdmin.DeleteByDateRange, Feature.OutboxAdmin.DeleteOutboxMessagesByDateRange.HandleAsync)
            .WithApiDoc("OutboxAdmin", "Delete Outbox Messages By Date Range", "Bulk delete Outbox messages within a specified date range.");

        outboxAdmin.MapPost(OutboxAdmin.DeleteInbox, Feature.OutboxAdmin.DeleteInboxMessage.HandleAsync)
            .WithApiDoc("OutboxAdmin", "Delete Inbox Message", "Delete a specific deduplicated Inbox message.");

        outboxAdmin.MapPost(OutboxAdmin.DeleteInboxByDateRange, Feature.OutboxAdmin.DeleteInboxMessagesByDateRange.HandleAsync)
            .WithApiDoc("OutboxAdmin", "Delete Inbox Messages By Date Range", "Bulk delete Inbox messages within a specified date range.");

        // ── Audit Log Endpoints ──────────────────────────────────────────────────────
        var auditLog = apiGroup.MapGroup("").RequireAuthorization();

        auditLog.MapGet(AuditLog.Logs, Feature.AuditLog.GetAuditLogs.HandleAsync)
            .WithApiDoc("AuditLog", "Get Audit Logs", "Retrieve and filter immutable audit trail records by user and tenant.");

        auditLog.MapDelete(AuditLog.Delete, Feature.AuditLog.DeleteAuditLog.HandleAsync)
            .WithApiDoc("AuditLog", "Delete Audit Log", "Delete a specific audit log entry (Admin only).");

        auditLog.MapDelete(AuditLog.DeleteByUser, Feature.AuditLog.DeleteAuditLogsByUser.HandleAsync)
            .WithApiDoc("AuditLog", "Delete Audit Logs By User", "Delete all audit logs for a specific user (Admin only).");

        auditLog.MapPost(AuditLog.DeleteByDateRange, Feature.AuditLog.DeleteAuditLogsByDateRange.HandleAsync)
            .WithApiDoc("AuditLog", "Delete Audit Logs By Date Range", "Delete audit logs within a date range (Admin only).");

        auditLog.MapPost(AuditLog.PurgeAll, Feature.AuditLog.PurgeAuditLogs.HandleAsync)
            .WithApiDoc("AuditLog", "Purge All Audit Logs", "Permanently purge all audit log records (Admin only).");

        return app;
    }
}
