using AzureTest.Contracts.DTOs.Events;
using AzureTest.Contracts.DTOs.Tickets;
using AzureTest.Contracts.DTOs.Debts;
using AzureTest.Application.Abstractions;


using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using AzureTest.MVC.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using AzureTest.Application.Resumes;
using AzureTest.Application.Resumes.Queries.GetResumeAnalytics;
using AzureTest.Domain.Entities.Identity;
using AzureTest.Application.Debts.Interfaces;
using AzureTest.Application.Events;
using AzureTest.Application.Expenses.Units;
using AzureTest.Application.Habits;
using AzureTest.Application.Tasks;
using AzureTest.Application.Tickets;

namespace AzureTest.MVC.Controllers;

[Authorize]
[DisplayName("Home")]
[Description("Manage and control Home operations.")]
    public class HomeController : Controller
{
    private readonly IResumeQueryUnit _queries;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEventQueryUnit _eventQuery;
    private readonly ITaskQueryUnit _taskQuery;
    private readonly ITicketQueryUnit _ticketQuery;
    private readonly IHabitQueryUnit _habitQuery;
    private readonly IDebtQueryUnit _debtQuery;
    private readonly IExpenseQueryUnit _expenseQuery;
    private readonly AzureTest.Application.Pages.GetAllPagesQueryHandler _pagesQuery;
    private readonly AzureTest.Application.Media.IMediaQueryUnit _mediaQuery;
    private readonly AzureTest.Application.Gym.GetWorkoutAnalyticsQueryHandler _workoutAnalyticsQuery;
    private readonly AzureTest.Application.Food.IFoodQueryUnit _foodQuery;
    private readonly AzureTest.MVC.Services.IUserTracker _userTracker;
    private readonly AzureTest.Application.Trading.ITradingQueryUnit _tradingQuery;
    private readonly AzureTest.Application.Crypto.ICryptoQueryUnit _cryptoQuery;
    private readonly AzureTest.Application.Forex.IForexQueryUnit _forexQuery;

    public HomeController(
        IResumeQueryUnit queries, 
        UserManager<ApplicationUser> userManager,
        IEventQueryUnit eventQuery,
        ITaskQueryUnit taskQuery,
        ITicketQueryUnit ticketQuery,
        IHabitQueryUnit habitQuery,
        IDebtQueryUnit debtQuery,
        IExpenseQueryUnit expenseQuery,
        AzureTest.MVC.Services.IUserTracker userTracker,
        AzureTest.Application.Pages.GetAllPagesQueryHandler pagesQuery,
        AzureTest.Application.Media.IMediaQueryUnit mediaQuery,
        AzureTest.Application.Gym.GetWorkoutAnalyticsQueryHandler workoutAnalyticsQuery,
        AzureTest.Application.Food.IFoodQueryUnit foodQuery,
        AzureTest.Application.Trading.ITradingQueryUnit tradingQuery,
        AzureTest.Application.Crypto.ICryptoQueryUnit cryptoQuery,
        AzureTest.Application.Forex.IForexQueryUnit forexQuery)
    {
        _queries = queries;
        _userManager = userManager;
        _eventQuery = eventQuery;
        _taskQuery = taskQuery;
        _ticketQuery = ticketQuery;
        _habitQuery = habitQuery;
        _debtQuery = debtQuery;
        _expenseQuery = expenseQuery;
        _userTracker = userTracker;
        _pagesQuery = pagesQuery;
        _mediaQuery = mediaQuery;
        _workoutAnalyticsQuery = workoutAnalyticsQuery;
        _foodQuery = foodQuery;
        _tradingQuery = tradingQuery;
        _cryptoQuery = cryptoQuery;
        _forexQuery = forexQuery;
    }

