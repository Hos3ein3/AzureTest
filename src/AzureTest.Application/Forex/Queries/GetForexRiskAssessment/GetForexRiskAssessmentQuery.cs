using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Forex.Services;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Forex;
using AzureTest.Domain.Entities.Forex;
using AzureTest.Domain.Entities.Forex.Enums;
using FluentValidation;

namespace AzureTest.Application.Forex.Queries.GetForexRiskAssessment;

public sealed record GetForexRiskAssessmentQuery : IQuery<Result<ForexRiskOverviewDto>>;

public sealed class GetForexRiskAssessmentQueryValidator : AbstractValidator<GetForexRiskAssessmentQuery>
{
    public GetForexRiskAssessmentQueryValidator()
    {
    }
}

public sealed class GetForexRiskAssessmentQueryHandler : IQueryHandler<GetForexRiskAssessmentQuery, Result<ForexRiskOverviewDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IForexRiskService _riskService;
    private readonly IForexAnalysisService _analysisService;
    private readonly ICurrentUser _currentUser;

    public GetForexRiskAssessmentQueryHandler(
        IUnitOfWork unitOfWork,
        IForexRiskService riskService,
        IForexAnalysisService analysisService,
        ICurrentUser currentUser)
    {
        _unitOfWork = unitOfWork;
        _riskService = riskService;
        _analysisService = analysisService;
        _currentUser = currentUser;
    }

    public async Task<Result<ForexRiskOverviewDto>> HandleAsync(GetForexRiskAssessmentQuery query, CancellationToken cancellationToken)
    {
        var accounts = await _unitOfWork.ForexAccounts.FindAsync(a => a.UserId == _currentUser.Id);
        var account = accounts.FirstOrDefault();
        if (account == null)
        {
            account = ForexAccount.Create(_currentUser.Id, "Demo Primary", "USD", 10_000m, 30, true);
            _unitOfWork.ForexAccounts.Add(account);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        var openTradesEntities = await _unitOfWork.ForexTrades.FindAsync(
            t => t.UserId == _currentUser.Id && t.Status == ForexTradeStatus.Open);

        var openTradeDtos = openTradesEntities.Select(t => new ForexTradeDto(
            t.Id, t.AccountId, t.OrderId, t.Symbol, t.Direction, t.LotSize, t.OpenPrice,
            t.ClosePrice, t.StopLoss, t.TakeProfit, t.TrailingStopPips, t.BreakEvenTriggered,
            t.MarginRequired, t.SwapCost, t.Commission, t.GrossProfit, t.NetProfit, t.Pips,
            t.Status, t.ExitReason, t.OpenTimeUtc, t.CloseTimeUtc, t.Notes, t.ScreenshotUrl,
            t.OpenPrice, 0m, 0m
        )).ToList();

        var upcomingEvents = await _analysisService.GetEconomicCalendarAsync(new ForexEconomicEventFilter(), cancellationToken);

        var accountDto = new ForexAccountDto(
            account.Id, account.Name, account.BaseCurrency, account.Balance, account.Equity,
            account.UsedMargin, account.FreeMargin, account.MarginLevelPercent, account.Leverage,
            account.IsDemo, account.PeakEquity, account.DailyStartingBalance, 0m, 0m
        );

        var overview = _riskService.GetRiskOverview(accountDto, openTradeDtos, upcomingEvents);
        return Result<ForexRiskOverviewDto>.Success(overview);
    }
}
