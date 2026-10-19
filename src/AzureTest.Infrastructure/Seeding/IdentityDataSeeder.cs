using Microsoft.Extensions.Configuration;


using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Domain.Documents;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Domain.Entities;
using AzureTest.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

using AzureTest.Persistence.Seeding;

namespace AzureTest.Infrastructure.Seeding;

public class IdentityDataSeeder
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly INoSqlUnitOfWork _noSqlUnitOfWork;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<IdentityDataSeeder> _logger;
    private readonly Microsoft.Extensions.Options.IOptions<AzureTest.Infrastructure.Configuration.DashboardSettings> _dashboardSettings;
    private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;

    public IdentityDataSeeder(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        INoSqlUnitOfWork noSqlUnitOfWork,
        IUnitOfWork unitOfWork,
        ILogger<IdentityDataSeeder> logger,
        Microsoft.Extensions.Options.IOptions<AzureTest.Infrastructure.Configuration.DashboardSettings> dashboardSettings, Microsoft.Extensions.Configuration.IConfiguration configuration)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _noSqlUnitOfWork = noSqlUnitOfWork;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _dashboardSettings = dashboardSettings;
        _configuration = configuration;
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

            // Assign Dashboard reserved slugs permissions to User role
            var userRole = await _roleManager.FindByNameAsync(userRoleName);
            if (userRole != null)
            {
                var reservedControllers = _dashboardSettings.Value?.ReservedSlugs ?? new List<string>();
                var allPermissions = await _unitOfWork.Permissions.GetAllPermissionsAsync();
                
                // Get current role permissions
                var currentRolePermissions = await _unitOfWork.RolePermissions.GetPermissionsByRoleAsync(userRoleName);
                var currentPermIds = new System.Collections.Generic.HashSet<Guid>(currentRolePermissions.Select(rp => rp.PermissionId));

                foreach (var perm in allPermissions)
                {
                    if (reservedControllers.Contains(perm.ControllerName, StringComparer.OrdinalIgnoreCase))
                    {
                        if (!currentPermIds.Contains(perm.Id))
                        {
                            _unitOfWork.RolePermissions.Add(new RolePermission
                            {
                                
                                RoleName = userRole.Name,
                                PermissionId = perm.Id
                            });
                        }
                    }
                }
                await _unitOfWork.SaveChangesAsync();
            }


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

            await SeedIdentitySettingAsync();
            await SeedDefaultPagesAsync();
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Error while seeding Identity data.");
        }
    }
    
    
    private async Task SeedIdentitySettingAsync ()
    {
        var settings = await _noSqlUnitOfWork.IdentitySettings.GetAllAsync();
        if (settings.Count == 0)
        {
            var conf = _configuration.GetSection("IdentitySettings").Exists()
                ? _configuration.GetSection("IdentitySettings")
                : _configuration.GetSection("AppIdentitySetting");

            var setting = new AzureTest.Domain.Documents.IdentitySetting
            {
                Id = Guid.CreateVersion7(),
                RedirectAfterRegister = conf["RedirectAfterRegister"] ?? "/Auth/Login",
                RedirectAfterLogin = conf["RedirectAfterLogin"] ?? "/",
                StayLoginAfterRegister = bool.TryParse(conf["StayLoginAfterRegister"], out var b) ? b : false,
                RolesForRegister = conf.GetSection("RolesForRegister").Get<List<string>>() ?? new List<string> { "User" }
            };
            
            await _noSqlUnitOfWork.IdentitySettings.AddAsync(setting);
            await _noSqlUnitOfWork.SaveChangesAsync();
            _logger.LogInformation("IdentitySetting document seeded from IdentitySettings.");
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
            new { Name = "Status Breakdown", Url = "/api/analytics/status-counts", Type = "doughnut", Color = "#3b82f6" },
            new { Name = "Applications By Date", Url = "/api/analytics/applications-by-date", Type = "line", Color = "#10b981" },
            new { Name = "Top Positions", Url = "/api/analytics/position-counts", Type = "bar", Color = "#8b5cf6" },
            new { Name = "Top Locations", Url = "/api/analytics/location-counts", Type = "bar", Color = "#f59e0b" },
            new { Name = "Application Sources", Url = "/api/analytics/source-counts", Type = "pie", Color = "#ec4899" },
            new { Name = "Expenses by Category", Url = "/api/analytics/expenses-by-category", Type = "doughnut", Color = "#f43f5e" },
            new { Name = "Debts by Person", Url = "/api/analytics/debts-by-person", Type = "bar", Color = "#14b8a6" },
            new { Name = "Ticket Status", Url = "/api/analytics/ticket-status-counts", Type = "doughnut", Color = "#0284c7" },
            new { Name = "Habit Adherence", Url = "/api/analytics/habit-adherence", Type = "pie", Color = "#10b981" },
            new { Name = "Monthly Expenses", Url = "/api/analytics/monthly-expenses", Type = "line", Color = "#f43f5e" },
            new { Name = "Monthly Debts", Url = "/api/analytics/monthly-debts", Type = "line", Color = "#8b5cf6" },
            // KPIs
            new { Name = "Total Applications", Url = "/api/analytics/total-applications", Type = "kpi", Color = "#3b82f6" },
            new { Name = "Active Processing", Url = "/api/analytics/active-processing", Type = "kpi", Color = "#10b981" },
            new { Name = "Interview Rate", Url = "/api/analytics/interview-rate", Type = "kpi", Color = "#f59e0b" },
            new { Name = "Total Companies", Url = "/api/analytics/total-companies", Type = "kpi", Color = "#6366f1" },
            new { Name = "Registered Users", Url = "/api/analytics/registered-users", Type = "kpi", Color = "#ec4899" },
            new { Name = "Occupied Storage", Url = "/api/analytics/occupied-storage", Type = "kpi", Color = "#8b5cf6" },
            new { Name = "Outstanding Debt", Url = "/api/analytics/outstanding-debt", Type = "kpi", Color = "#ef4444" },
            new { Name = "Expenses This Month", Url = "/api/analytics/expenses-this-month", Type = "kpi", Color = "#f97316" },
            new { Name = "Unanswered Tickets", Url = "/api/analytics/unanswered-tickets", Type = "kpi", Color = "#0ea5e9" },
            // Lists
            new { Name = "Pending Tasks", Url = "/api/analytics/pending-tasks", Type = "list", Color = "#3b82f6" },
            new { Name = "Upcoming Events", Url = "/api/analytics/upcoming-events", Type = "list", Color = "#10b981" },
            new { Name = "Daily Habits", Url = "/api/analytics/daily-habits", Type = "list", Color = "#f59e0b" },
            new { Name = "Recent Debts", Url = "/api/analytics/recent-debts", Type = "list", Color = "#ef4444" },
            new { Name = "Recent Expenses", Url = "/api/analytics/recent-expenses", Type = "list", Color = "#ec4899" },
            new { Name = "Debts Due Soon", Url = "/api/analytics/debts-due-soon", Type = "list", Color = "#f43f5e" },
            new { Name = "Recent Activity", Url = "/api/analytics/recent-activity", Type = "list", Color = "#6366f1" }
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
                HtmlContent = h.Item2,
                MinWidth = 2,
                MinHeight = 2,
                IsResizable = true
            });
        }
        
        await _noSqlUnitOfWork.SaveChangesAsync();
    }

    private async Task SeedDefaultPagesAsync()
    {
        try
        {
            var pages = await _noSqlUnitOfWork.Pages.GetAllAsync();

            // 1. Subscription Plans Page
            if (!pages.Any(p => p.Slug == "subscription-plans"))
            {
                var subscriptionPage = new AzureTest.Domain.Documents.Pages.PageDocument
                {
                    Id = Guid.NewGuid(),
                    Title = "Membership & Subscription Plans",
                    Slug = "subscription-plans",
                    Status = AzureTest.Domain.Enums.PageStatus.Published,
                    CreatedAt = DateTime.UtcNow,
                    Content = """
                    <div class="py-8 px-4 max-w-5xl mx-auto font-sans">
                      <div class="text-center mb-12">
                        <span class="px-3.5 py-1 text-xs font-bold text-indigo-600 bg-indigo-50 rounded-full border border-indigo-100 uppercase tracking-wider">Premium Experience</span>
                        <h2 class="text-3xl sm:text-4xl font-extrabold text-slate-900 mt-3 mb-3">Elevate Your Football Fantasy Journey</h2>
                        <p class="text-slate-600 max-w-2xl mx-auto text-sm sm:text-base">Unlock real-time Opta match analytics, automated lineup auto-subs, unlimited private leagues, and high-stakes prediction club tournaments.</p>
                      </div>

                      <div class="grid grid-cols-1 md:grid-cols-3 gap-8">
                        <!-- Free Tier -->
                        <div class="bg-white rounded-2xl p-6 border border-slate-200 shadow-sm flex flex-col justify-between">
                          <div>
                            <div class="font-bold text-slate-900 text-lg">Starter Manager</div>
                            <p class="text-slate-500 text-xs mt-1">Perfect for casual fans and friendly office leagues.</p>
                            <div class="my-6">
                              <span class="text-3xl font-extrabold text-slate-900">€0</span>
                              <span class="text-slate-400 text-xs font-medium">/ forever</span>
                            </div>
                            <ul class="space-y-3 text-xs text-slate-600">
                              <li class="flex items-center gap-2"><i class="fa-solid fa-check text-emerald-500"></i> Join up to 3 Public Leagues</li>
                              <li class="flex items-center gap-2"><i class="fa-solid fa-check text-emerald-500"></i> 1 Fantasy Team per competition</li>
                              <li class="flex items-center gap-2"><i class="fa-solid fa-check text-emerald-500"></i> Standard Gameweek Transfers</li>
                              <li class="flex items-center gap-2"><i class="fa-solid fa-check text-emerald-500"></i> Predict Club match forecasting</li>
                            </ul>
                          </div>
                          <a href="/Auth/Register" class="mt-8 block text-center py-2.5 px-4 bg-slate-100 hover:bg-slate-200 text-slate-800 text-xs font-bold rounded-xl transition-colors">Start Free</a>
                        </div>

                        <!-- Pro Tier -->
                        <div class="bg-slate-900 rounded-2xl p-6 text-white shadow-xl relative flex flex-col justify-between border-2 border-indigo-500">
                          <div class="absolute -top-3 right-6 bg-indigo-600 text-white text-[10px] uppercase font-black px-3 py-1 rounded-full tracking-wider">Most Popular</div>
                          <div>
                            <div class="font-bold text-lg text-white">Pro Tactician</div>
                            <p class="text-slate-400 text-xs mt-1">Designed for serious fantasy managers seeking a tactical edge.</p>
                            <div class="my-6">
                              <span class="text-3xl font-extrabold text-white">€4.99</span>
                              <span class="text-slate-400 text-xs font-medium">/ month</span>
                            </div>
                            <ul class="space-y-3 text-xs text-slate-300">
                              <li class="flex items-center gap-2"><i class="fa-solid fa-check text-indigo-400"></i> Unlimited Fantasy Leagues & Private Clubs</li>
                              <li class="flex items-center gap-2"><i class="fa-solid fa-check text-indigo-400"></i> Automated AI Auto-Substitutions</li>
                              <li class="flex items-center gap-2"><i class="fa-solid fa-check text-indigo-400"></i> Live In-Match Player Ratings & Heatmaps</li>
                              <li class="flex items-center gap-2"><i class="fa-solid fa-check text-indigo-400"></i> Advanced Price Rise/Fall Predictor tool</li>
                              <li class="flex items-center gap-2"><i class="fa-solid fa-check text-indigo-400"></i> Custom League Creation with Head Coach rules</li>
                            </ul>
                          </div>
                          <a href="/Subscription/Checkout?plan=pro" class="mt-8 block text-center py-2.5 px-4 bg-indigo-600 hover:bg-indigo-700 text-white text-xs font-bold rounded-xl transition-colors shadow-md">Upgrade to Pro</a>
                        </div>

                        <!-- Elite Tier -->
                        <div class="bg-white rounded-2xl p-6 border border-slate-200 shadow-sm flex flex-col justify-between">
                          <div>
                            <div class="font-bold text-slate-900 text-lg">Club Owner & VIP</div>
                            <p class="text-slate-500 text-xs mt-1">Full auction hosting, trophy leagues, and API access.</p>
                            <div class="my-6">
                              <span class="text-3xl font-extrabold text-slate-900">€9.99</span>
                              <span class="text-slate-400 text-xs font-medium">/ month</span>
                            </div>
                            <ul class="space-y-3 text-xs text-slate-600">
                              <li class="flex items-center gap-2"><i class="fa-solid fa-check text-emerald-500"></i> Host Live Real-Time Draft & Auction Rooms</li>
                              <li class="flex items-center gap-2"><i class="fa-solid fa-check text-emerald-500"></i> Custom Tournament Brackets & Knockout Cups</li>
                              <li class="flex items-center gap-2"><i class="fa-solid fa-check text-emerald-500"></i> Priority Discord/Telegram Gameweek Webhooks</li>
                              <li class="flex items-center gap-2"><i class="fa-solid fa-check text-emerald-500"></i> Verified League Badges & Custom Trophies</li>
                            </ul>
                          </div>
                          <a href="/Subscription/Checkout?plan=elite" class="mt-8 block text-center py-2.5 px-4 bg-slate-100 hover:bg-slate-200 text-slate-800 text-xs font-bold rounded-xl transition-colors">Go VIP</a>
                        </div>
                      </div>
                    </div>
                    """
                };
                await _noSqlUnitOfWork.Pages.AddAsync(subscriptionPage);
            }

            // 2. Player Pricing Rules Page
            if (!pages.Any(p => p.Slug == "player-valuation-rules"))
            {
                var valuationPage = new AzureTest.Domain.Documents.Pages.PageDocument
                {
                    Id = Guid.NewGuid(),
                    Title = "Player Valuation Models: Performance vs Demanding Mode",
                    Slug = "player-valuation-rules",
                    Status = AzureTest.Domain.Enums.PageStatus.Published,
                    CreatedAt = DateTime.UtcNow,
                    Content = """
                    <div class="py-8 px-4 max-w-4xl mx-auto font-sans leading-relaxed text-slate-800">
                      <div class="border-b border-slate-200 pb-6 mb-8">
                        <span class="px-3 py-1 bg-emerald-50 text-emerald-700 font-bold text-xs rounded-full border border-emerald-200">Economic Engine</span>
                        <h1 class="text-3xl font-black text-slate-900 mt-3">Player Valuation Models: Performance vs Demanding Mode</h1>
                        <p class="text-sm text-slate-500 mt-1">How dynamic player prices evolve weekly across leagues.</p>
                      </div>

                      <div class="space-y-8 text-sm">
                        <div class="p-5 rounded-2xl bg-slate-50 border border-slate-200">
                          <h3 class="text-base font-bold text-slate-900 flex items-center gap-2 mb-2">
                            <i class="fa-solid fa-chart-line text-blue-600"></i> 1. Performance-Driven Valuation Model
                          </h3>
                          <p class="text-slate-600 mb-3">Prices fluctuate strictly based on objective on-pitch real-world stats collected via API-Football over a rolling 3-gameweek window:</p>
                          <div class="grid grid-cols-1 sm:grid-cols-2 gap-3 text-xs">
                            <div class="p-3 bg-white rounded-xl border border-slate-200">
                              <div class="font-bold text-slate-700 mb-1">Key Growth Drivers (+€0.1M to +€0.3M)</div>
                              <ul class="list-disc pl-4 space-y-0.5 text-slate-500">
                                <li>Provider Match Rating ≥ 8.0</li>
                                <li>Consecutive Goals / Clean Sheets</li>
                                <li>Key Passes &gt; 3 per 90 mins</li>
                                <li>Man of the Match (MVP) honors</li>
                              </ul>
                            </div>
                            <div class="p-3 bg-white rounded-xl border border-slate-200">
                              <div class="font-bold text-slate-700 mb-1">Price Drops (-€0.1M to -€0.2M)</div>
                              <ul class="list-disc pl-4 space-y-0.5 text-slate-500">
                                <li>Minutes played &lt; 30 mins</li>
                                <li>Red cards or conceded penalties</li>
                                <li>Provider Match Rating &lt; 6.0</li>
                                <li>Confirmed injury / suspension status</li>
                              </ul>
                            </div>
                          </div>
                        </div>

                        <div class="p-5 rounded-2xl bg-slate-50 border border-slate-200">
                          <h3 class="text-base font-bold text-slate-900 flex items-center gap-2 mb-2">
                            <i class="fa-solid fa-fire text-amber-600"></i> 2. Demanding (Market-Driven) Valuation Model
                          </h3>
                          <p class="text-slate-600 mb-3">Modeled after real stock markets and Premier League FPL mechanics, prices fluctuate based on net transfer transactions:</p>
                          <div class="p-4 bg-white rounded-xl border border-slate-200 space-y-2 text-xs">
                            <div><strong>Net Transfers Threshold:</strong> If net transfers (Purchases − Sales) exceed <strong>+5%</strong> of active managers in a gameweek, the player rises by <strong>+€0.1M</strong>.</div>
                            <div><strong>Heavy Selling Drops:</strong> If net sales exceed <strong>−5%</strong>, the price drops by <strong>−€0.1M</strong>.</div>
                            <div><strong>Gameweek Cap:</strong> A player's price cannot rise or fall by more than <strong>±€0.3M</strong> within a single gameweek, preventing sudden economic shocks.</div>
                          </div>
                        </div>
                      </div>
                    </div>
                    """
                };
                await _noSqlUnitOfWork.Pages.AddAsync(valuationPage);
            }

            await _noSqlUnitOfWork.SaveChangesAsync();
            _logger.LogInformation("Default Pages (Subscription Plans & Player Valuation Rules) seeded successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not seed default pages into NoSQL document store.");
        }
    }
}
