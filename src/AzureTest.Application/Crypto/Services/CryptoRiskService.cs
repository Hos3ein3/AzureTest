using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Contracts.DTOs.Crypto;
using AzureTest.Domain.Entities.Crypto;
using AzureTest.Domain.Entities.Crypto.Enums;

namespace AzureTest.Application.Crypto.Services;

public class CryptoRiskService : ICryptoRiskService
{
    private readonly IUnitOfWork _uow;

    public CryptoRiskService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public PositionSizeCalculatorDto CalculatePositionSize(
        decimal accountEquity,
        decimal riskPercent,
        decimal entryPrice,
        decimal stopLossPrice,
        int requestedLeverage)
    {
        accountEquity = Math.Max(100m, accountEquity);
        riskPercent = Math.Clamp(riskPercent, CryptoAccount.Rules.MinRiskPerTrade, CryptoAccount.Rules.MaxRiskPerTrade);
        entryPrice = Math.Max(0.00000001m, entryPrice);
        stopLossPrice = Math.Max(0.00000001m, stopLossPrice);
        requestedLeverage = Math.Clamp(requestedLeverage, CryptoPosition.Rules.LeverageMin, CryptoPosition.Rules.LeverageMax);

        decimal riskAmount = accountEquity * (riskPercent / 100m);
        decimal slDistance = Math.Abs(entryPrice - stopLossPrice);

        if (slDistance == 0m)
        {
            slDistance = entryPrice * 0.02m;
        }

        decimal recommendedQty = riskAmount / slDistance;
        decimal totalValue = recommendedQty * entryPrice;

        int recommendedLeverage = Math.Max(1, (int)Math.Ceiling(totalValue / accountEquity));
        if (requestedLeverage > 1) recommendedLeverage = requestedLeverage;

        decimal liqPrice;
        if (recommendedLeverage <= 1)
        {
            liqPrice = 0m;
        }
        else
        {
            decimal leverageRatio = 1.0m / recommendedLeverage;
            liqPrice = Math.Max(0m, entryPrice * (1.0m - leverageRatio + CryptoPosition.Rules.MaintenanceMarginRatio));
        }

        decimal distToLiqPct = entryPrice > 0 ? Math.Abs((entryPrice - liqPrice) / entryPrice) * 100m : 0m;

        return new PositionSizeCalculatorDto(
            AccountEquity: Math.Round(accountEquity, 2),
            RiskPercent: Math.Round(riskPercent, 2),
            RiskAmount: Math.Round(riskAmount, 2),
            EntryPrice: Math.Round(entryPrice, 4),
            StopLossPrice: Math.Round(stopLossPrice, 4),
            RecommendedQuantity: Math.Round(recommendedQty, 4),
            TotalPositionValue: Math.Round(totalValue, 2),
            RecommendedLeverage: recommendedLeverage,
            LiquidationPrice: Math.Round(liqPrice, 4),
            DistanceToLiquidationPercent: Math.Round(distToLiqPct, 2));
    }

