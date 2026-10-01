using AzureTest.Application.Abstractions;


using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.MVC.Services;
using System.Collections.Generic;

namespace AzureTest.MVC.Controllers.Api
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AnalyticsEndpointController : ControllerBase
    {
        private readonly IDashboardDataLoader _dataLoader;

        public AnalyticsEndpointController(IDashboardDataLoader dataLoader)
        {
            _dataLoader = dataLoader;
        }

        [HttpGet("status-counts")]
        public async Task<IActionResult> GetStatusCounts(CancellationToken cancellationToken)
        {
            var data = await _dataLoader.LoadDataAsync(User, null, cancellationToken);
            var result = data.Analytics.StatusCounts.Select(k => new { title = k.Key, value = k.Value });
            return Ok(result);
        }

        [HttpGet("applications-by-date")]
        public async Task<IActionResult> GetApplicationsByDate(CancellationToken cancellationToken)
        {
            var data = await _dataLoader.LoadDataAsync(User, null, cancellationToken);
            var result = data.Analytics.ApplicationsByDate.Select(k => new { title = k.Key, value = k.Value });
            return Ok(result);
        }

        [HttpGet("position-counts")]
        public async Task<IActionResult> GetPositionCounts(CancellationToken cancellationToken)
        {
            var data = await _dataLoader.LoadDataAsync(User, null, cancellationToken);
            var result = data.Analytics.PositionCounts.Select(k => new { title = k.Key, value = k.Value });
            return Ok(result);
        }

        [HttpGet("location-counts")]
        public async Task<IActionResult> GetLocationCounts(CancellationToken cancellationToken)
        {
            var data = await _dataLoader.LoadDataAsync(User, null, cancellationToken);
            var result = data.Analytics.LocationCounts.Select(k => new { title = k.Key, value = k.Value });
            return Ok(result);
        }

        [HttpGet("source-counts")]
        public async Task<IActionResult> GetSourceCounts(CancellationToken cancellationToken)
        {
            var data = await _dataLoader.LoadDataAsync(User, null, cancellationToken);
            var result = data.Analytics.SourceCounts.Select(k => new { title = k.Key, value = k.Value });
            return Ok(result);
        }

        [HttpGet("expenses-by-category")]
        public async Task<IActionResult> GetExpensesByCategory(CancellationToken cancellationToken)
        {
            var data = await _dataLoader.LoadDataAsync(User, null, cancellationToken);
            var result = data.ExpensesByCategory.Select(k => new { title = k.Key, value = k.Value });
            return Ok(result);
        }

        [HttpGet("debts-by-person")]
        public async Task<IActionResult> GetDebtsByPerson(CancellationToken cancellationToken)
        {
            var data = await _dataLoader.LoadDataAsync(User, null, cancellationToken);
            var result = data.DebtsByPerson.Select(k => new { title = k.Key, value = k.Value });
            return Ok(result);
        }

        [HttpGet("ticket-status-counts")]
        public async Task<IActionResult> GetTicketStatusCounts(CancellationToken cancellationToken)
        {
            var data = await _dataLoader.LoadDataAsync(User, null, cancellationToken);
            var result = data.TicketStatusCounts.Select(k => new { title = k.Key, value = k.Value });
            return Ok(result);
        }

        [HttpGet("habit-adherence")]
        public async Task<IActionResult> GetHabitAdherence(CancellationToken cancellationToken)
        {
            var data = await _dataLoader.LoadDataAsync(User, null, cancellationToken);
            var result = data.HabitAdherence.Select(k => new { title = k.Key, value = k.Value });
            return Ok(result);
        }

        [HttpGet("monthly-expenses")]
        public async Task<IActionResult> GetMonthlyExpenses(CancellationToken cancellationToken)
        {
            var data = await _dataLoader.LoadDataAsync(User, null, cancellationToken);
            var result = data.MonthlyExpenses.Select(k => new { title = k.Key, value = k.Value });
            return Ok(result);
        }
        
        [HttpGet("monthly-debts")]
        public async Task<IActionResult> GetMonthlyDebts(CancellationToken cancellationToken)
        {
            var data = await _dataLoader.LoadDataAsync(User, null, cancellationToken);
            var result = data.MonthlyDebts.Select(k => new { title = k.Key, value = k.Value });
            return Ok(result);
        }

        // KPIs
        [HttpGet("total-applications")]
        public async Task<IActionResult> GetTotalApplications(CancellationToken cancellationToken)
        {
            var data = await _dataLoader.LoadDataAsync(User, null, cancellationToken);
            return Ok(new { value = data.Analytics.TotalApplications });
        }

        [HttpGet("active-processing")]
        public async Task<IActionResult> GetActiveProcessing(CancellationToken cancellationToken)
        {
            var data = await _dataLoader.LoadDataAsync(User, null, cancellationToken);
            return Ok(new { value = data.Analytics.ActiveApplications });
        }

        [HttpGet("interview-rate")]
        public async Task<IActionResult> GetInterviewRate(CancellationToken cancellationToken)
        {
            var data = await _dataLoader.LoadDataAsync(User, null, cancellationToken);
            return Ok(new { value = data.Analytics.InterviewRate.ToString("0.0") + "%" });
        }

        [HttpGet("total-companies")]
        public async Task<IActionResult> GetTotalCompanies(CancellationToken cancellationToken)
        {
            var data = await _dataLoader.LoadDataAsync(User, null, cancellationToken);
            return Ok(new { value = data.Analytics.TotalCompanies });
        }

        [HttpGet("registered-users")]
        public async Task<IActionResult> GetRegisteredUsers(CancellationToken cancellationToken)
        {
            var data = await _dataLoader.LoadDataAsync(User, null, cancellationToken);
            return Ok(new { value = data.Analytics.TotalUsers });
        }

        [HttpGet("occupied-storage")]
        public async Task<IActionResult> GetOccupiedStorage(CancellationToken cancellationToken)
        {
            var data = await _dataLoader.LoadDataAsync(User, null, cancellationToken);
            return Ok(new { value = data.Analytics.OccupiedStorage });
        }

        [HttpGet("outstanding-debt")]
        public async Task<IActionResult> GetOutstandingDebt(CancellationToken cancellationToken)
        {
            var data = await _dataLoader.LoadDataAsync(User, null, cancellationToken);
            return Ok(new { value = "$" + data.TotalOutstandingDebt });
        }

        [HttpGet("expenses-this-month")]
        public async Task<IActionResult> GetExpensesThisMonth(CancellationToken cancellationToken)
        {
            var data = await _dataLoader.LoadDataAsync(User, null, cancellationToken);
            return Ok(new { value = "$" + data.TotalExpensesThisMonth });
        }

        [HttpGet("unanswered-tickets")]
        public async Task<IActionResult> GetUnansweredTickets(CancellationToken cancellationToken)
        {
            var data = await _dataLoader.LoadDataAsync(User, null, cancellationToken);
            return Ok(new { value = data.Tickets.Count });
        }

        // Lists
        [HttpGet("pending-tasks")]
        public async Task<IActionResult> GetPendingTasks(CancellationToken cancellationToken)
        {
            var data = await _dataLoader.LoadDataAsync(User, null, cancellationToken);
            var result = data.PendingTasks.Select(t => new { title = t.Title, subtitle = t.EndDate.HasValue ? t.EndDate.Value.ToString("MMM dd") : "No due date" });
            return Ok(result);
        }

        [HttpGet("upcoming-events")]
        public async Task<IActionResult> GetUpcomingEvents(CancellationToken cancellationToken)
        {
            var data = await _dataLoader.LoadDataAsync(User, null, cancellationToken);
            var result = data.RecentEvents.Select(e => new { title = e.Title, subtitle = e.StartDate.ToString("MMM dd") });
            return Ok(result);
        }

        [HttpGet("daily-habits")]
        public async Task<IActionResult> GetDailyHabits(CancellationToken cancellationToken)
        {
            var data = await _dataLoader.LoadDataAsync(User, null, cancellationToken);
            var result = data.RecentHabits.Select(h => new { title = h.Name, subtitle = h.Frequency.ToString() });
            return Ok(result);
        }

        [HttpGet("recent-debts")]
        public async Task<IActionResult> GetRecentDebts(CancellationToken cancellationToken)
        {
            var data = await _dataLoader.LoadDataAsync(User, null, cancellationToken);
            var result = data.RecentDebts.Select(d => new { title = d.DebtPersonName, subtitle = "$" + d.Amount });
            return Ok(result);
        }

        [HttpGet("recent-expenses")]
        public async Task<IActionResult> GetRecentExpenses(CancellationToken cancellationToken)
        {
            var data = await _dataLoader.LoadDataAsync(User, null, cancellationToken);
            var result = data.RecentExpenses.Select(e => new { title = e.Description, subtitle = "$" + e.Amount });
            return Ok(result);
        }

        [HttpGet("debts-due-soon")]
        public async Task<IActionResult> GetDebtsDueSoon(CancellationToken cancellationToken)
        {
            var data = await _dataLoader.LoadDataAsync(User, null, cancellationToken);
            var result = data.PromissedDebts.Select(d => new { title = d.DebtPersonName, subtitle = "Due: " + d.PromissedReturnDate?.ToString("MMM dd") + " ($" + d.Amount + ")" });
            return Ok(result);
        }

        [HttpGet("recent-activity")]
        public async Task<IActionResult> GetRecentActivity(CancellationToken cancellationToken)
        {
            var data = await _dataLoader.LoadDataAsync(User, null, cancellationToken);
            var result = data.Analytics.RecentActivities.Select(a => new { title = a.PositionTitle, subtitle = a.CompanyName + " - " + a.Status });
            return Ok(result);
        }
    }
}
