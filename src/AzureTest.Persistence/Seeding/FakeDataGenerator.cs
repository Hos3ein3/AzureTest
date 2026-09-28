using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureTest.Application.Configuration;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Domain.Documents;
using AzureTest.Domain.Entities;
using AzureTest.Domain.Entities.Identity;
using AzureTest.Domain.Enums;
using AzureTest.Persistence.Context;
using Bogus;
using Microsoft.AspNetCore.Identity;

namespace AzureTest.Persistence.Seeding;

/// <summary>
/// Generates large-scale, reproducible fake data for every SQL entity and NoSQL document
/// in the AzureTest domain using the Bogus library.
///
/// Fixed Roles:   Admin | User | System
/// Fixed Users:
///   admin@azuretest.local   / 123456  → role: Admin
///   user@azuretest.local    / 123456  → role: User
///   system@azuretest.local  / 123456  → role: System
///
/// Data distribution (by record count):
///   90% → user@azuretest.local  (the main persona)
///    9% → admin@azuretest.local
///    1% → system@azuretest.local
///
/// All DateTime values are stored as UTC (DateTime.SpecifyKind(..., DateTimeKind.Utc))
/// to satisfy PostgreSQL's timestamptz requirement.
/// </summary>
public static class FakeDataGenerator
{
    // Fixed seed → deterministic / reproducible every run
    private const int Seed = 42_000_000;

    // ──────────────────────────────────────────────────────────────────────────
    // Volume constants  (tweak to make even larger)
    // ──────────────────────────────────────────────────────────────────────────
    private const int CompanyCount          = 120;   // global NoSQL docs
    private const int DebtPersonsPerUser    = 20;
    private const int CategoriesPerUser     = 12;
    private const int ResumesUser           = 350;   // 90 %
    private const int ResumesAdmin          = 35;    //  9 %
    private const int ResumesSystem         = 5;     //  1 %
    private const int ExpensesUser          = 600;
    private const int ExpensesAdmin         = 60;
    private const int ExpensesSystem        = 7;
    private const int DebtsUser             = 120;
    private const int DebtsAdmin            = 12;
    private const int DebtsSystem           = 2;
    private const int TasksUser             = 300;
    private const int TasksAdmin            = 28;
    private const int TasksSystem           = 3;
    private const int EventsUser            = 200;
    private const int EventsAdmin           = 18;
    private const int EventsSystem          = 2;
    private const int HabitsUser            = 30;
    private const int HabitsAdmin           = 3;
    private const int HabitsSystem          = 1;
    private const int HabitLogDays          = 90;    // log history per tracker
    private const int TicketsUser           = 80;
    private const int TicketsAdmin          = 8;
    private const int TicketsSystem         = 1;
    private const int StatusLogsPerResume   = 4;

    // ──────────────────────────────────────────────────────────────────────────
    // Colour palette for events
    // ──────────────────────────────────────────────────────────────────────────
    private static readonly string[] EventColors =
    [
        "#3b82f6", "#10b981", "#f59e0b", "#ef4444",
        "#8b5cf6", "#ec4899", "#06b6d4", "#f97316",
        "#84cc16", "#a855f7"
    ];

    private static readonly string[] Priorities = ["Low", "Medium", "High", "Critical"];

    private static readonly string[] Languages = ["en", "nl", "de", "fr", "es", "it", "pt", "tr"];

    private static readonly string[] Themes = ["blue", "green", "purple", "orange", "slate", "rose", "teal", "amber"];

    private static readonly string[] CategoryNames =
    [
        "Food & Dining", "Transport", "Housing & Rent", "Entertainment",
        "Healthcare", "Education", "Clothing & Fashion", "Utilities",
        "Savings & Investment", "Travel", "Subscriptions", "Sport & Fitness"
    ];

    // ──────────────────────────────────────────────────────────────────────────
    // UTC helper — PostgreSQL requires DateTimeKind.Utc for timestamp columns.
    // Bogus generates Unspecified / Local datetimes; this wrapper forces UTC.
    // ──────────────────────────────────────────────────────────────────────────
    private static DateTime Utc(DateTime dt) =>
        DateTime.SpecifyKind(dt, DateTimeKind.Utc);