    public async Task<CryptoRiskOverviewDto> GetRiskOverviewAsync(Guid userId, CancellationToken ct = default)
    {
        var accounts = await _uow.CryptoAccounts.FindAsync(a => a.UserId == userId);
        var account = accounts.FirstOrDefault() ?? CryptoAccount.Create(userId, Guid.Empty, "Primary Trading Account", "USDT", CryptoTradingMode.PaperTrading);

        var positions = (await _uow.CryptoPositions.FindAsync(p => p.UserId == userId && p.Status == CryptoPositionStatus.Open)).ToList();

        decimal totalRiskExposure = positions.Sum(p =>
        {
            if (p.StopLoss.HasValue)
            {
                return Math.Abs(p.EntryPrice - p.StopLoss.Value) * p.Quantity;
            }
            return p.InitialMargin;
        });

        decimal marginLevel = account.MarginUsed > 0 ? (account.Equity / account.MarginUsed) * 100m : 999.0m;

        var calculator = CalculatePositionSize(
            accountEquity: account.Equity,
            riskPercent: account.RiskPercentagePerTrade,
            entryPrice: 67500m,
            stopLossPrice: 66150m,
            requestedLeverage: 1);

        var alerts = new List<string>();

        if (account.IsKillSwitchActive)
        {
            alerts.Add($"KILL SWITCH ACTIVE: {account.KillSwitchReason}. All trading disabled.");
        }

        if (account.CurrentDailyLoss >= (account.DailyStartingEquity * (account.DailyLossLimitPercent / 100m)))
        {
            alerts.Add($"Daily loss limit breached ({account.DailyLossLimitPercent}%). Take a break and review your trading journal.");
        }

        if (positions.Count >= account.MaxOpenPositions)
        {
            alerts.Add($"Maximum concurrent open positions reached ({account.MaxOpenPositions}/{account.MaxOpenPositions}). Close positions before opening new ones.");
        }

        var highLeverageCount = positions.Count(p => p.Leverage > 5);
        if (highLeverageCount > 0)
        {
            alerts.Add($"You have {highLeverageCount} position(s) with leverage > 5x. High leverage dramatically magnifies liquidation risk during sudden crypto wicks.");
        }

        var accountDto = new CryptoAccountDto(
            Id: account.Id,
            Name: account.Name,
            BaseCurrency: account.BaseCurrency,
            TradingMode: account.TradingMode,
            CashBalance: account.CashBalance,
            Equity: account.Equity,
            MarginUsed: account.MarginUsed,
            FreeMargin: account.FreeMargin,
            DailyStartingEquity: account.DailyStartingEquity,
            DailyLoss: new CurrentDailyLossDto(
                Amount: account.CurrentDailyLoss,
                Percent: account.DailyStartingEquity > 0 ? Math.Round((account.CurrentDailyLoss / account.DailyStartingEquity) * 100m, 2) : 0m,
                LimitPercent: account.DailyLossLimitPercent,
                IsBreached: account.CurrentDailyLoss >= (account.DailyStartingEquity * (account.DailyLossLimitPercent / 100m))),
            RiskPercentagePerTrade: account.RiskPercentagePerTrade,
            MaxOpenPositions: account.MaxOpenPositions,
            IsKillSwitchActive: account.IsKillSwitchActive,
            KillSwitchReason: account.KillSwitchReason);

        string dutchNote = "In the Netherlands and EU, the AFM (Autoriteit Financiële Markten) and ESMA enforce strict CFD/crypto derivative leverage caps (typically 2:1 for crypto CFDs for retail investors). Additionally, under the EU MiCA (Markets in Crypto-Assets) regulation, unauthorized exchanges without CASP registration cannot market to Dutch residents. Crypto assets held on January 1st must be reported under Dutch Box 3 (overige bezittingen).";

        return new CryptoRiskOverviewDto(
            Account: accountDto,
            TotalRiskExposureUsd: Math.Round(totalRiskExposure, 2),
            MarginLevelPercent: Math.Round(marginLevel, 2),
            OpenPositionsCount: positions.Count,
            MaxAllowedPositions: account.MaxOpenPositions,
            KillSwitchActive: account.IsKillSwitchActive,
            PositionSizingGuide: calculator,
            RiskAlerts: alerts,
            DutchAfmWarningNote: dutchNote);
    }

    public async Task<bool> CanOpenNewPositionAsync(CryptoAccount account, decimal requiredMargin, CancellationToken ct = default)
    {
        if (account.IsKillSwitchActive) return false;

        if (account.DailyStartingEquity > 0)
        {
            var maxLossAllowed = account.DailyStartingEquity * (account.DailyLossLimitPercent / 100m);
            if (account.CurrentDailyLoss >= maxLossAllowed)
            {
                return false;
            }
        }

        var activePositions = await _uow.CryptoPositions.FindAsync(p => p.AccountId == account.Id && p.Status == CryptoPositionStatus.Open);
        if (activePositions.Count() >= account.MaxOpenPositions)
        {
            return false;
        }

        if (account.FreeMargin < requiredMargin)
        {
            return false;
        }

        return true;
    }
}