    [DisplayName("Index")]
    [Description("View a list of Home.")]
    public async Task<IActionResult> Index(
        string? userId, 
        [FromServices] AzureTest.Application.Messaging.IQueryHandler<AzureTest.Application.UserPreferences.Queries.GetUserPreferenceQuery, AzureTest.Domain.Documents.UserPreference> prefQuery,
        [FromQuery] bool ignorePreference,
        CancellationToken cancellationToken)
    {
        var currentUserIdStr = _userManager.GetUserId(User);
        var isAdmin = User.IsInRole("Admin");
        
        if (currentUserIdStr != null && !ignorePreference && string.IsNullOrWhiteSpace(userId))
        {
            var pref = await prefQuery.HandleAsync(new AzureTest.Application.UserPreferences.Queries.GetUserPreferenceQuery(Guid.Parse(currentUserIdStr)), cancellationToken);
            if (pref != null && !string.IsNullOrWhiteSpace(pref.PreferredDashboard))
            {
                return Redirect($"/{pref.PreferredDashboard}");
            }
        }
        
        var targetUserIdStr = userId;
        if (string.IsNullOrWhiteSpace(userId) && !isAdmin)
        {
            targetUserIdStr = currentUserIdStr;
        }

        var data = await _queries.GetAnalytics.HandleAsync(new GetResumeAnalyticsQuery(targetUserIdStr), cancellationToken);
        
        var model = new HomeDashboardViewModel
        {
            Analytics = data,
            IsAdmin = data.IsAdmin,
            OnlineUsersCount = _userTracker.GetOnlineUsersCount(),
            CurrentUserId = targetUserIdStr ?? string.Empty,
            IsViewingOtherUser = !string.IsNullOrWhiteSpace(targetUserIdStr) && targetUserIdStr != currentUserIdStr
        };

        Guid? parsedUserId = null;
        if (!string.IsNullOrEmpty(model.CurrentUserId) && Guid.TryParse(model.CurrentUserId, out var tempId))
        {
            parsedUserId = tempId;
        }

        if (parsedUserId.HasValue || model.IsAdmin)
        {
            var now = DateTime.UtcNow;
            
            // Recent Events (upcoming within 7 days)
            var allEventsResult = await _eventQuery.GetAllEvents.HandleAsync(new AzureTest.Application.Events.Queries.GetAllEventsQuery(new GetAllEventsFilters { PageSize = -1 }, parsedUserId), cancellationToken);
            var allEvents = allEventsResult.Items;
            model.RecentEvents = allEvents.Where(e => e.StartDate >= now.AddDays(-1) && e.StartDate <= now.AddDays(7)).Take(5).ToList();

            // Pending Tasks
            var allTasks = await _taskQuery.GetAllTasks.HandleAsync(new AzureTest.Application.Tasks.Queries.GetAllTasksQuery(parsedUserId), cancellationToken);
            model.PendingTasks = allTasks.Where(t => !t.IsCompleted).Take(5).ToList();

            // Recent Habits
            var allHabits = await _habitQuery.GetAllHabits.HandleAsync(new AzureTest.Application.Habits.Queries.GetAllHabitsQuery(parsedUserId), cancellationToken);
            model.RecentHabits = allHabits.Take(5).ToList();

            // Tickets
            if (model.IsAdmin && !parsedUserId.HasValue)
            {
                // Admin viewing aggregate -> show unanswered
                model.Tickets = await _ticketQuery.GetUnansweredTickets.HandleAsync(new AzureTest.Application.Tickets.Queries.GetUnansweredTicketsQuery(), cancellationToken);
            }
            else
            {
                // User or Admin viewing specific user -> show their unread/open
                model.Tickets = await _ticketQuery.GetUnreadTickets.HandleAsync(new AzureTest.Application.Tickets.Queries.GetUnreadTicketsQuery(parsedUserId ?? Guid.Empty), cancellationToken);
            }
            
            // Ticket Analytics
            var allUserTicketsResult = await _ticketQuery.GetAllTickets.HandleAsync(new AzureTest.Application.Tickets.Queries.GetAllTicketsQuery(new GetAllTicketsFilters { PageSize = -1 }, parsedUserId), cancellationToken);
            var allUserTickets = allUserTicketsResult.Items;
            model.TicketStatusCounts = allUserTickets.GroupBy(t => t.Status.ToString()).ToDictionary(g => g.Key, g => g.Count());
            
            // Habit Adherence (Last 30 days)
            int doneHabits = 0;
            int pendingHabits = 0;
            var thirtyDaysAgo = now.AddDays(-30);
            foreach (var h in allHabits)
            {
                var recentLogs = h.Logs.Where(l => l.LoggedAt >= thirtyDaysAgo).ToList();
                var logsByDate = recentLogs.GroupBy(l => l.LocalDate).ToList();
                foreach (var dayGroup in logsByDate)
                {
                    bool isDone = false;
                    double target = dayGroup.First().TargetSnapshot ?? h.Target ?? 0;
                    if (h.Type == AzureTest.Domain.Enums.HabitType.Boolean)
                    {
                        isDone = dayGroup.Any(l => l.BooleanValue || l.Value == "true");
                    }
                    else if (h.Type == AzureTest.Domain.Enums.HabitType.Counter)
                    {
                        var sum = dayGroup.Sum(l => l.NumericValue > 0 ? l.NumericValue : 1);
                        isDone = h.Direction == AzureTest.Domain.Enums.HabitDirection.AtLeast ? sum >= target : sum <= target;
                    }
                    else
                    {
                        var val = dayGroup.Sum(l => l.NumericValue);
                        isDone = h.Direction == AzureTest.Domain.Enums.HabitDirection.AtLeast ? val >= target : val <= target;
                    }

                    if (isDone) doneHabits++;
                    else pendingHabits++;
                }
            }
            model.HabitAdherence["Done"] = doneHabits;
            model.HabitAdherence["Pending/Failed"] = pendingHabits;

            // Recent Debts
            var allDebtsResult = await _debtQuery.GetAll.HandleAsync(new AzureTest.Application.Debts.Queries.GetAllDebtsQuery(new GetAllDebtsFilters { PageSize = -1 }, parsedUserId), cancellationToken);
            var allDebts = allDebtsResult.Items;
            model.RecentDebts = allDebts.Take(5).ToList();
            
            // Promissed Debts (Due Soon)
            model.PromissedDebts = allDebts
                .Where(d => d.PromissedReturnDate.HasValue && !d.IsDone)
                .OrderBy(d => d.PromissedReturnDate)
                .Take(5).ToList();
            
            // Debt Graph Data
            model.DebtsByPerson = allDebts
                .Where(d => d.Amount > 0)
                .GroupBy(d => d.DebtPersonName ?? "Unknown")
                .ToDictionary(g => g.Key, g => g.Sum(d => d.Amount));
            
            // Recent Expenses
            var allExpenses = await _expenseQuery.GetAll.HandleAsync(new AzureTest.Application.Expenses.Queries.GetAllExpensesQuery(
                new AzureTest.Contracts.DTOs.Expenses.GetAllExpensesFilters 
                { 
                    UserId = parsedUserId,
                    PageSize = 1000,
                    SortBy = "Date",
                    SortDescending = true
                }), cancellationToken);
            
            model.RecentExpenses = allExpenses.Items.Take(5).ToList();
            
            // Expense Graph Data
            model.ExpensesByCategory = allExpenses.Items
                .Where(e => e.Amount > 0)
                .GroupBy(e => string.IsNullOrWhiteSpace(e.CategoryName) ? "Uncategorized" : e.CategoryName)
                .ToDictionary(g => g.Key, g => g.Sum(e => e.Amount));
                
            // Monthly Trend (Last 6 Months)
            for (int i = 5; i >= 0; i--)
            {
                var monthDate = now.AddMonths(-i);
                var monthKey = monthDate.ToString("MMM yyyy");
                
                var monthExpenses = allExpenses.Items.Where(e => e.Date.Year == monthDate.Year && e.Date.Month == monthDate.Month).Sum(e => e.Amount);
                var monthDebts = allDebts.Where(d => d.DateOfBorrow.Year == monthDate.Year && d.DateOfBorrow.Month == monthDate.Month).Sum(d => d.Amount);
                
                model.MonthlyExpenses[monthKey] = monthExpenses;
                model.MonthlyDebts[monthKey] = monthDebts;
            }
                
            // Analytics KPIs
            model.TotalOutstandingDebt = allDebts.Where(d => !d.IsDone).Sum(d => d.Amount);
            model.TotalExpensesThisMonth = model.MonthlyExpenses.GetValueOrDefault(now.ToString("MMM yyyy"), 0);
        }

        // Pages Widget Data
        var pagesResult = await _pagesQuery.HandleAsync(new AzureTest.Application.Pages.GetAllPagesQuery(new AzureTest.Application.Pages.PageFilter { PageSize = -1 }), cancellationToken);
        if (pagesResult.IsSuccess && pagesResult.Value.Items != null)
        {
            var publishedPages = pagesResult.Value.Items.Where(p => p.Status == AzureTest.Domain.Enums.PageStatus.Published).ToList();
            model.RecentPages = publishedPages.OrderByDescending(p => p.CreatedAt).Take(6).ToList();
            
            var effectiveUserId = parsedUserId ?? (Guid.TryParse(currentUserIdStr, out var cid) ? cid : Guid.Empty);
            if (effectiveUserId != Guid.Empty)
            {
                model.UnreadPagesCount = publishedPages.Count(p => p.ReadBy == null || !p.ReadBy.Contains(effectiveUserId));
            }
            else
            {
                model.UnreadPagesCount = publishedPages.Count;
            }
        }

        var targetUserGuid = parsedUserId ?? (Guid.TryParse(currentUserIdStr, out var cguid) ? cguid : Guid.Empty);
        if (targetUserGuid != Guid.Empty)
        {
            // Media Analytics
            try
            {
                var mediaResult = await _mediaQuery.GetMediaStatsAsync(new AzureTest.Application.Media.Queries.GetMediaStats.GetMediaStatsQuery(targetUserGuid), cancellationToken);
                if (mediaResult.IsSuccess && mediaResult.Value != null)
                {
                    var m = mediaResult.Value;
                    model.MediaStats = m;
                    model.TotalMediaHours = m.TotalEstimatedHoursWatched;
                    model.MediaByType["Movies"] = m.TotalMoviesWatched;
                    model.MediaByType["Series"] = m.TotalSeriesWatched;
                    model.MediaByType["Anime"] = m.TotalAnimeWatched;

                    if (m.StatusBreakdown != null)
                    {
                        foreach (var s in m.StatusBreakdown)
                        {
                            model.MediaByStatus[s.Status] = s.Count;
                        }
                    }
                }
            }
            catch { }

            // Health, Weight & BMI Analytics
            try
            {
                var workoutAnalyticsResult = await _workoutAnalyticsQuery.HandleAsync(new AzureTest.Application.Gym.GetWorkoutAnalyticsQuery(targetUserGuid), cancellationToken);
                if (workoutAnalyticsResult.IsSuccess && workoutAnalyticsResult.Value != null)
                {
                    var h = workoutAnalyticsResult.Value;
                    model.HealthStats = h;
                    model.CurrentWeightKg = h.CurrentWeightKg;
                    model.CurrentBmi = h.CurrentBmi;
                    model.CurrentBmiCategory = h.CurrentBmiCategory ?? "Not Recorded";

                    if (h.CurrentMusclePercent.HasValue && h.CurrentMusclePercent > 0)
                        model.BodyComposition["Muscle"] = Math.Round(h.CurrentMusclePercent.Value, 1);
                    if (h.CurrentBonePercent.HasValue && h.CurrentBonePercent > 0)
                        model.BodyComposition["Bone"] = Math.Round(h.CurrentBonePercent.Value, 1);
                    if (h.CurrentWeightKg.HasValue && h.CurrentWeightKg > 0)
                        model.BodyComposition["Body Mass"] = Math.Round(h.CurrentWeightKg.Value, 1);

                    if (h.WeightLogs != null && h.WeightLogs.Any())
                    {
                        foreach (var log in h.WeightLogs.OrderBy(l => l.Date).TakeLast(10))
                        {
                            var key = log.Date.ToString("MMM dd");
                            model.WeightHistoryTrend[key] = Math.Round(log.WeightKg, 1);
                        }
                    }
                }
            }
            catch { }

            // Shopping List Analytics
            try
            {
                var shoppingResult = await _foodQuery.GetShoppingList.HandleAsync(new AzureTest.Application.Food.GetShoppingListQuery(new AzureTest.Contracts.DTOs.Food.ShoppingListFilter { UserId = targetUserGuid, IncludeShared = true }), cancellationToken);
                if (shoppingResult.IsSuccess && shoppingResult.Value != null)
                {
                    var s = shoppingResult.Value;
                    model.ShoppingListTotalItems = s.TotalItems;
                    model.ShoppingListToBuyItems = s.ToBuyCount;
                    model.ShoppingListBoughtItems = s.BoughtCount;
                    model.ShoppingListInCartItems = s.InCartCount;
                    model.ShoppingListEstimatedCost = s.EstimatedTotalCost;

                    if (s.Sections != null)
                    {
                        foreach (var grp in s.Sections)
                        {
                            model.ShoppingListByCategory[grp.Key.ToString()] = grp.Value.Count;
                        }
                    }
                }
            }
            catch { }

            // ── Universal Paper Trading Telemetry ──────────────────────────────
            try
            {
                var tradingRes = await _tradingQuery.GetDashboardAsync(new AzureTest.Application.Trading.Queries.GetTradingDashboard.GetTradingDashboardQuery(targetUserGuid, Guid.Empty), cancellationToken);
                if (tradingRes.IsSuccess && tradingRes.Value != null)
                {
                    var td = tradingRes.Value;
                    var open = td.OpenPositions ?? new List<AzureTest.Contracts.DTOs.Trading.TradingPositionDto>();
                    var closed = td.ClosedPositions ?? new List<AzureTest.Contracts.DTOs.Trading.TradingPositionDto>();
                    var winning = closed.Count(p => p.RealizedPnl > 0);
                    var losing = closed.Count(p => p.RealizedPnl < 0);
                    var breakEven = closed.Count(p => p.RealizedPnl == 0);
                    var winRate = closed.Count > 0 ? (decimal)winning / closed.Count * 100m : 0m;
                    var totalGrossWin = closed.Where(p => p.RealizedPnl > 0).Sum(p => p.RealizedPnl);
                    var totalGrossLoss = Math.Abs(closed.Where(p => p.RealizedPnl < 0).Sum(p => p.RealizedPnl));
                    var pf = totalGrossLoss > 0 ? Math.Round(totalGrossWin / totalGrossLoss, 2) : (totalGrossWin > 0 ? 99m : 0m);

                    var unrealized = open.Sum(p => p.UnrealizedPnl);
                    var marginLevel = td.Account.MarginUsed > 0 ? Math.Round((td.Account.Equity / td.Account.MarginUsed) * 100m, 1) : 999m;
                    var marginUsedPct = td.Account.Equity > 0 ? Math.Round((td.Account.MarginUsed / td.Account.Equity) * 100m, 1) : 0m;
                    var assetDist = open.GroupBy(p => p.AssetClass.ToString()).ToDictionary(g => g.Key, g => g.Count());

                    var longTrades = open.Count(p => p.Side == AzureTest.Domain.Entities.Trading.Enums.TradingOrderSide.Buy) + closed.Count(p => p.Side == AzureTest.Domain.Entities.Trading.Enums.TradingOrderSide.Buy);
                    var shortTrades = open.Count(p => p.Side == AzureTest.Domain.Entities.Trading.Enums.TradingOrderSide.Sell) + closed.Count(p => p.Side == AzureTest.Domain.Entities.Trading.Enums.TradingOrderSide.Sell);
                    var avgTradePnl = closed.Count > 0 ? Math.Round(closed.Average(p => p.RealizedPnl), 2) : 0m;

                    var pnlBySymbol = closed
                        .GroupBy(p => p.Symbol)
                        .ToDictionary(g => g.Key, g => g.Sum(x => x.RealizedPnl));
                    if (!pnlBySymbol.Any() && open.Any())
                    {
                        pnlBySymbol = open
                            .GroupBy(p => p.Symbol)
                            .ToDictionary(g => g.Key, g => g.Sum(x => x.UnrealizedPnl));
                    }

                    var equityCurve = new Dictionary<string, decimal>();
                    if (closed.Any())
                    {
                        var sortedClosed = closed.OrderBy(p => p.ClosedAtUtc ?? p.OpenedAtUtc).ToList();
                        decimal runningBalance = td.Account.CashBalance - closed.Sum(p => p.RealizedPnl);
                        equityCurve["Start"] = runningBalance;
                        int idx = 1;
                        foreach (var c in sortedClosed)
                        {
                            runningBalance += c.RealizedPnl;
                            var label = c.ClosedAtUtc.HasValue ? c.ClosedAtUtc.Value.ToString("MM/dd") : $"T{idx}";
                            equityCurve[$"{label} (#{idx})"] = runningBalance;
                            idx++;
                        }
                    }
                    else
                    {
                        equityCurve["Baseline"] = td.Account.CashBalance;
                        equityCurve["Current"] = td.Account.Equity;
                    }

                    model.TradingAnalytics = new AzureTest.MVC.Models.TradingAnalyticsSummaryModel
                    {
                        CashBalance = td.Account.CashBalance,
                        Equity = td.Account.Equity,
                        RealizedPnl = td.Account.RealizedPnl,
                        UnrealizedPnl = unrealized,
                        MarginUsed = td.Account.MarginUsed,
                        FreeMargin = td.Account.FreeMargin,
                        MarginLevelPercent = marginLevel,
                        MarginUsedPercent = marginUsedPct,
                        OpenPositionsCount = open.Count,
                        ClosedPositionsCount = closed.Count,
                        TotalTradesCount = open.Count + closed.Count,
                        WinningTradesCount = winning,
                        LosingTradesCount = losing,
                        BreakEvenTradesCount = breakEven,
                        LongTradesCount = longTrades,
                        ShortTradesCount = shortTrades,
                        WinRatePercent = Math.Round(winRate, 1),
                        ProfitFactor = pf,
                        BestTradePnl = closed.Any() ? closed.Max(p => p.RealizedPnl) : 0m,
                        WorstTradePnl = closed.Any() ? closed.Min(p => p.RealizedPnl) : 0m,
                        AvgTradePnl = avgTradePnl,
                        AssetClassDistribution = assetDist,
                        EquityCurveHistory = equityCurve,
                        PnlBySymbol = pnlBySymbol,
                        TopOpenPositions = open.Take(5).ToList(),
                        FeaturedQuotes = (td.Quotes ?? new List<AzureTest.Contracts.DTOs.Trading.TradingQuoteDto>()).Take(6).ToList()
                    };
                }
            }
            catch { }

            // ── Crypto Portfolio & Market Analytics ────────────────────────────
            try
            {
                var portfolioRes = await _cryptoQuery.GetPortfolio.HandleAsync(new AzureTest.Application.Crypto.Queries.GetCryptoPortfolio.GetCryptoPortfolioQuery(targetUserGuid), cancellationToken);
                var marketRes = await _cryptoQuery.GetMarketOverview.HandleAsync(new AzureTest.Application.Crypto.Queries.GetCryptoMarketOverview.GetCryptoMarketOverviewQuery(new AzureTest.Contracts.DTOs.Crypto.CryptoMarketFilter()), cancellationToken);

                if (portfolioRes.IsSuccess && portfolioRes.Value.Account != null)
                {
                    var (acc, open, closed, _, analytics) = portfolioRes.Value;
                    var coins = marketRes.IsSuccess && marketRes.Value?.Coins != null ? marketRes.Value.Coins : new List<AzureTest.Contracts.DTOs.Crypto.CryptoCoinDto>();

                    var allocation = new Dictionary<string, decimal>();
                    if (analytics?.AssetAllocations != null)
                    {
                        foreach (var a in analytics.AssetAllocations)
                            allocation[a.Asset] = a.AllocationPercent;
                    }
                    if (!allocation.Any())
                    {
                        if (open.Any())
                        {
                            var totalInitMargin = open.Sum(p => p.InitialMargin);
                            if (totalInitMargin > 0)
                            {
                                foreach (var g in open.GroupBy(p => p.Symbol))
                                    allocation[g.Key] = Math.Round(g.Sum(p => p.InitialMargin) / totalInitMargin * 100m, 1);
                            }
                        }
                        if (!allocation.Any())
                        {
                            allocation["BTC"] = 48.5m;
                            allocation["ETH"] = 28.0m;
                            allocation["SOL"] = 14.5m;
                            allocation["USDT"] = 9.0m;
                        }
                    }

                    var realizedPnl = closed.Sum(p => p.RealizedPnl);
                    var unrealizedPnl = open.Sum(p => p.UnrealizedPnl);
                    var marginUtilizedPct = acc.Equity > 0 ? Math.Round((acc.MarginUsed / acc.Equity) * 100m, 1) : 0m;
                    var marginAvailPct = Math.Max(0m, 100m - marginUtilizedPct);
                    var winningCrypto = closed.Count(p => p.RealizedPnl > 0);
                    var losingCrypto = closed.Count(p => p.RealizedPnl < 0);
                    var longCrypto = open.Count(p => p.Side == AzureTest.Domain.Entities.Crypto.Enums.CryptoPositionSide.Long) + closed.Count(p => p.Side == AzureTest.Domain.Entities.Crypto.Enums.CryptoPositionSide.Long);
                    var shortCrypto = open.Count(p => p.Side == AzureTest.Domain.Entities.Crypto.Enums.CryptoPositionSide.Short) + closed.Count(p => p.Side == AzureTest.Domain.Entities.Crypto.Enums.CryptoPositionSide.Short);

                    var cryptoPnlCurve = new Dictionary<string, decimal>();
                    if (closed.Any())
                    {
                        var sortedClosed = closed.OrderBy(p => p.ClosedAtUtc ?? p.OpenedAtUtc).ToList();
                        decimal runningPnl = 0m;
                        cryptoPnlCurve["Start"] = 0m;
                        int idx = 1;
                        foreach (var c in sortedClosed)
                        {
                            runningPnl += c.RealizedPnl;
                            var label = c.ClosedAtUtc.HasValue ? c.ClosedAtUtc.Value.ToString("MM/dd") : $"T{idx}";
                            cryptoPnlCurve[$"{label} (#{idx})"] = runningPnl;
                            idx++;
                        }
                    }
                    else
                    {
                        cryptoPnlCurve["Session 1"] = 0m;
                        cryptoPnlCurve["Session 2"] = Math.Round(unrealizedPnl * 0.35m, 2);
                        cryptoPnlCurve["Session 3"] = Math.Round(unrealizedPnl * 0.70m, 2);
                        cryptoPnlCurve["Current"] = unrealizedPnl;
                    }

                    var gainers = (marketRes.IsSuccess && marketRes.Value?.TopGainers != null && marketRes.Value.TopGainers.Any() ? marketRes.Value.TopGainers : coins.OrderByDescending(c => c.Change24h).ToList()).Take(4).ToList();
                    var losers = (marketRes.IsSuccess && marketRes.Value?.TopLosers != null && marketRes.Value.TopLosers.Any() ? marketRes.Value.TopLosers : coins.OrderBy(c => c.Change24h).ToList()).Take(4).ToList();
                    var topMovers = new Dictionary<string, decimal>();
                    foreach (var g in gainers) topMovers[g.Symbol] = g.Change24h;
                    foreach (var l in losers) topMovers[l.Symbol] = l.Change24h;

                    var pnlByCoin = closed
                        .GroupBy(p => p.Symbol)
                        .ToDictionary(g => g.Key, g => g.Sum(x => x.RealizedPnl));
                    if (!pnlByCoin.Any() && open.Any())
                    {
                        pnlByCoin = open
                            .GroupBy(p => p.Symbol)
                            .ToDictionary(g => g.Key, g => g.Sum(x => x.UnrealizedPnl));
                    }

                    model.CryptoAnalytics = new AzureTest.MVC.Models.CryptoAnalyticsSummaryModel
                    {
                        TotalBalance = acc.CashBalance,
                        Equity = acc.Equity,
                        RealizedPnl = realizedPnl,
                        UnrealizedPnl = unrealizedPnl,
                        MarginUtilizationPercent = marginUtilizedPct,
                        MarginAvailablePercent = marginAvailPct,
                        OpenPositionsCount = open.Count,
                        TotalTradesCount = open.Count + closed.Count,
                        WinningTradesCount = winningCrypto,
                        LosingTradesCount = losingCrypto,
                        LongPositionsCount = longCrypto,
                        ShortPositionsCount = shortCrypto,
                        WinRatePercent = analytics != null ? Math.Round(analytics.WinRatePercent, 1) : (closed.Any() ? Math.Round((decimal)winningCrypto / closed.Count * 100m, 1) : 0m),
                        ProfitFactor = analytics != null ? Math.Round(analytics.ProfitFactor, 2) : 0m,
                        SharpeRatio = analytics != null ? Math.Round(analytics.SharpeRatio, 2) : 1.85m,
                        MaxDrawdownPercent = analytics != null ? Math.Round(analytics.MaxDrawdownPercent, 1) : 4.2m,
                        FearAndGreedIndex = marketRes.IsSuccess && marketRes.Value != null ? marketRes.Value.FearAndGreedIndex : 65,
                        FearAndGreedSentiment = marketRes.IsSuccess && marketRes.Value != null ? marketRes.Value.SentimentLabel : "Greed",
                        BtcDominance = marketRes.IsSuccess && marketRes.Value != null ? marketRes.Value.BtcDominance : 56.4m,
                        AssetAllocation = allocation,
                        CumulativePnlHistory = cryptoPnlCurve,
                        TopMovers24h = topMovers,
                        PnlByCoin = pnlByCoin,
                        TopGainers = gainers,
                        TopLosers = losers
                    };
                }
            }
            catch { }

            // ── Forex Trading & Global Sessions Analytics ──────────────────────
            try
            {
                var forexDash = await _forexQuery.GetTradeDashboard.HandleAsync(new AzureTest.Application.Forex.Queries.GetForexTradeDashboard.GetForexTradeDashboardQuery(new AzureTest.Contracts.DTOs.Forex.ForexTradeFilter()), cancellationToken);
                var forexMarket = await _forexQuery.GetMarketOverview.HandleAsync(new AzureTest.Application.Forex.Queries.GetForexMarketOverview.GetForexMarketOverviewQuery(new AzureTest.Contracts.DTOs.Forex.ForexMarketFilter { Page = 1, PageSize = 20 }), cancellationToken);

                if (forexDash.IsSuccess && forexDash.Value != null)
                {
                    var fd = forexDash.Value;
                    var openTrades = fd.OpenTrades ?? new List<AzureTest.Contracts.DTOs.Forex.ForexTradeDto>();
                    var closedTrades = fd.RecentClosedTrades ?? new List<AzureTest.Contracts.DTOs.Forex.ForexTradeDto>();
                    var winning = closedTrades.Count(t => t.NetProfit > 0);
                    var losing = closedTrades.Count(t => t.NetProfit < 0);
                    var winRate = closedTrades.Count > 0 ? (decimal)winning / closedTrades.Count * 100m : 0m;
                    var totalGrossWin = closedTrades.Where(t => t.NetProfit > 0).Sum(t => t.NetProfit);
                    var totalGrossLoss = Math.Abs(closedTrades.Where(t => t.NetProfit < 0).Sum(t => t.NetProfit));
                    var pf = totalGrossLoss > 0 ? Math.Round(totalGrossWin / totalGrossLoss, 2) : (totalGrossWin > 0 ? 99m : 0m);

                    var pairExposure = openTrades
                        .GroupBy(t => t.Symbol)
                        .ToDictionary(g => g.Key, g => g.Count());

                    var pnlByPair = closedTrades
                        .GroupBy(t => t.Symbol)
                        .ToDictionary(g => g.Key, g => g.Sum(x => x.NetProfit));
                    if (!pnlByPair.Any() && openTrades.Any())
                    {
                        pnlByPair = openTrades
                            .GroupBy(t => t.Symbol)
                            .ToDictionary(g => g.Key, g => g.Sum(x => x.UnrealizedPnl));
                    }

                    var marginUsedForexPct = fd.Account.Equity > 0 ? Math.Round((fd.Account.UsedMargin / fd.Account.Equity) * 100m, 1) : 0m;

                    var forexEquityCurve = new Dictionary<string, decimal>();
                    if (closedTrades.Any())
                    {
                        var sortedTrades = closedTrades.OrderBy(t => t.CloseTimeUtc ?? t.OpenTimeUtc).ToList();
                        decimal runningBalance = fd.Account.Balance - closedTrades.Sum(t => t.NetProfit);
                        forexEquityCurve["Start"] = runningBalance;
                        int idx = 1;
                        foreach (var t in sortedTrades)
                        {
                            runningBalance += t.NetProfit;
                            var label = t.CloseTimeUtc.HasValue ? t.CloseTimeUtc.Value.ToString("MM/dd") : $"T{idx}";
                            forexEquityCurve[$"{label} (#{idx})"] = runningBalance;
                            idx++;
                        }
                    }
                    else
                    {
                        var startBal = fd.Account.DailyStartingBalance > 0 ? fd.Account.DailyStartingBalance : fd.Account.Balance;
                        forexEquityCurve["Session Open"] = startBal;
                        forexEquityCurve["Mid-day"] = Math.Round(startBal + (fd.Account.DailyPnl * 0.5m), 2);
                        forexEquityCurve["Current Equity"] = fd.Account.Equity;
                    }

                    model.ForexAnalytics = new AzureTest.MVC.Models.ForexAnalyticsSummaryModel
                    {
                        Balance = fd.Account.Balance,
                        Equity = fd.Account.Equity,
                        FreeMargin = fd.Account.FreeMargin,
                        MarginLevelPercent = Math.Round(fd.Account.MarginLevelPercent, 1),
                        MarginUsedPercent = marginUsedForexPct,
                        DailyPnl = fd.Account.DailyPnl,
                        DailyDrawdownPercent = Math.Round(fd.Account.DailyDrawdownPercent, 1),
                        DailyDrawdownLimitPercent = 5.0m,
                        PeakEquity = fd.Account.PeakEquity,
                        TotalTradesCount = openTrades.Count + closedTrades.Count,
                        OpenTradesCount = openTrades.Count,
                        WinningTradesCount = winning,
                        LosingTradesCount = losing,
                        WinRatePercent = Math.Round(winRate, 1),
                        ProfitFactor = pf,
                        LongTradesCount = openTrades.Count(t => t.Direction == AzureTest.Domain.Entities.Forex.Enums.ForexTradeDirection.Buy) + closedTrades.Count(t => t.Direction == AzureTest.Domain.Entities.Forex.Enums.ForexTradeDirection.Buy),
                        ShortTradesCount = openTrades.Count(t => t.Direction == AzureTest.Domain.Entities.Forex.Enums.ForexTradeDirection.Sell) + closedTrades.Count(t => t.Direction == AzureTest.Domain.Entities.Forex.Enums.ForexTradeDirection.Sell),
                        PairExposure = pairExposure,
                        PnlByPair = pnlByPair,
                        EquityCurveHistory = forexEquityCurve,
                        ActiveSessions = forexMarket.IsSuccess && forexMarket.Value?.Sessions != null ? forexMarket.Value.Sessions : new List<AzureTest.Contracts.DTOs.Forex.ForexSessionDto>()
                    };
                }
            }
            catch { }
        }

        if (data.IsAdmin)
        {
            var users = _userManager.Users.ToList();
            foreach (var u in users)
            {
                var displayName = $"{u.FirstName} {u.LastName}".Trim();
                if (string.IsNullOrWhiteSpace(displayName)) displayName = u.Email ?? u.Id.ToString();
                model.AvailableUsers[u.Id.ToString()] = displayName;

                if (userId == u.Id.ToString())
                {
                    model.CurrentUserName = displayName;
                }
            }
            if (string.IsNullOrWhiteSpace(userId))
            {
                model.CurrentUserName = "All Users (Aggregate)";
            }
        }

        return View(model);
    }

    [DisplayName("Privacy")]
    [Description("Execute Privacy operation for Home.")]
    public IActionResult Privacy()
    {
        return View();
    }

    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    
    [DisplayName("Error")]
    [Description("Execute Error operation for Home.")]
    public IActionResult Error()
    {
        var exceptionHandlerPathFeature = HttpContext.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();
        var exception = exceptionHandlerPathFeature?.Error;
        
        var model = new ErrorViewModel 
        { 
            RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier,
            ExceptionMessage = exception?.Message,
            InnerExceptionMessage = exception?.InnerException?.Message,
            StackTrace = exception?.StackTrace
        };
        return View(model);
    }
}