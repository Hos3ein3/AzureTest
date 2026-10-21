using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AzureTest.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class forexAndCrypto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CryptoAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    BaseCurrency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    TradingMode = table.Column<int>(type: "integer", nullable: false),
                    CashBalance = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    Equity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    MarginUsed = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    DailyStartingEquity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    CurrentDailyLoss = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    DailyLossLimitPercent = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    RiskPercentagePerTrade = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    MaxOpenPositions = table.Column<int>(type: "integer", nullable: false),
                    IsKillSwitchActive = table.Column<bool>(type: "boolean", nullable: false),
                    KillSwitchReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    KillSwitchTriggeredAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CryptoAccounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CryptoAlerts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Symbol = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    AlertType = table.Column<int>(type: "integer", nullable: false),
                    TargetValue = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: false),
                    Message = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Channel = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsTriggered = table.Column<bool>(type: "boolean", nullable: false),
                    TriggeredAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CryptoAlerts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CryptoApiSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Exchange = table.Column<int>(type: "integer", nullable: false),
                    ApiKeyMasked = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EncryptedApiKey = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    EncryptedApiSecret = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    EncryptedPassphrase = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsTestnet = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsConnected = table.Column<bool>(type: "boolean", nullable: false),
                    LastTestedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ConnectionStatusMessage = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CryptoApiSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CryptoBacktests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StrategyId = table.Column<Guid>(type: "uuid", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    StrategyName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    StrategyType = table.Column<int>(type: "integer", nullable: false),
                    Symbol = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Timeframe = table.Column<int>(type: "integer", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    InitialCapital = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    FinalCapital = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    TotalTrades = table.Column<int>(type: "integer", nullable: false),
                    WinningTrades = table.Column<int>(type: "integer", nullable: false),
                    LosingTrades = table.Column<int>(type: "integer", nullable: false),
                    WinRate = table.Column<decimal>(type: "numeric(8,4)", precision: 8, scale: 4, nullable: false),
                    ProfitFactor = table.Column<decimal>(type: "numeric(8,4)", precision: 8, scale: 4, nullable: false),
                    MaxDrawdownPercent = table.Column<decimal>(type: "numeric(8,4)", precision: 8, scale: 4, nullable: false),
                    SharpeRatio = table.Column<decimal>(type: "numeric(8,4)", precision: 8, scale: 4, nullable: false),
                    CommissionFeePercent = table.Column<decimal>(type: "numeric(8,4)", precision: 8, scale: 4, nullable: false),
                    SlippagePercent = table.Column<decimal>(type: "numeric(8,4)", precision: 8, scale: 4, nullable: false),
                    TotalFeesPaid = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    EquityCurveJson = table.Column<string>(type: "character varying(50000)", maxLength: 50000, nullable: false),
                    WalkForwardSummaryJson = table.Column<string>(type: "character varying(50000)", maxLength: 50000, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CryptoBacktests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CryptoJournalEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PositionId = table.Column<Guid>(type: "uuid", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Symbol = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    StrategyTag = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EmotionTag = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Rating = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    LessonsLearned = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CryptoJournalEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CryptoOrders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Symbol = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Side = table.Column<int>(type: "integer", nullable: false),
                    OrderType = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: false),
                    FilledQuantity = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: false),
                    RequestedPrice = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: false),
                    StopPrice = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: true),
                    FillPrice = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: true),
                    Commission = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: false),
                    Slippage = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    TradingMode = table.Column<int>(type: "integer", nullable: false),
                    IdempotencyKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ClientOrderId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RejectionReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FilledAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelledAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CryptoOrders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CryptoPositions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Symbol = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Side = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: false),
                    EntryPrice = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: false),
                    CurrentPrice = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: false),
                    ClosePrice = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: true),
                    Leverage = table.Column<int>(type: "integer", nullable: false),
                    InitialMargin = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: false),
                    LiquidationPrice = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: false),
                    UnrealizedPnl = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    RealizedPnl = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    StopLoss = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: true),
                    TakeProfit = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: true),
                    TrailingStopDistance = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: true),
                    HighestPriceReached = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: true),
                    LowestPriceReached = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    OpenedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ClosedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CryptoPositions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CryptoStrategies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    StrategyType = table.Column<int>(type: "integer", nullable: false),
                    Symbol = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Timeframe = table.Column<int>(type: "integer", nullable: false),
                    ParametersJson = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CryptoStrategies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ForexAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    BaseCurrency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Balance = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Equity = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    UsedMargin = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    FreeMargin = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    MarginLevelPercent = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Leverage = table.Column<int>(type: "integer", nullable: false),
                    IsDemo = table.Column<bool>(type: "boolean", nullable: false),
                    PeakEquity = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DailyStartingBalance = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    LastDailyResetDateUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ForexAccounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ForexApiSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TwelveDataApiKey = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    AlphaVantageApiKey = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    FredApiKey = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    FinnhubApiKey = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    OandaApiKey = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    OandaAccountId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PrimaryPriceProvider = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    UseSimulatedExecutionBroker = table.Column<bool>(type: "boolean", nullable: false),
                    SimulatedLatencyMs = table.Column<int>(type: "integer", nullable: false),
                    EnableSafeRetries = table.Column<bool>(type: "boolean", nullable: false),
                    EnableNewsFilterWarning = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ForexApiSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ForexBacktests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StrategyId = table.Column<Guid>(type: "uuid", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    StrategyName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Symbol = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Timeframe = table.Column<int>(type: "integer", nullable: false),
                    StartDateUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDateUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    InitialBalance = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    FinalBalance = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    NetProfit = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ReturnPercent = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    MaxDrawdownPercent = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    WinRatePercent = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalTrades = table.Column<int>(type: "integer", nullable: false),
                    WinningTrades = table.Column<int>(type: "integer", nullable: false),
                    LosingTrades = table.Column<int>(type: "integer", nullable: false),
                    ProfitFactor = table.Column<decimal>(type: "numeric(10,4)", precision: 10, scale: 4, nullable: false),
                    ExpectancyPips = table.Column<decimal>(type: "numeric(10,4)", precision: 10, scale: 4, nullable: false),
                    SharpeRatio = table.Column<decimal>(type: "numeric(10,4)", precision: 10, scale: 4, nullable: false),
                    SpreadPips = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    SlippagePips = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalSwapCost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    IsWalkForward = table.Column<bool>(type: "boolean", nullable: false),
                    OutOfSampleEfficiency = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TradeLogsJson = table.Column<string>(type: "character varying(100000)", maxLength: 100000, nullable: false),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ForexBacktests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ForexEconomicAlerts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    ImpactLevel = table.Column<int>(type: "integer", nullable: false),
                    LeadMinutes = table.Column<int>(type: "integer", nullable: false),
                    AutoNoTradeFilterEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ForexEconomicAlerts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ForexJournalEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    TradeId = table.Column<Guid>(type: "uuid", nullable: true),
                    Symbol = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TradeDateUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    MarketSession = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SetupType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Emotions = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Mistakes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    LessonsLearned = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    DetailedNotes = table.Column<string>(type: "character varying(3000)", maxLength: 3000, nullable: false),
                    ScreenshotUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ExecutionRating = table.Column<int>(type: "integer", nullable: false),
                    FollowedPlan = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ForexJournalEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ForexOrders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Symbol = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Direction = table.Column<int>(type: "integer", nullable: false),
                    OrderType = table.Column<int>(type: "integer", nullable: false),
                    LotSize = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    RequestedPrice = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    FillPrice = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: true),
                    StopLoss = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: true),
                    TakeProfit = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: true),
                    SlippagePips = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    IdempotencyKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RejectionReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FilledAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelledAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ForexOrders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ForexStrategies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    StrategyType = table.Column<int>(type: "integer", nullable: false),
                    DefaultSymbol = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DefaultTimeframe = table.Column<int>(type: "integer", nullable: false),
                    RiskPercent = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    StopLossPips = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TakeProfitPips = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    UseTrailingStop = table.Column<bool>(type: "boolean", nullable: false),
                    TrailingStopPips = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    BreakEvenAtPipsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    BreakEvenTriggerPips = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ParametersJson = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ForexStrategies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ForexTrades",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    Symbol = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Direction = table.Column<int>(type: "integer", nullable: false),
                    LotSize = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    OpenPrice = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    ClosePrice = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: true),
                    StopLoss = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: true),
                    TakeProfit = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: true),
                    TrailingStopPips = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    BreakEvenTriggered = table.Column<bool>(type: "boolean", nullable: false),
                    MarginRequired = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    SwapCost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Commission = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    GrossProfit = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    NetProfit = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Pips = table.Column<decimal>(type: "numeric(18,5)", precision: 18, scale: 5, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ExitReason = table.Column<int>(type: "integer", nullable: false),
                    OpenTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CloseTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ScreenshotUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ForexTrades", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CryptoAccounts_TenantId",
                table: "CryptoAccounts",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_CryptoAccounts_UserId",
                table: "CryptoAccounts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CryptoAlerts_IsActive",
                table: "CryptoAlerts",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_CryptoAlerts_TenantId",
                table: "CryptoAlerts",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_CryptoAlerts_UserId",
                table: "CryptoAlerts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CryptoApiSettings_TenantId",
                table: "CryptoApiSettings",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_CryptoApiSettings_UserId",
                table: "CryptoApiSettings",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CryptoApiSettings_UserId_Exchange",
                table: "CryptoApiSettings",
                columns: new[] { "UserId", "Exchange" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CryptoBacktests_StrategyId",
                table: "CryptoBacktests",
                column: "StrategyId");

            migrationBuilder.CreateIndex(
                name: "IX_CryptoBacktests_TenantId",
                table: "CryptoBacktests",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_CryptoBacktests_UserId",
                table: "CryptoBacktests",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CryptoJournalEntries_PositionId",
                table: "CryptoJournalEntries",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_CryptoJournalEntries_TenantId",
                table: "CryptoJournalEntries",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_CryptoJournalEntries_UserId",
                table: "CryptoJournalEntries",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CryptoOrders_AccountId",
                table: "CryptoOrders",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_CryptoOrders_IdempotencyKey",
                table: "CryptoOrders",
                column: "IdempotencyKey");

            migrationBuilder.CreateIndex(
                name: "IX_CryptoOrders_TenantId",
                table: "CryptoOrders",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_CryptoOrders_UserId",
                table: "CryptoOrders",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CryptoPositions_AccountId",
                table: "CryptoPositions",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_CryptoPositions_Status",
                table: "CryptoPositions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_CryptoPositions_TenantId",
                table: "CryptoPositions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_CryptoPositions_UserId",
                table: "CryptoPositions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CryptoStrategies_TenantId",
                table: "CryptoStrategies",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_CryptoStrategies_UserId",
                table: "CryptoStrategies",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ForexAccounts_TenantId",
                table: "ForexAccounts",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ForexAccounts_UserId",
                table: "ForexAccounts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ForexApiSettings_TenantId",
                table: "ForexApiSettings",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ForexApiSettings_UserId",
                table: "ForexApiSettings",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ForexBacktests_StrategyId",
                table: "ForexBacktests",
                column: "StrategyId");

            migrationBuilder.CreateIndex(
                name: "IX_ForexBacktests_TenantId",
                table: "ForexBacktests",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ForexBacktests_UserId",
                table: "ForexBacktests",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ForexEconomicAlerts_TenantId",
                table: "ForexEconomicAlerts",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ForexEconomicAlerts_UserId",
                table: "ForexEconomicAlerts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ForexJournalEntries_Symbol",
                table: "ForexJournalEntries",
                column: "Symbol");

            migrationBuilder.CreateIndex(
                name: "IX_ForexJournalEntries_TenantId",
                table: "ForexJournalEntries",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ForexJournalEntries_TradeId",
                table: "ForexJournalEntries",
                column: "TradeId");

            migrationBuilder.CreateIndex(
                name: "IX_ForexJournalEntries_UserId",
                table: "ForexJournalEntries",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ForexOrders_AccountId",
                table: "ForexOrders",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_ForexOrders_IdempotencyKey",
                table: "ForexOrders",
                column: "IdempotencyKey");

            migrationBuilder.CreateIndex(
                name: "IX_ForexOrders_TenantId",
                table: "ForexOrders",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ForexOrders_UserId",
                table: "ForexOrders",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ForexStrategies_TenantId",
                table: "ForexStrategies",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ForexStrategies_UserId",
                table: "ForexStrategies",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ForexTrades_AccountId",
                table: "ForexTrades",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_ForexTrades_Status",
                table: "ForexTrades",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ForexTrades_Symbol",
                table: "ForexTrades",
                column: "Symbol");

            migrationBuilder.CreateIndex(
                name: "IX_ForexTrades_TenantId",
                table: "ForexTrades",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ForexTrades_UserId",
                table: "ForexTrades",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CryptoAccounts");

            migrationBuilder.DropTable(
                name: "CryptoAlerts");

            migrationBuilder.DropTable(
                name: "CryptoApiSettings");

            migrationBuilder.DropTable(
                name: "CryptoBacktests");

            migrationBuilder.DropTable(
                name: "CryptoJournalEntries");

            migrationBuilder.DropTable(
                name: "CryptoOrders");

            migrationBuilder.DropTable(
                name: "CryptoPositions");

            migrationBuilder.DropTable(
                name: "CryptoStrategies");

            migrationBuilder.DropTable(
                name: "ForexAccounts");

            migrationBuilder.DropTable(
                name: "ForexApiSettings");

            migrationBuilder.DropTable(
                name: "ForexBacktests");

            migrationBuilder.DropTable(
                name: "ForexEconomicAlerts");

            migrationBuilder.DropTable(
                name: "ForexJournalEntries");

            migrationBuilder.DropTable(
                name: "ForexOrders");

            migrationBuilder.DropTable(
                name: "ForexStrategies");

            migrationBuilder.DropTable(
                name: "ForexTrades");
        }
    }
}
