using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AzureTest.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTradingArea : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TradingAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    CashBalance = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    Equity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    MarginUsed = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    RealizedPnl = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
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
                    table.PrimaryKey("PK_TradingAccounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TradingOrders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Symbol = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    AssetName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AssetClass = table.Column<int>(type: "integer", nullable: false),
                    Side = table.Column<int>(type: "integer", nullable: false),
                    OrderType = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    TriggerPrice = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    Leverage = table.Column<int>(type: "integer", nullable: false),
                    StopLoss = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    TakeProfit = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    MultipleTakeProfitsJson = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    TrailingStopDistance = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    TimeInForce = table.Column<int>(type: "integer", nullable: false),
                    ExpiryDateUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    PlacedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_TradingOrders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TradingPositions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Symbol = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    AssetName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AssetClass = table.Column<int>(type: "integer", nullable: false),
                    Side = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    EntryPrice = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    CurrentPrice = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    ClosePrice = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    Leverage = table.Column<int>(type: "integer", nullable: false),
                    InitialMargin = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    UnrealizedPnl = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    RealizedPnl = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    StopLoss = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    TakeProfit = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    MultipleTakeProfitsJson = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    TrailingStopDistance = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    HighestPriceReached = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    LowestPriceReached = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
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
                    table.PrimaryKey("PK_TradingPositions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TradingWatchlists",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Symbol = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    AssetName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AssetClass = table.Column<int>(type: "integer", nullable: false),
                    AddedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_TradingWatchlists", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TradingAccounts_TenantId",
                table: "TradingAccounts",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_TradingAccounts_UserId",
                table: "TradingAccounts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TradingOrders_AccountId",
                table: "TradingOrders",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_TradingOrders_Status",
                table: "TradingOrders",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_TradingOrders_Symbol",
                table: "TradingOrders",
                column: "Symbol");

            migrationBuilder.CreateIndex(
                name: "IX_TradingOrders_TenantId",
                table: "TradingOrders",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_TradingOrders_UserId",
                table: "TradingOrders",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TradingPositions_AccountId",
                table: "TradingPositions",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_TradingPositions_Status",
                table: "TradingPositions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_TradingPositions_Symbol",
                table: "TradingPositions",
                column: "Symbol");

            migrationBuilder.CreateIndex(
                name: "IX_TradingPositions_TenantId",
                table: "TradingPositions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_TradingPositions_UserId",
                table: "TradingPositions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TradingWatchlists_TenantId",
                table: "TradingWatchlists",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_TradingWatchlists_UserId",
                table: "TradingWatchlists",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TradingWatchlists_UserId_Symbol",
                table: "TradingWatchlists",
                columns: new[] { "UserId", "Symbol" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TradingAccounts");

            migrationBuilder.DropTable(
                name: "TradingOrders");

            migrationBuilder.DropTable(
                name: "TradingPositions");

            migrationBuilder.DropTable(
                name: "TradingWatchlists");
        }
    }
}
