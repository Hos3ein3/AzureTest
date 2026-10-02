

using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Domain.Documents;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Domain.Entities;
using AzureTest.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

using AzureTest.Persistence.Seeding;

namespace AzureTest.Infrastructure.Seeding;

public class IdentityDataSeeder
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly INoSqlUnitOfWork _noSqlUnitOfWork;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<IdentityDataSeeder> _logger;

    public IdentityDataSeeder(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        INoSqlUnitOfWork noSqlUnitOfWork,
        IUnitOfWork unitOfWork,
        ILogger<IdentityDataSeeder> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _noSqlUnitOfWork = noSqlUnitOfWork;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        try
        {
            var systemRoleName = "system";
            var adminRoleName = "Admin";
            var userRoleName = "User";

            if (!await _roleManager.RoleExistsAsync(systemRoleName))
                await _roleManager.CreateAsync(new ApplicationRole { Name = systemRoleName });

            if (!await _roleManager.RoleExistsAsync(adminRoleName))
                await _roleManager.CreateAsync(new ApplicationRole { Name = adminRoleName });

            if (!await _roleManager.RoleExistsAsync(userRoleName))
                await _roleManager.CreateAsync(new ApplicationRole { Name = userRoleName });

            // Create system user
            var systemUserEmail = "system@azuretest.local";
            if (await _userManager.FindByEmailAsync(systemUserEmail) == null)
            {
                var systemUser = new ApplicationUser
                {
                    UserName = systemUserEmail,
                    Email = systemUserEmail,
                    FirstName = "System",
                    LastName = "Account",
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(systemUser, "System!123");
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(systemUser, systemRoleName);
                }
            }

            // Create admin user
            var adminUserEmail = "admin@azuretest.local";
            if (await _userManager.FindByEmailAsync(adminUserEmail) == null)
            {
                var adminUser = new ApplicationUser
                {
                    UserName = adminUserEmail,
                    Email = adminUserEmail,
                    FirstName = "Admin",
                    LastName = "User",
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(adminUser, "123456");
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(adminUser, adminRoleName);
                    
                }
            }
            
            // Create support user
            var supportUserEmail = "support@azuretest.local";
            if (await _userManager.FindByEmailAsync(supportUserEmail) == null)
            {
                var supportUser = new ApplicationUser
                {
                    UserName = supportUserEmail,
                    Email = supportUserEmail,
                    FirstName = "Support",
                    LastName = "Staff",
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(supportUser, "123456");
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(supportUser, "User");
                }
            }
var userUserEmail = "mohammadhosein.behzadifard@gmail.com";
            var userUser = await _userManager.FindByEmailAsync(userUserEmail);
            if (userUser == null)
            {
                userUser = new ApplicationUser
                {
                    UserName = userUserEmail,
                    Email = userUserEmail,
                    FirstName = "Mohammad Hosein",
                    LastName = "Behzadifard",
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(userUser, "123456");
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(userUser, userRoleName);
                }
            }
            
            // Seed Dashboard components for this user
            if (userUser != null)
            {
                await SeedDashboardComponentsAsync(userUser.Id);
                
            }
            var existingComps = await _noSqlUnitOfWork.DashboardComponents.GetAllAsync(default);
            if (!existingComps.Any())
            {
                var dashboardComponents = FakeDataGenerator.BuildDashboardComponents();
                foreach (var dc in dashboardComponents) await _noSqlUnitOfWork.DashboardComponents.AddAsync(dc);
            }
            
            // Hotfix: update existing 'Current Time' widget if it uses old HTML
            var rawComps = await _noSqlUnitOfWork.DashboardRawComponents.GetAllAsync(default);
            var currentTimeWidget = System.Linq.Enumerable.FirstOrDefault(rawComps, x => x.Name == "Current Time");
            if (currentTimeWidget != null && currentTimeWidget.HtmlContent != null && currentTimeWidget.HtmlContent.Contains("text-3xl font-mono text-slate-800"))
            {
                currentTimeWidget.HtmlContent = @"<div class=""h-full w-full bg-slate-900 rounded-xl flex items-center justify-center flex-col shadow-inner""><div class=""text-xs text-slate-400 font-semibold uppercase tracking-widest mb-2"">Local Time</div><h2 id=""clock"" class=""text-4xl font-mono text-white font-bold tracking-tight""></h2></div><script>setInterval(()=>document.getElementById('clock').innerText=new Date().toLocaleTimeString(), 1000);</script>";
                await _noSqlUnitOfWork.DashboardRawComponents.UpdateAsync(currentTimeWidget.Id, currentTimeWidget, default);
            }
            
            _logger.LogInformation("Identity data seeded successfully.");
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Error while seeding Identity data.");
        }
    }
    
    private async Task SeedDashboardComponentsAsync(System.Guid userId)
    {
        var existingDashboard = await _noSqlUnitOfWork.UserDashboards.GetByUserIdAndSlugAsync(userId, "dynamic-analytics");
        if (existingDashboard != null)
        {
            // Already seeded
            return;
        }

        var mainDashboard = new UserDashboard
        {
            Id = Guid.CreateVersion7(),
            UserId = userId,
            Name = "Dynamic Analytics",
            Slug = "dynamic-analytics"
        };
        await _noSqlUnitOfWork.UserDashboards.AddAsync(mainDashboard);

        // JSON Configs for Real Data via AnalyticsEndpointController
        var jsonConfigs = new[]
        {
            new { Name = "Status Breakdown", Url = "/api/analyticsendpoint/status-counts", Type = "doughnut", Color = "#3b82f6" },
            new { Name = "Applications By Date", Url = "/api/analyticsendpoint/applications-by-date", Type = "line", Color = "#10b981" },
            new { Name = "Top Positions", Url = "/api/analyticsendpoint/position-counts", Type = "bar", Color = "#8b5cf6" },
            new { Name = "Top Locations", Url = "/api/analyticsendpoint/location-counts", Type = "bar", Color = "#f59e0b" },
            new { Name = "Application Sources", Url = "/api/analyticsendpoint/source-counts", Type = "pie", Color = "#ec4899" },
            new { Name = "Expenses by Category", Url = "/api/analyticsendpoint/expenses-by-category", Type = "doughnut", Color = "#f43f5e" },
            new { Name = "Debts by Person", Url = "/api/analyticsendpoint/debts-by-person", Type = "bar", Color = "#14b8a6" },
            new { Name = "Ticket Status", Url = "/api/analyticsendpoint/ticket-status-counts", Type = "doughnut", Color = "#0284c7" },
            new { Name = "Habit Adherence", Url = "/api/analyticsendpoint/habit-adherence", Type = "pie", Color = "#10b981" },
            new { Name = "Monthly Expenses", Url = "/api/analyticsendpoint/monthly-expenses", Type = "line", Color = "#f43f5e" },
            new { Name = "Monthly Debts", Url = "/api/analyticsendpoint/monthly-debts", Type = "line", Color = "#8b5cf6" },
            // KPIs
            new { Name = "Total Applications", Url = "/api/analyticsendpoint/total-applications", Type = "kpi", Color = "#3b82f6" },
            new { Name = "Active Processing", Url = "/api/analyticsendpoint/active-processing", Type = "kpi", Color = "#10b981" },
            new { Name = "Interview Rate", Url = "/api/analyticsendpoint/interview-rate", Type = "kpi", Color = "#f59e0b" },
            new { Name = "Total Companies", Url = "/api/analyticsendpoint/total-companies", Type = "kpi", Color = "#6366f1" },
            new { Name = "Registered Users", Url = "/api/analyticsendpoint/registered-users", Type = "kpi", Color = "#ec4899" },
            new { Name = "Occupied Storage", Url = "/api/analyticsendpoint/occupied-storage", Type = "kpi", Color = "#8b5cf6" },
            new { Name = "Outstanding Debt", Url = "/api/analyticsendpoint/outstanding-debt", Type = "kpi", Color = "#ef4444" },
            new { Name = "Expenses This Month", Url = "/api/analyticsendpoint/expenses-this-month", Type = "kpi", Color = "#f97316" },
            new { Name = "Unanswered Tickets", Url = "/api/analyticsendpoint/unanswered-tickets", Type = "kpi", Color = "#0ea5e9" },
            // Lists
            new { Name = "Pending Tasks", Url = "/api/analyticsendpoint/pending-tasks", Type = "list", Color = "#3b82f6" },
            new { Name = "Upcoming Events", Url = "/api/analyticsendpoint/upcoming-events", Type = "list", Color = "#10b981" },
            new { Name = "Daily Habits", Url = "/api/analyticsendpoint/daily-habits", Type = "list", Color = "#f59e0b" },
            new { Name = "Recent Debts", Url = "/api/analyticsendpoint/recent-debts", Type = "list", Color = "#ef4444" },
            new { Name = "Recent Expenses", Url = "/api/analyticsendpoint/recent-expenses", Type = "list", Color = "#ec4899" },
            new { Name = "Debts Due Soon", Url = "/api/analyticsendpoint/debts-due-soon", Type = "list", Color = "#f43f5e" },
            new { Name = "Recent Activity", Url = "/api/analyticsendpoint/recent-activity", Type = "list", Color = "#6366f1" }
        };

        foreach (var j in jsonConfigs)
        {
            await _noSqlUnitOfWork.DashboardRawComponents.AddAsync(new DashboardRawComponent
            {
                Name = j.Name,
                Type = "JsonConfig",
                Category = "Analytics",
                JsonConfiguration = $$"""
                {
                  "title": "{{j.Name}}",
                  "chartType": "{{j.Type}}",
                  "themeColor": "{{j.Color}}",
                  "endpointUrl": "{{j.Url}}"
                }
                """
            });
        }

        // 10 HTML Raw Configs
        var htmlConfigs = new[]
        {
            ("System Status", @"<div class=""p-4 bg-green-50 rounded text-green-800""><h3 class=""font-bold"">All Systems Operational</h3><p class=""text-sm mt-1"">Last checked: Just now</p></div>"),
            ("Quick Note", @"<div class=""p-4""><textarea class=""w-full h-full p-2 border rounded"" placeholder=""Write a quick note here...""></textarea></div>"),
            ("Current Time", @"<div class=""h-full w-full bg-slate-900 rounded-xl flex items-center justify-center flex-col shadow-inner""><div class=""text-xs text-slate-400 font-semibold uppercase tracking-widest mb-2"">Local Time</div><h2 id=""clock"" class=""text-4xl font-mono text-white font-bold tracking-tight""></h2></div><script>setInterval(()=>document.getElementById('clock').innerText=new Date().toLocaleTimeString(), 1000);</script>"),
            ("Quote of the Day", @"<div class=""p-6 bg-slate-50 italic text-slate-600 border-l-4 border-slate-400"">""The only way to do great work is to love what you do."" - Steve Jobs</div>"),
            ("Weather Widget", @"<div class=""p-4 flex items-center gap-4""><i class=""fa-solid fa-sun text-4xl text-yellow-500""></i><div><h3 class=""font-bold text-xl"">24°C</h3><p class=""text-slate-500"">Sunny, Amsterdam</p></div></div>"),
            ("Goal Progress", @"<div class=""p-4""><div class=""flex justify-between mb-1""><span>Monthly Revenue</span><span>75%</span></div><div class=""w-full bg-slate-200 rounded-full h-2.5""><div class=""bg-brand-600 h-2.5 rounded-full"" style=""width: 75%""></div></div></div>"),
            ("Useful Links", @"<ul class=""p-4 space-y-2 text-brand-600""><li><a href=""#"" class=""hover:underline"">Documentation</a></li><li><a href=""#"" class=""hover:underline"">Support Portal</a></li><li><a href=""#"" class=""hover:underline"">API Reference</a></li></ul>"),
            ("Currency Rates", @"<div class=""p-4""><table class=""w-full text-sm text-left text-slate-500""><thead class=""text-xs text-slate-700 uppercase bg-slate-50""><tr><th>Currency</th><th>Rate</th></tr></thead><tbody><tr><td>USD/EUR</td><td>0.92</td></tr><tr><td>GBP/EUR</td><td>1.17</td></tr></tbody></table></div>"),
            ("Welcome Banner", @"<div class=""p-6 bg-gradient-to-r from-brand-500 to-purple-600 text-white rounded shadow""><h2 class=""text-2xl font-bold mb-2"">Welcome back!</h2><p>Here is what's happening with your projects today.</p></div>"),
            ("Stock Ticker", @"<div class=""p-4 flex gap-4 overflow-hidden whitespace-nowrap bg-slate-900 text-green-400 font-mono text-sm""><span>AAPL 180.95 ▲</span><span>MSFT 415.50 ▲</span><span>GOOGL 142.65 ▼</span></div>")
        };

        foreach (var h in htmlConfigs)
        {
            await _noSqlUnitOfWork.DashboardRawComponents.AddAsync(new DashboardRawComponent
            {
                Name = h.Item1,
                Type = "RawHtml",
                Category = "Widgets",
                HtmlContent = h.Item2
            });
        }
        
        await _noSqlUnitOfWork.SaveChangesAsync();
    }
}
