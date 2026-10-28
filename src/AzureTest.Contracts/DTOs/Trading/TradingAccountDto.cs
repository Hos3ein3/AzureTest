using System;

namespace AzureTest.Contracts.DTOs.Trading;

public sealed record TradingAccountDto(
    Guid Id,
    Guid UserId,
    Guid TenantId,
    string Name,
    string Currency,
    decimal CashBalance,
    decimal Equity,
    decimal MarginUsed,
    decimal FreeMargin,
    decimal RealizedPnl
);