    // ──────────────────────────────────────────────────────────────────────────
    // Entry point
    // ──────────────────────────────────────────────────────────────────────────
    public static async Task SeedAsync(
        AzureTestDbContext sqlContext,
        INoSqlUnitOfWork noSql,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        SimulateSetting? settings = null)
    {
        settings ??= new SimulateSetting();
        Randomizer.Seed = new Random(Seed);

        // ── Step 1 ── Roles ──────────────────────────────────────────────────
        await EnsureRoleAsync(roleManager, "Admin");
        await EnsureRoleAsync(roleManager, "User");
        await EnsureRoleAsync(roleManager, "System");

        // ── Step 2 ── Fixed seed users ───────────────────────────────────────
        var userUser   = await EnsureUserAsync(userManager, "user@azuretest.local",   "123456", "Regular", "User",   "User");
        var adminUser  = await EnsureUserAsync(userManager, "admin@azuretest.local",  "123456", "Admin",   "Admin",  "Admin");
        var systemUser = await EnsureUserAsync(userManager, "system@azuretest.local", "123456", "System",  "System", "System");

        var userId   = userUser.Id;
        var adminId  = adminUser.Id;
        var systemId = systemUser.Id;

        var allUserIds = new List<Guid> { userId };

        if (settings.AddSeedUser)
        {
            var userFaker = new Faker();
            for (int i = 0; i < 50; i++)
            {
                var fakeUser = await EnsureUserAsync(
                    userManager, 
                    $"user{i}@azuretest.local", 
                    "123456", 
                    userFaker.Name.FirstName(), 
                    userFaker.Name.LastName(), 
                    "User");
                allUserIds.Add(fakeUser.Id);
            }
        }

        // ── Step 3 ── NoSQL Documents ─────────────────────────────────────────

        // ── 3a. Dashboard Components ──────────────────────────────────────────
        var existingComps = await noSql.DashboardComponents.GetAllAsync(default);
        if (!existingComps.Any())
        {
            var dashboardComponents = BuildDashboardComponents();
            foreach (var dc in dashboardComponents) await noSql.DashboardComponents.AddAsync(dc);
        }

        // ── 3a-2. User Dashboards and Raw Components ──────────────────────────
        var existingDashboards = await noSql.UserDashboards.GetAllAsync(default);
        if (!existingDashboards.Any(d => d.Name == "Dynamic Analytics"))
        {
            var dashboards = BuildUserDashboardsAndRawComponents(userId, out var rawComponents);
            foreach (var d in dashboards) await noSql.UserDashboards.AddAsync(d);
            foreach (var rc in rawComponents) await noSql.DashboardRawComponents.AddAsync(rc);
        }

        // ── 3b. Companies (global, no owner) ─────────────────────────────────
        var companies = BuildCompanies(CompanyCount);
        foreach (var c in companies) await noSql.Companies.AddAsync(c);

        // ── 3b. DebtPersons (per-user) ────────────────────────────────────────
        var allDebtPersons    = new List<DebtPerson>();
        var debtPersonsByUser = new Dictionary<Guid, List<DebtPerson>>();

        foreach (var (uid, count) in new[]
        {
            (userId,   DebtPersonsPerUser),
            (adminId,  (int)Math.Ceiling(DebtPersonsPerUser * 0.09)),
            (systemId, 1)
        })
        {
            var persons = BuildDebtPersons(uid, count);
            debtPersonsByUser[uid] = persons;
            allDebtPersons.AddRange(persons);
            foreach (var p in persons) await noSql.DebtPersons.AddAsync(p);
        }

        // ── 3c. ExpenseCategories (per-user) ──────────────────────────────────
        var allCategories    = new List<ExpenseCategory>();
        var categoriesByUser = new Dictionary<Guid, List<ExpenseCategory>>();

        foreach (var (uid, count) in new[]
        {
            (userId,   CategoriesPerUser),
            (adminId,  CategoriesPerUser),
            (systemId, 4)
        })
        {
            var cats = BuildExpenseCategories(uid, count);
            categoriesByUser[uid] = cats;
            allCategories.AddRange(cats);
            foreach (var c in cats) await noSql.ExpenseCategories.AddAsync(c);
        }

        // ── 3d. UserPreferences (one per user) ────────────────────────────────
        var prefFaker = new Faker();
        await noSql.UserPreferences.AddAsync(new UserPreference { UserId = userId,   Language = "en", ThemeColor = "blue" });
        await noSql.UserPreferences.AddAsync(new UserPreference { UserId = adminId,  Language = "nl", ThemeColor = "slate" });
        await noSql.UserPreferences.AddAsync(new UserPreference { UserId = systemId, Language = prefFaker.PickRandom(Languages), ThemeColor = prefFaker.PickRandom(Themes) });

        await noSql.SaveChangesAsync();

        // ── Step 4 ── SQL Entities ────────────────────────────────────────────

        // ── 4a. Resumes ───────────────────────────────────────────────────────
        var allResumes = new List<Resume>();
        allResumes.AddRange(BuildResumes(userId,   ResumesUser,   companies));
        allResumes.AddRange(BuildResumes(adminId,  ResumesAdmin,  companies));
        allResumes.AddRange(BuildResumes(systemId, ResumesSystem, companies));
        await sqlContext.Resumes.AddRangeAsync(allResumes);
        await sqlContext.SaveChangesAsync();

        // ── 4b. ResumeStatusLogs (NoSQL, linked to saved resumes) ────────────
        var logFaker = new Faker();
        foreach (var resume in allResumes)
        {
            int logCount   = logFaker.Random.Int(1, StatusLogsPerResume);
            var prevStatus = resume.Status;
            for (int i = 0; i < logCount; i++)
            {
                var newStatus = logFaker.PickRandom<ResumeStatus>();
                await noSql.ResumeStatusLogs.AddAsync(new ResumeStatusLog
                {
                    ResumeId  = resume.Id,
                    Date      = Utc(logFaker.Date.Recent(180)),
                    OldStatus = prevStatus,
                    NewStatus = newStatus,
                    Note      = logFaker.Random.Bool(0.4f) ? logFaker.Lorem.Sentence() : null
                });
                prevStatus = newStatus;
            }
        }
        await noSql.SaveChangesAsync();

        // ── 4c. Expenses ──────────────────────────────────────────────────────
        var allExpenses = new List<Expense>();
        allExpenses.AddRange(BuildExpenses(userId,   ExpensesUser,   categoriesByUser[userId]));
        allExpenses.AddRange(BuildExpenses(adminId,  ExpensesAdmin,  categoriesByUser[adminId]));
        allExpenses.AddRange(BuildExpenses(systemId, ExpensesSystem, categoriesByUser[systemId]));
        await sqlContext.Expenses.AddRangeAsync(allExpenses);
        await sqlContext.SaveChangesAsync();

        // ── 4d. Debts ─────────────────────────────────────────────────────────
        var allDebts = new List<Debt>();
        allDebts.AddRange(BuildDebts(userId,   DebtsUser,   debtPersonsByUser[userId]));
        allDebts.AddRange(BuildDebts(adminId,  DebtsAdmin,  debtPersonsByUser[adminId]));
        allDebts.AddRange(BuildDebts(systemId, DebtsSystem, debtPersonsByUser[systemId]));
        await sqlContext.Debts.AddRangeAsync(allDebts);
        await sqlContext.SaveChangesAsync();

        // ── 4e. TaskItems ─────────────────────────────────────────────────────
        var allTasks = new List<TaskItem>();
        allTasks.AddRange(BuildTaskItems(userId,   TasksUser));
        allTasks.AddRange(BuildTaskItems(adminId,  TasksAdmin));
        allTasks.AddRange(BuildTaskItems(systemId, TasksSystem));
        await sqlContext.Tasks.AddRangeAsync(allTasks);
        await sqlContext.SaveChangesAsync();

        // ── 4f. Events ────────────────────────────────────────────────────────
        var allEvents = new List<Event>();
        allEvents.AddRange(BuildEvents(userId,   EventsUser));
        allEvents.AddRange(BuildEvents(adminId,  EventsAdmin));
        allEvents.AddRange(BuildEvents(systemId, EventsSystem));
        await sqlContext.Events.AddRangeAsync(allEvents);
        await sqlContext.SaveChangesAsync();

        // ── 4g. HabitTrackers + HabitLogs ─────────────────────────────────────
        var allTrackers = new List<HabitTracker>();
        allTrackers.AddRange(BuildHabitTrackers(userId,   HabitsUser));
        allTrackers.AddRange(BuildHabitTrackers(adminId,  HabitsAdmin));
        allTrackers.AddRange(BuildHabitTrackers(systemId, HabitsSystem));
        await sqlContext.HabitTrackers.AddRangeAsync(allTrackers);
        await sqlContext.SaveChangesAsync();

        var allLogs = new List<HabitLog>();
        foreach (var tracker in allTrackers)
            allLogs.AddRange(BuildHabitLogs(tracker));
        await sqlContext.HabitLogs.AddRangeAsync(allLogs);
        await sqlContext.SaveChangesAsync();

        // ── 4h. Tickets + TicketMessages ──────────────────────────────────────
        var allTickets = new List<Ticket>();
        allTickets.AddRange(BuildTickets(userId,   TicketsUser));
        allTickets.AddRange(BuildTickets(adminId,  TicketsAdmin));
        allTickets.AddRange(BuildTickets(systemId, TicketsSystem));
        await sqlContext.Tickets.AddRangeAsync(allTickets);
        await sqlContext.SaveChangesAsync();

        var allMessages = new List<TicketMessage>();
        var msgFaker    = new Faker();
        foreach (var ticket in allTickets)
        {
            int msgCount = msgFaker.Random.Int(1, 8);
            for (int i = 0; i < msgCount; i++)
            {
                var speakerId = i % 2 == 0 ? ticket.UserId : adminId;
                var msg = TicketMessage.Create(ticket.Id, speakerId, msgFaker.Lorem.Paragraphs(msgFaker.Random.Int(1, 3)));
                msg.CreatedOn = Utc(msgFaker.Date.Recent(30));
                msg.CreatedBy = speakerId.ToString();
                allMessages.Add(msg);
            }
        }
        await sqlContext.TicketMessages.AddRangeAsync(allMessages);
        await sqlContext.SaveChangesAsync();
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Builder helpers — every DateTime returned is UTC
    // ──────────────────────────────────────────────────────────────────────────

    private static List<DashboardComponent> BuildDashboardComponents()
    {
        return new List<DashboardComponent>
        {
            new DashboardComponent { Name = "Active Processing", ImagePath = "/dashboardsamples/ActiveProcessing.png", PartialViewName = "_WidgetActiveProcessing", Category = "General", DefaultWidth = 3, DefaultHeight = 2 },
            new DashboardComponent { Name = "Application By Status", ImagePath = "/dashboardsamples/ApplicationByStatus.png", PartialViewName = "_WidgetApplicationByStatus", Category = "Resume", DefaultWidth = 4, DefaultHeight = 4 },
            new DashboardComponent { Name = "Application Over Time", ImagePath = "/dashboardsamples/ApplicationOverTime.png", PartialViewName = "_WidgetApplicationOverTime", Category = "Resume", DefaultWidth = 8, DefaultHeight = 4 },
            new DashboardComponent { Name = "Application Sources", ImagePath = "/dashboardsamples/ApplicationSources.png", PartialViewName = "_WidgetApplicationSources", Category = "Resume", DefaultWidth = 4, DefaultHeight = 4 },
            new DashboardComponent { Name = "Daily Habits", ImagePath = "/dashboardsamples/DailyHabits.png", PartialViewName = "_WidgetDailyHabits", Category = "Habits", DefaultWidth = 4, DefaultHeight = 4 },
            new DashboardComponent { Name = "Debts By Person", ImagePath = "/dashboardsamples/DebtsByPerson.png", PartialViewName = "_WidgetDebtsByPerson", Category = "Debts", DefaultWidth = 6, DefaultHeight = 4 },
            new DashboardComponent { Name = "Debts Due Soon", ImagePath = "/dashboardsamples/DebtsDueSoon.png", PartialViewName = "_WidgetDebtsDueSoon", Category = "Debts", DefaultWidth = 4, DefaultHeight = 4 },
            new DashboardComponent { Name = "Expenses By Category", ImagePath = "/dashboardsamples/ExpensesByCategory.png", PartialViewName = "_WidgetExpensesByCategory", Category = "Expenses", DefaultWidth = 6, DefaultHeight = 4 },
            new DashboardComponent { Name = "Habit Adherence", ImagePath = "/dashboardsamples/HabitAdherence.png", PartialViewName = "_WidgetHabitAdherence", Category = "Habits", DefaultWidth = 4, DefaultHeight = 4 },
            new DashboardComponent { Name = "Interview Rate", ImagePath = "/dashboardsamples/InterviewRate.png", PartialViewName = "_WidgetInterviewRate", Category = "Resume", DefaultWidth = 3, DefaultHeight = 2 },
            new DashboardComponent { Name = "Monthly Cash Flow", ImagePath = "/dashboardsamples/MonthlyCashFlow.png", PartialViewName = "_WidgetMonthlyCashFlow", Category = "Expenses", DefaultWidth = 4, DefaultHeight = 4 },
            new DashboardComponent { Name = "Monthly Expenses", ImagePath = "/dashboardsamples/MonthlyExoenses.png", PartialViewName = "_WidgetMonthlyExpenses", Category = "Expenses", DefaultWidth = 3, DefaultHeight = 2 },
            new DashboardComponent { Name = "Occupied Storage", ImagePath = "/dashboardsamples/OccupiedStorage.png", PartialViewName = "_WidgetOccupiedStorage", Category = "General", DefaultWidth = 3, DefaultHeight = 2 },
            new DashboardComponent { Name = "Outstanding Debt", ImagePath = "/dashboardsamples/OutStandingDebt.png", PartialViewName = "_WidgetOutstandingDebt", Category = "Debts", DefaultWidth = 3, DefaultHeight = 2 },
            new DashboardComponent { Name = "Pending Tasks", ImagePath = "/dashboardsamples/PendingTasks.png", PartialViewName = "_WidgetPendingTasks", Category = "General", DefaultWidth = 4, DefaultHeight = 4 },
            new DashboardComponent { Name = "Recent Activity", ImagePath = "/dashboardsamples/RecentActivity.png", PartialViewName = "_WidgetRecentActivity", Category = "General", DefaultWidth = 4, DefaultHeight = 4 },
            new DashboardComponent { Name = "Recent Debts", ImagePath = "/dashboardsamples/RecentDebts.png", PartialViewName = "_WidgetRecentDebts", Category = "Debts", DefaultWidth = 4, DefaultHeight = 4 },
            new DashboardComponent { Name = "Recent Expenses", ImagePath = "/dashboardsamples/RecentExpenses.png", PartialViewName = "_WidgetRecentExpenses", Category = "Expenses", DefaultWidth = 4, DefaultHeight = 4 },
            new DashboardComponent { Name = "Registered Users", ImagePath = "/dashboardsamples/RegisteredUsers.png", PartialViewName = "_WidgetRegisteredUsers", Category = "Users", RequiredRole = "Admin", DefaultWidth = 3, DefaultHeight = 2 },
            new DashboardComponent { Name = "Support Tickets", ImagePath = "/dashboardsamples/SupportTickets.png", PartialViewName = "_WidgetSupportTickets", Category = "Tickets", DefaultWidth = 4, DefaultHeight = 4 },
            new DashboardComponent { Name = "Top Locations", ImagePath = "/dashboardsamples/TopLocations.png", PartialViewName = "_WidgetTopLocations", Category = "Resume", DefaultWidth = 4, DefaultHeight = 4 },
            new DashboardComponent { Name = "Top Positions", ImagePath = "/dashboardsamples/TopPostions.png", PartialViewName = "_WidgetTopPositions", Category = "Resume", DefaultWidth = 4, DefaultHeight = 4 },
            new DashboardComponent { Name = "Total Applications", ImagePath = "/dashboardsamples/TotalApplications.png", PartialViewName = "_WidgetTotalApplications", Category = "Resume", DefaultWidth = 3, DefaultHeight = 2 },
            new DashboardComponent { Name = "Total Companies", ImagePath = "/dashboardsamples/TotalCompanies.png", PartialViewName = "_WidgetTotalCompanies", Category = "Resume", DefaultWidth = 3, DefaultHeight = 2 },
            new DashboardComponent { Name = "Unanswered Tickets", ImagePath = "/dashboardsamples/UnansweredTickets.png", PartialViewName = "_WidgetUnansweredTickets", Category = "Tickets", DefaultWidth = 4, DefaultHeight = 4 },
            new DashboardComponent { Name = "Upcoming Events", ImagePath = "/dashboardsamples/UpcomingEvents.png", PartialViewName = "_WidgetUpcomingEvents", Category = "General", DefaultWidth = 4, DefaultHeight = 4 }
        };
    }

    private static List<UserDashboard> BuildUserDashboardsAndRawComponents(Guid userId, out List<DashboardRawComponent> rawComponents)
    {
        var dashboards = new List<UserDashboard>();
        rawComponents = new List<DashboardRawComponent>();
        
        var mainDashboard = new UserDashboard
        {
            Id = Guid.CreateVersion7(),
            UserId = userId,
            Name = "Dynamic Analytics",
            Slug = "dynamic-analytics"
        };
        dashboards.Add(mainDashboard);

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
            rawComponents.Add(new DashboardRawComponent
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
            rawComponents.Add(new DashboardRawComponent
            {
                Name = h.Item1,
                Type = "RawHtml",
                Category = "Widgets",
                HtmlContent = h.Item2
            });
        }

        return dashboards;
    }

    private static List<Company> BuildCompanies(int count)
    {
        return new Faker<Company>()
            .RuleFor(c => c.CompanyName, f => f.Company.CompanyName())
            .RuleFor(c => c.KvkNumber,   f => f.Random.ReplaceNumbers("########"))
            .RuleFor(c => c.IsINDMember, f => f.Random.Bool(0.25f))
            .RuleFor(c => c.LinkedinURL, f => $"https://linkedin.com/company/{f.Internet.DomainWord()}")
            .RuleFor(c => c.IndeedUrl,   f => $"https://indeed.com/cmp/{f.Internet.DomainWord()}")
            .RuleFor(c => c.Location,    f => $"{f.Address.City()}, {f.Address.Country()}")
            .Generate(count);
    }

    private static List<DebtPerson> BuildDebtPersons(Guid userId, int count)
    {
        return new Faker<DebtPerson>()
            .RuleFor(p => p.UserId,   _ => userId)
            .RuleFor(p => p.FullName, f => f.Name.FullName())
            .Generate(count);
    }

    private static List<ExpenseCategory> BuildExpenseCategories(Guid userId, int count)
    {
        var f      = new Faker();
        var result = new List<ExpenseCategory>();
        var names  = CategoryNames.OrderBy(_ => f.Random.Int()).Take(count).ToArray();
        foreach (var name in names)
            result.Add(new ExpenseCategory { UserId = userId, Name = name });
        return result;
    }

    private static List<Resume> BuildResumes(Guid userId, int count, List<Company> companies)
    {
        var f      = new Faker();
        var result = new List<Resume>();
        for (int i = 0; i < count; i++)
        {
            var company = f.Random.Bool(0.8f) ? f.PickRandom(companies) : null;
            var status  = f.PickRandom<ResumeStatus>();
            var source  = f.PickRandom<ResumeSource>();
            var ats     = f.Random.Bool(0.7f) ? (int?)f.Random.Int(0, 100) : null;

            var resume = Resume.Create(
                positionTitle: f.Name.JobTitle(),
                description:   f.Lorem.Paragraphs(f.Random.Int(1, 4)),
                location:      f.Random.Bool(0.3f) ? "Remote" : $"{f.Address.City()}, {f.Address.Country()}",
                jobLink:       f.Random.Bool(0.9f) ? f.Internet.Url() : string.Empty,
                note:          f.Random.Bool(0.5f) ? f.Lorem.Sentences(f.Random.Int(1, 3)) : string.Empty,
                chatUrl:       f.Random.Bool(0.4f) ? f.Internet.Url() : string.Empty,
                companyId:     company?.Id,
                userId:        userId,
                source:        source,
                status:        status,
                atsScore:      ats
            );
            // ★ UTC: Bogus Date.Past returns local time — force UTC
            resume.CreatedOn = Utc(f.Date.Past(2));
            resume.CreatedBy = userId.ToString();
            result.Add(resume);
        }
        return result;
    }

    private static List<Expense> BuildExpenses(Guid userId, int count, List<ExpenseCategory> categories)
    {
        var f      = new Faker();
        var result = new List<Expense>();
        for (int i = 0; i < count; i++)
        {
            Guid? catId = categories.Any() && f.Random.Bool(0.85f)
                ? f.PickRandom(categories).Id
                : null;

            var expense = Expense.Create(
                userId:      userId,
                amount:      f.Finance.Amount(1, 2000),
                date:        Utc(f.Date.Past(2)),          // ★ UTC expense date
                description: f.Commerce.ProductName() + " – " + f.Commerce.Department(),
                categoryId:  catId
            );
            expense.CreatedOn = Utc(f.Date.Past(2));       // ★ UTC audit field
            expense.CreatedBy = userId.ToString();
            result.Add(expense);
        }
        return result;
    }

    private static List<Debt> BuildDebts(Guid userId, int count, List<DebtPerson> persons)
    {
        var f      = new Faker();
        var result = new List<Debt>();

        if (!persons.Any()) return result;

        for (int i = 0; i < count; i++)
        {
            var person     = persons[i % persons.Count];
            var borrowDate = Utc(f.Date.Past(2));           // ★ UTC
            var promised   = borrowDate.AddMonths(f.Random.Int(1, 18));
            var isDone     = f.Random.Bool(0.35f);

            result.Add(new Debt
            {
                UserId              = userId,
                DebtPersonId        = person.Id,
                Amount              = f.Finance.Amount(20, 10_000),
                DateOfBorrow        = borrowDate,            // ★ UTC
                DateOfReturn        = isDone
                    ? (DateTime?)promised.AddDays(f.Random.Int(-30, 30))   // inherits UTC from promised
                    : null,
                PromissedReturnDate = promised,              // ★ UTC (derived from borrowDate)
                Profit              = f.Random.Bool(0.4f) ? f.Finance.Amount(0, 500) : 0,
                Description         = f.Random.Bool(0.6f) ? f.Lorem.Sentence() : null,
                IsDone              = isDone,
                CreatedOn           = Utc(f.Date.Past(2)),  // ★ UTC audit
                CreatedBy           = userId.ToString()
            });
        }
        return result;
    }

    private static List<TaskItem> BuildTaskItems(Guid userId, int count)
    {
        var f      = new Faker();
        var result = new List<TaskItem>();
        for (int i = 0; i < count; i++)
        {
            // ★ UTC: use UtcNow as reference, then offset
            var start = f.Random.Bool(0.5f)
                ? DateTime.UtcNow.AddDays(-f.Random.Int(1, 365))
                : DateTime.UtcNow.AddDays(f.Random.Int(1, 365));
            var end = start.AddDays(f.Random.Int(1, 14));

            var task = TaskItem.Create(
                title:       f.Lorem.Sentence(f.Random.Int(3, 8)),
                description: f.Random.Bool(0.7f) ? f.Lorem.Paragraph() : null,
                startDate:   start,     // ★ UTC
                endDate:     end,       // ★ UTC
                priority:    f.PickRandom(Priorities),
                userId:      userId
            );
            task.CreatedOn = Utc(f.Date.Past(1));           // ★ UTC audit
            task.CreatedBy = userId.ToString();

            if (f.Random.Bool(0.4f)) task.ToggleCompletion();

            result.Add(task);
        }
        return result;
    }

    private static List<Event> BuildEvents(Guid userId, int count)
    {
        var f      = new Faker();
        var result = new List<Event>();
        for (int i = 0; i < count; i++)
        {
            var isAllDay = f.Random.Bool(0.15f);

            // ★ UTC: generate offsets from UtcNow instead of using f.Date.Between (local)
            var offsetDays = f.Random.Int(-180, 180);
            var start      = DateTime.UtcNow.AddDays(offsetDays).AddHours(f.Random.Int(0, 23));
            var end        = isAllDay
                ? start.Date.AddDays(1)                // midnight next day, still UTC
                : start.AddHours(f.Random.Int(1, 8));

            var ev = Event.Create(
                title:       f.Lorem.Sentence(f.Random.Int(2, 6)),
                description: f.Random.Bool(0.6f) ? f.Lorem.Paragraph() : null,
                startDate:   start,     // ★ UTC
                endDate:     end,       // ★ UTC
                isAllDay:    isAllDay,
                color:       f.PickRandom(EventColors),
                userId:      userId
            );
            ev.CreatedOn = Utc(f.Date.Past(1));             // ★ UTC audit
            ev.CreatedBy = userId.ToString();
            result.Add(ev);
        }
        return result;
    }

    private static List<HabitTracker> BuildHabitTrackers(Guid userId, int count)
    {
        var f      = new Faker();
        var result = new List<HabitTracker>();
        for (int i = 0; i < count; i++)
        {
            var type = f.PickRandom<HabitType>();
            var goal = type == HabitType.Boolean ? 1.0 : f.Random.Double(2, 50);

            var tracker = HabitTracker.Create(
                name:        f.Lorem.Word() + " " + f.PickRandom("Tracker", "Goal", "Challenge", "Routine", "Practice"),
                description: f.Random.Bool(0.65f) ? f.Lorem.Sentence() : null,
                type:        type,
                frequency:   f.PickRandom<HabitFrequency>(),
                goal:        goal,
                userId:      userId
            );
            tracker.CreatedOn = Utc(f.Date.Past(1));        // ★ UTC audit
            tracker.CreatedBy = userId.ToString();
            result.Add(tracker);
        }
        return result;
    }

    private static List<HabitLog> BuildHabitLogs(HabitTracker tracker)
    {
        var f      = new Faker();
        var result = new List<HabitLog>();

        for (int day = 0; day < HabitLogDays; day++)
        {
            if (!f.Random.Bool(0.75f)) continue;

            // ★ UTC: DateTime.UtcNow.Date is already UTC
            var logDate = DateTime.UtcNow.Date.AddDays(-day);

            result.Add(HabitLog.Create(
                habitTrackerId: tracker.Id,
                logDate:        logDate,          // ★ UTC date
                booleanValue:   f.Random.Bool(0.7f),
                numericValue:   f.Random.Double(0, tracker.Goal * 1.5),
                notes:          f.Random.Bool(0.2f) ? f.Lorem.Sentence() : null
            ));
        }
        return result;
    }

    private static List<Ticket> BuildTickets(Guid userId, int count)
    {
        var f      = new Faker();
        var result = new List<Ticket>();
        for (int i = 0; i < count; i++)
        {
            var ticket = Ticket.Create(
                subject:  f.Lorem.Sentence(f.Random.Int(4, 10)),
                content:  f.Lorem.Paragraphs(f.Random.Int(1, 4)),
                priority: f.PickRandom<TicketPriority>(),
                userId:   userId
            );

            var status = f.PickRandom<TicketStatus>();
            if (status != TicketStatus.Open) ticket.UpdateStatus(status);

            ticket.CreatedOn = Utc(f.Date.Past(1));         // ★ UTC audit
            ticket.CreatedBy = userId.ToString();
            result.Add(ticket);
        }
        return result;
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Identity helpers
    // ──────────────────────────────────────────────────────────────────────────

    private static async Task EnsureRoleAsync(RoleManager<ApplicationRole> roleManager, string roleName)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
            await roleManager.CreateAsync(new ApplicationRole(roleName));
    }

    private static async Task<ApplicationUser> EnsureUserAsync(
        UserManager<ApplicationUser> userManager,
        string email,
        string password,
        string firstName,
        string lastName,
        string roleName)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName       = email,
                Email          = email,
                EmailConfirmed = true,
                FirstName      = firstName,
                LastName       = lastName
            };
            var result = await userManager.CreateAsync(user, password);
            if (!result.Succeeded)
                throw new InvalidOperationException(
                    $"Failed to create user {email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }

        if (!await userManager.IsInRoleAsync(user, roleName))
            await userManager.AddToRoleAsync(user, roleName);

        return user;
    }
}
