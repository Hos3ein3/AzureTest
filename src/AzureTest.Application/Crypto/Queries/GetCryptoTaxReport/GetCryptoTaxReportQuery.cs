using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Crypto;
using AzureTest.Domain.Entities.Crypto.Enums;
using FluentValidation;

namespace AzureTest.Application.Crypto.Queries.GetCryptoTaxReport;

public sealed record GetCryptoTaxReportQuery(Guid UserId, int TaxYear = 2026) : IQuery<Result<CryptoTaxReportDto>>;

public sealed class GetCryptoTaxReportQueryValidator : AbstractValidator<GetCryptoTaxReportQuery>
{
    public GetCryptoTaxReportQueryValidator()
    {
        RuleFor(x => x.TaxYear).InclusiveBetween(2020, 2030);
    }
}

public sealed class GetCryptoTaxReportQueryHandler : IQueryHandler<GetCryptoTaxReportQuery, Result<CryptoTaxReportDto>>
{
    private readonly IUnitOfWork _uow;

    public GetCryptoTaxReportQueryHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Result<CryptoTaxReportDto>> HandleAsync(GetCryptoTaxReportQuery query, CancellationToken ct)
    {
        var positions = (await _uow.CryptoPositions.FindAsync(p => p.UserId == query.UserId && p.Status != CryptoPositionStatus.Open))
            .Where(p => p.ClosedAtUtc.HasValue && p.ClosedAtUtc.Value.Year == query.TaxYear)
            .OrderBy(p => p.ClosedAtUtc)
            .ToList();

        decimal totalRealizedPnl = positions.Sum(p => p.RealizedPnl);
        decimal totalVolume = positions.Sum(p => p.Quantity * (p.ClosePrice ?? p.EntryPrice));

        var items = positions.Select(p => new CryptoTaxItemDto(
            Date: p.ClosedAtUtc ?? DateTime.UtcNow,
            Symbol: p.Symbol,
            Action: p.Side == CryptoPositionSide.Long ? "SELL (Close Long)" : "BUY (Close Short)",
            Quantity: p.Quantity,
            PriceEur: Math.Round((p.ClosePrice ?? p.EntryPrice) * 0.92m, 4), // Converted to EUR reference
            TotalValueEur: Math.Round(p.Quantity * (p.ClosePrice ?? p.EntryPrice) * 0.92m, 2),
            RealizedGainLossEur: Math.Round(p.RealizedPnl * 0.92m, 2))).ToList();

        // Dutch Box 3 Guidance
        string guidance = "Under Dutch Belastingdienst legislation, cryptocurrency is classified as 'overige bezittingen' (other assets) in Box 3. You are taxed not on your actual capital gains or trading profits (unless classified as professional business income in Box 1), but on a deemed return ('forfaitair rendement') on the total fair market value of your crypto assets on the reference date of January 1st, 00:00 AM of the tax year. Assets exceeding the tax-free allowance ('heffingvrij vermogen') are subject to Box 3 wealth tax. Keep this trade journal and valuation export as proof of your January 1st portfolio value and annual transactional trail.";

        var report = new CryptoTaxReportDto(
            TaxYear: query.TaxYear,
            TotalRealizedPnlEur: Math.Round(totalRealizedPnl * 0.92m, 2),
            TotalTradesCount: positions.Count,
            TotalVolumeEur: Math.Round(totalVolume * 0.92m, 2),
            EstimatedBox3ValuationJan1stEur: 9200.00m,
            DutchTaxBox3Guidance: guidance,
            Items: items);

        return Result<CryptoTaxReportDto>.Success(report);
    }
}
