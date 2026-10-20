using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Domain.Documents.Pages;
using AzureTest.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace AzureTest.Infrastructure.Seeding;

public static class VersionReleasePagesSeeder
{
    public static async Task SeedAsync(INoSqlUnitOfWork noSqlUnitOfWork, ILogger logger)
    {
        try
        {
            var existingPages = (await noSqlUnitOfWork.Pages.GetAllAsync()).ToList();

            var releasePages = GetReleasePages();

            bool anyAdded = false;
            foreach (var page in releasePages)
            {
                if (!existingPages.Any(p => p.Slug.Equals(page.Slug, StringComparison.OrdinalIgnoreCase)))
                {
                    await noSqlUnitOfWork.Pages.AddAsync(page);
                    anyAdded = true;
                }
            }

            if (anyAdded)
            {
                await noSqlUnitOfWork.SaveChangesAsync();
                logger.LogInformation("Version release documentation pages (v1.0 to v2.3) seeded successfully.");
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to seed version release documentation pages.");
        }
    }

    public static List<PageDocument> GetReleasePages()
    {
        return new List<PageDocument>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Version 1.0 — Enterprise Identity, Dynamic Authorization & Foundation",
                Slug = "v1-0-core-platform",
                Status = PageStatus.Published,
                CreatedAt = new DateTime(2026, 10, 11, 18, 0, 0, DateTimeKind.Utc),
                Content = """
                <div class="py-6 px-4 max-w-4xl mx-auto font-sans leading-relaxed text-slate-800">
                  <div class="border-b border-slate-200 pb-6 mb-8">
                    <div class="flex items-center gap-2 mb-3">
                      <span class="px-3 py-1 bg-indigo-50 text-indigo-700 font-bold text-xs rounded-full border border-indigo-200">Release v1.0</span>
                      <span class="text-xs text-slate-500"><i class="fa-regular fa-calendar mr-1"></i> October 2026</span>
                    </div>
                    <h1 class="text-3xl sm:text-4xl font-black text-slate-900 tracking-tight">Enterprise Identity, Dynamic Authorization & Core Architecture</h1>
                    <p class="text-base text-slate-600 mt-2">The initial production-grade foundation of the platform, introducing unified authentication, role hierarchies, and dynamic controller-action permissions.</p>
                  </div>

                  <div class="grid grid-cols-1 md:grid-cols-3 gap-4 mb-10">
                    <div class="p-4 rounded-xl bg-slate-50 border border-slate-200">
                      <div class="text-xs font-bold text-slate-500 uppercase">Core Protocol</div>
                      <div class="text-lg font-bold text-slate-900 mt-1"><i class="fa-solid fa-shield-halved text-indigo-600 mr-2"></i>ASP.NET Identity</div>
                    </div>
                    <div class="p-4 rounded-xl bg-slate-50 border border-slate-200">
                      <div class="text-xs font-bold text-slate-500 uppercase">Access Control</div>
                      <div class="text-lg font-bold text-slate-900 mt-1"><i class="fa-solid fa-key text-emerald-600 mr-2"></i>Dynamic Matrix</div>
                    </div>
                    <div class="p-4 rounded-xl bg-slate-50 border border-slate-200">
                      <div class="text-xs font-bold text-slate-500 uppercase">Interface Framework</div>
                      <div class="text-lg font-bold text-slate-900 mt-1"><i class="fa-solid fa-palette text-blue-600 mr-2"></i>Tailwind UI Engine</div>
                    </div>
                  </div>

                  <div class="space-y-8 text-sm">
                    <div class="p-6 rounded-2xl bg-white border border-slate-200 shadow-sm">
                      <h3 class="text-lg font-bold text-slate-900 flex items-center gap-2 mb-3">
                        <i class="fa-solid fa-lock text-indigo-600"></i> Hardened Identity & Password Policies
                      </h3>
                      <p class="text-slate-600 mb-3">Engineered on ASP.NET Core Identity with customized Argon2 password hashing algorithms, secure cookie sessions, and multi-factor validation flows.</p>
                      <ul class="list-disc pl-5 space-y-1.5 text-slate-600">
                        <li><strong>Secure Cookie Authentication:</strong> Configured with HTTP-only, SameSite=Lax, and sliding session expirations.</li>
                        <li><strong>Identity Settings Manager:</strong> Centralized interface at <a href="/IdentitySetting" class="text-indigo-600 font-semibold hover:underline">/IdentitySetting</a> to enforce lockout thresholds, complexity rules, and password age policies.</li>
                        <li><strong>User Lifecycle Management:</strong> Admin-controlled provisioning, status toggles, and role assignments at <a href="/User" class="text-indigo-600 font-semibold hover:underline">/User</a>.</li>
                      </ul>
                    </div>

                    <div class="p-6 rounded-2xl bg-white border border-slate-200 shadow-sm">
                      <h3 class="text-lg font-bold text-slate-900 flex items-center gap-2 mb-3">
                        <i class="fa-solid fa-sliders text-emerald-600"></i> Dynamic Action & Controller Permission Matrix
                      </h3>
                      <p class="text-slate-600 mb-3">Replaced static hardcoded policy strings with a dynamic permission system that inspects all controller endpoints at runtime.</p>
                      <ul class="list-disc pl-5 space-y-1.5 text-slate-600">
                        <li><strong>Permission Scanning:</strong> Auto-discovers every action and controller across the MVC surface with localized descriptions.</li>
                        <li><strong>Granular Role Assignment:</strong> Admins can selectively toggle read/write permissions for specific roles without code deployments via <a href="/Permission" class="text-indigo-600 font-semibold hover:underline">/Permission</a>.</li>
                        <li><strong>Authorization Filter:</strong> Custom authorization pipeline that validates user claims against active role permissions in sub-millisecond lookups.</li>
                      </ul>
                    </div>

                    <div class="p-6 rounded-2xl bg-white border border-slate-200 shadow-sm">
                      <h3 class="text-lg font-bold text-slate-900 flex items-center gap-2 mb-3">
                        <i class="fa-solid fa-gauge text-blue-600"></i> Responsive Dashboard & User Personalization
                      </h3>
                      <p class="text-slate-600 mb-3">A clean, high-performance executive dashboard that adapts to personal preferences and device viewport dimensions.</p>
                      <ul class="list-disc pl-5 space-y-1.5 text-slate-600">
                        <li><strong>Theme Engine:</strong> Persisted user preferences for Dark and Light mode via <a href="/UserPreference" class="text-indigo-600 font-semibold hover:underline">/UserPreference</a>.</li>
                        <li><strong>Telemetry & Correlation IDs:</strong> Automated W3C trace-context tracking and correlation IDs for every web request handled by <a href="/Error" class="text-indigo-600 font-semibold hover:underline">/Error</a>.</li>
                        <li><strong>Sample Data Seeder:</strong> Integrated Bogus-driven test data generation for rapid staging environment testing at <a href="/SampleData" class="text-indigo-600 font-semibold hover:underline">/SampleData</a>.</li>
                      </ul>
                    </div>
                  </div>
                </div>
                """
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Version 1.1 — Financial Ledger, Debt Counterparties & Expense Analytics",
                Slug = "v1-1-financial-ledger",
                Status = PageStatus.Published,
                CreatedAt = new DateTime(2026, 10, 12, 10, 0, 0, DateTimeKind.Utc),
                Content = """
                <div class="py-6 px-4 max-w-4xl mx-auto font-sans leading-relaxed text-slate-800">
                  <div class="border-b border-slate-200 pb-6 mb-8">
                    <div class="flex items-center gap-2 mb-3">
                      <span class="px-3 py-1 bg-emerald-50 text-emerald-700 font-bold text-xs rounded-full border border-emerald-200">Release v1.1</span>
                      <span class="text-xs text-slate-500"><i class="fa-regular fa-calendar mr-1"></i> October 2026</span>
                    </div>
                    <h1 class="text-3xl sm:text-4xl font-black text-slate-900 tracking-tight">Personal Finance Ledger, Debts & Multi-Category Expenses</h1>
                    <p class="text-base text-slate-600 mt-2">Comprehensive personal financial control, tracking counterparty debts, installment repayments, categorical expenses, and monthly budget analytics.</p>
                  </div>

                  <div class="grid grid-cols-1 md:grid-cols-3 gap-4 mb-10">
                    <div class="p-4 rounded-xl bg-slate-50 border border-slate-200">
                      <div class="text-xs font-bold text-slate-500 uppercase">Debt Management</div>
                      <div class="text-lg font-bold text-slate-900 mt-1"><i class="fa-solid fa-hand-holding-dollar text-emerald-600 mr-2"></i>Debtors & Creditors</div>
                    </div>
                    <div class="p-4 rounded-xl bg-slate-50 border border-slate-200">
                      <div class="text-xs font-bold text-slate-500 uppercase">Expense Engine</div>
                      <div class="text-lg font-bold text-slate-900 mt-1"><i class="fa-solid fa-receipt text-amber-600 mr-2"></i>Categorical Spend</div>
                    </div>
                    <div class="p-4 rounded-xl bg-slate-50 border border-slate-200">
                      <div class="text-xs font-bold text-slate-500 uppercase">Analytics</div>
                      <div class="text-lg font-bold text-slate-900 mt-1"><i class="fa-solid fa-chart-pie text-indigo-600 mr-2"></i>Cashflow Trends</div>
                    </div>
                  </div>

                  <div class="space-y-8 text-sm">
                    <div class="p-6 rounded-2xl bg-white border border-slate-200 shadow-sm">
                      <h3 class="text-lg font-bold text-slate-900 flex items-center gap-2 mb-3">
                        <i class="fa-solid fa-users text-emerald-600"></i> Counterparty Debt Tracking & Repayment Logs
                      </h3>
                      <p class="text-slate-600 mb-3">Bidirectional loan and debt ledger allowing you to track both money lent out and money borrowed from colleagues, friends, or institutions.</p>
                      <ul class="list-disc pl-5 space-y-1.5 text-slate-600">
                        <li><strong>Person Catalog:</strong> Manage counterparty contact profiles, phone numbers, and notes at <a href="/DebtPerson" class="text-indigo-600 font-semibold hover:underline">/DebtPerson</a>.</li>
                        <li><strong>Installment Schedules:</strong> Record partial repayments with timestamps and balance updates at <a href="/Debt" class="text-indigo-600 font-semibold hover:underline">/Debt</a>.</li>
                        <li><strong>Settlement Status:</strong> Automatic status transitions between Pending, Partially Paid, and Fully Settled.</li>
                      </ul>
                    </div>

                    <div class="p-6 rounded-2xl bg-white border border-slate-200 shadow-sm">
                      <h3 class="text-lg font-bold text-slate-900 flex items-center gap-2 mb-3">
                        <i class="fa-solid fa-tags text-amber-600"></i> Multi-Category Expense Ledger
                      </h3>
                      <p class="text-slate-600 mb-3">Track daily expenditures across customized life categories with receipt storage and payment method tags.</p>
                      <ul class="list-disc pl-5 space-y-1.5 text-slate-600">
                        <li><strong>Dynamic Categories:</strong> Create and color-code categories (Housing, Groceries, Utilities, Entertainment) at <a href="/ExpenseCategory" class="text-indigo-600 font-semibold hover:underline">/ExpenseCategory</a>.</li>
                        <li><strong>Transaction Records:</strong> Log amounts, payment currencies, transaction dates, and itemized notes at <a href="/Expense" class="text-indigo-600 font-semibold hover:underline">/Expense</a>.</li>
                        <li><strong>Dual Storage Architecture:</strong> Relational PostgreSQL records for indexed queries paired with flexible MongoDB documents for category metadata.</li>
                      </ul>
                    </div>

                    <div class="p-6 rounded-2xl bg-white border border-slate-200 shadow-sm">
                      <h3 class="text-lg font-bold text-slate-900 flex items-center gap-2 mb-3">
                        <i class="fa-solid fa-chart-column text-indigo-600"></i> Financial Analytics & Visual Breakdowns
                      </h3>
                      <p class="text-slate-600 mb-3">Gain actionable insights into your spending habits with real-time aggregates and chart representations.</p>
                      <ul class="list-disc pl-5 space-y-1.5 text-slate-600">
                        <li><strong>Monthly Burn Rate:</strong> Compare current month expenses against previous periods at <a href="/Analytics" class="text-indigo-600 font-semibold hover:underline">/Analytics</a>.</li>
                        <li><strong>Category Distribution:</strong> Interactive breakdowns pinpointing where money flows each billing cycle.</li>
                      </ul>
                    </div>
                  </div>
                </div>
                """
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Version 1.2 — Career Resume Engine, Cloud Vault & Automated Web Crawlers",
                Slug = "v1-2-career-resumes",
                Status = PageStatus.Published,
                CreatedAt = new DateTime(2026, 10, 19, 9, 0, 0, DateTimeKind.Utc),
                Content = """
                <div class="py-6 px-4 max-w-4xl mx-auto font-sans leading-relaxed text-slate-800">
                  <div class="border-b border-slate-200 pb-6 mb-8">
                    <div class="flex items-center gap-2 mb-3">
                      <span class="px-3 py-1 bg-blue-50 text-blue-700 font-bold text-xs rounded-full border border-blue-200">Release v1.2</span>
                      <span class="text-xs text-slate-500"><i class="fa-regular fa-calendar mr-1"></i> October 2026</span>
                    </div>
                    <h1 class="text-3xl sm:text-4xl font-black text-slate-900 tracking-tight">Career Engine, Cloud Document Vault & Automated Web Crawlers</h1>
                    <p class="text-base text-slate-600 mt-2">Empowering professionals with an interactive CV builder, secure Azure Blob document vaulting, and automated web background crawlers for jobs and real estate.</p>
                  </div>

                  <div class="grid grid-cols-1 md:grid-cols-3 gap-4 mb-10">
                    <div class="p-4 rounded-xl bg-slate-50 border border-slate-200">
                      <div class="text-xs font-bold text-slate-500 uppercase">CV Builder</div>
                      <div class="text-lg font-bold text-slate-900 mt-1"><i class="fa-solid fa-file-lines text-blue-600 mr-2"></i>Structured Resumes</div>
                    </div>
                    <div class="p-4 rounded-xl bg-slate-50 border border-slate-200">
                      <div class="text-xs font-bold text-slate-500 uppercase">Storage Vault</div>
                      <div class="text-lg font-bold text-slate-900 mt-1"><i class="fa-solid fa-cloud-arrow-up text-indigo-600 mr-2"></i>Blob Storage</div>
                    </div>
                    <div class="p-4 rounded-xl bg-slate-50 border border-slate-200">
                      <div class="text-xs font-bold text-slate-500 uppercase">Automation</div>
                      <div class="text-lg font-bold text-slate-900 mt-1"><i class="fa-solid fa-spider text-purple-600 mr-2"></i>Web Crawlers</div>
                    </div>
                  </div>

                  <div class="space-y-8 text-sm">
                    <div class="p-6 rounded-2xl bg-white border border-slate-200 shadow-sm">
                      <h3 class="text-lg font-bold text-slate-900 flex items-center gap-2 mb-3">
                        <i class="fa-solid fa-id-card text-blue-600"></i> Interactive Resume Builder & Experience Timeline
                      </h3>
                      <p class="text-slate-600 mb-3">Create tailored professional CVs with structured experience modules, education credentials, skill matrices, and expanded description lengths.</p>
                      <ul class="list-disc pl-5 space-y-1.5 text-slate-600">
                        <li><strong>Schema Migration:</strong> Executed EF Core migration <code class="text-xs bg-slate-100 px-1.5 py-0.5 rounded">resumeChangeDescSize</code> expanding job descriptions to support extensive project narratives.</li>
                        <li><strong>Resume Dashboard:</strong> Build, preview, and update profiles dynamically at <a href="/Resume" class="text-indigo-600 font-semibold hover:underline">/Resume</a>.</li>
                        <li><strong>Application Tracking:</strong> Track submission statuses, company interview stages, and feedback notes at <a href="/Company" class="text-indigo-600 font-semibold hover:underline">/Company</a>.</li>
                      </ul>
                    </div>

                    <div class="p-6 rounded-2xl bg-white border border-slate-200 shadow-sm">
                      <h3 class="text-lg font-bold text-slate-900 flex items-center gap-2 mb-3">
                        <i class="fa-solid fa-file-pdf text-red-600"></i> Azure Blob Storage Vault & File Explorer
                      </h3>
                      <p class="text-slate-600 mb-3">High-speed file management supporting PDF resumes, portfolio assets, and certification documents backed by Azure Blob Storage.</p>
                      <ul class="list-disc pl-5 space-y-1.5 text-slate-600">
                        <li><strong>File Vault:</strong> Upload, download, and manage encrypted career documents via <a href="/ResumeFile" class="text-indigo-600 font-semibold hover:underline">/ResumeFile</a>.</li>
                        <li><strong>File Explorer:</strong> Browse system attachments and document hierarchies in <a href="/FileExplorer" class="text-indigo-600 font-semibold hover:underline">/FileExplorer</a>.</li>
                      </ul>
                    </div>

                    <div class="p-6 rounded-2xl bg-white border border-slate-200 shadow-sm">
                      <h3 class="text-lg font-bold text-slate-900 flex items-center gap-2 mb-3">
                        <i class="fa-solid fa-robot text-purple-600"></i> Automated Crawlers: Job Search & Real Estate
                      </h3>
                      <p class="text-slate-600 mb-3">Background scraping engines tailored for finding remote engineering opportunities and real estate rentals.</p>
                      <ul class="list-disc pl-5 space-y-1.5 text-slate-600">
                        <li><strong>Job Crawler:</strong> Background harvesting of matching tech job listings at <a href="/JobCrawler" class="text-indigo-600 font-semibold hover:underline">/JobCrawler</a>.</li>
                        <li><strong>House Crawler:</strong> Automated scraping of rental platforms with credential vaults and match alerts at <a href="/HouseCrawler" class="text-indigo-600 font-semibold hover:underline">/HouseCrawler</a>.</li>
                      </ul>
                    </div>
                  </div>
                </div>
                """
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Version 1.3 — Habit Formation, Streak Calculations & Life Logging",
                Slug = "v1-3-habit-streaks",
                Status = PageStatus.Published,
                CreatedAt = new DateTime(2026, 10, 20, 11, 0, 0, DateTimeKind.Utc),
                Content = """
                <div class="py-6 px-4 max-w-4xl mx-auto font-sans leading-relaxed text-slate-800">
                  <div class="border-b border-slate-200 pb-6 mb-8">
                    <div class="flex items-center gap-2 mb-3">
                      <span class="px-3 py-1 bg-amber-50 text-amber-700 font-bold text-xs rounded-full border border-amber-200">Release v1.3</span>
                      <span class="text-xs text-slate-500"><i class="fa-regular fa-calendar mr-1"></i> October 2026</span>
                    </div>
                    <h1 class="text-3xl sm:text-4xl font-black text-slate-900 tracking-tight">Habit Formation, Streak Calculations & Life Logging</h1>
                    <p class="text-base text-slate-600 mt-2">Empowering daily discipline through configurable habit frequencies, one-click completion logging, automated streak analytics, and calendar integration.</p>
                  </div>

                  <div class="grid grid-cols-1 md:grid-cols-3 gap-4 mb-10">
                    <div class="p-4 rounded-xl bg-slate-50 border border-slate-200">
                      <div class="text-xs font-bold text-slate-500 uppercase">Habit Engine</div>
                      <div class="text-lg font-bold text-slate-900 mt-1"><i class="fa-solid fa-repeat text-amber-600 mr-2"></i>Frequency Models</div>
                    </div>
                    <div class="p-4 rounded-xl bg-slate-50 border border-slate-200">
                      <div class="text-xs font-bold text-slate-500 uppercase">Streak Metrics</div>
                      <div class="text-lg font-bold text-slate-900 mt-1"><i class="fa-solid fa-fire-flame-curved text-orange-600 mr-2"></i>Consistency Math</div>
                    </div>
                    <div class="p-4 rounded-xl bg-slate-50 border border-slate-200">
                      <div class="text-xs font-bold text-slate-500 uppercase">Planning</div>
                      <div class="text-lg font-bold text-slate-900 mt-1"><i class="fa-solid fa-calendar-days text-indigo-600 mr-2"></i>Calendar Matrix</div>
                    </div>
                  </div>

                  <div class="space-y-8 text-sm">
                    <div class="p-6 rounded-2xl bg-white border border-slate-200 shadow-sm">
                      <h3 class="text-lg font-bold text-slate-900 flex items-center gap-2 mb-3">
                        <i class="fa-solid fa-bullseye text-amber-600"></i> Configurable Habit Trackers & Frequency Targets
                      </h3>
                      <p class="text-slate-600 mb-3">Designed for both binary habits (e.g., Reading, Meditation) and quantifiable goals (e.g., Drink 2500 ml Water).</p>
                      <ul class="list-disc pl-5 space-y-1.5 text-slate-600">
                        <li><strong>Schema Expansion:</strong> Migration <code class="text-xs bg-slate-100 px-1.5 py-0.5 rounded">ExpandHabitTrackerAndLog</code> introduced target counts, measurement units, and custom frequencies.</li>
                        <li><strong>Habit Hub:</strong> Create, manage, and inspect habit performance at <a href="/Habit" class="text-indigo-600 font-semibold hover:underline">/Habit</a>.</li>
                        <li><strong>Custom Intervals:</strong> Support for Daily, Weekdays Only, Weekly, and Monthly cadence schedules.</li>
                      </ul>
                    </div>

                    <div class="p-6 rounded-2xl bg-white border border-slate-200 shadow-sm">
                      <h3 class="text-lg font-bold text-slate-900 flex items-center gap-2 mb-3">
                        <i class="fa-solid fa-bolt text-orange-600"></i> Automatic Streak Math & Daily Completion Logs
                      </h3>
                      <p class="text-slate-600 mb-3">Intelligent background algorithms compute streaks and completion consistency without performance penalties.</p>
                      <ul class="list-disc pl-5 space-y-1.5 text-slate-600">
                        <li><strong>Current vs Longest Streak:</strong> Dynamically calculates consecutive days completed and remembers all-time records.</li>
                        <li><strong>One-Click Check-in:</strong> Instant completion check-in with journal notes for reflective tracking.</li>
                        <li><strong>Grace Period Logic:</strong> Prevents premature streak reset if log is completed before day boundary transitions.</li>
                      </ul>
                    </div>

                    <div class="p-6 rounded-2xl bg-white border border-slate-200 shadow-sm">
                      <h3 class="text-lg font-bold text-slate-900 flex items-center gap-2 mb-3">
                        <i class="fa-solid fa-calendar-check text-indigo-600"></i> Calendar Sync & Scheduling
                      </h3>
                      <p class="text-slate-600 mb-3">Unifies daily routines, scheduled events, and task deadlines into a single visual calendar.</p>
                      <ul class="list-disc pl-5 space-y-1.5 text-slate-600">
                        <li><strong>Event Timeline:</strong> Plan milestones and recurring events at <a href="/Event" class="text-indigo-600 font-semibold hover:underline">/Event</a>.</li>
                        <li><strong>Unified Calendar:</strong> View events and daily commitments in <a href="/Calendar" class="text-indigo-600 font-semibold hover:underline">/Calendar</a>.</li>
                      </ul>
                    </div>
                  </div>
                </div>
                """
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Version 1.4 — Pomodoro Focus Engine, Task Integration & Ambient Zen Mode",
                Slug = "v1-4-pomodoro-focus",
                Status = PageStatus.Published,
                CreatedAt = new DateTime(2026, 10, 20, 13, 0, 0, DateTimeKind.Utc),
                Content = """
                <div class="py-6 px-4 max-w-4xl mx-auto font-sans leading-relaxed text-slate-800">
                  <div class="border-b border-slate-200 pb-6 mb-8">
                    <div class="flex items-center gap-2 mb-3">
                      <span class="px-3 py-1 bg-red-50 text-red-700 font-bold text-xs rounded-full border border-red-200">Release v1.4</span>
                      <span class="text-xs text-slate-500"><i class="fa-regular fa-calendar mr-1"></i> October 2026</span>
                    </div>
                    <h1 class="text-3xl sm:text-4xl font-black text-slate-900 tracking-tight">Pomodoro Focus Engine, Task Integration & Zen Mode</h1>
                    <p class="text-base text-slate-600 mt-2">Boost deep work productivity with an interactive Pomodoro focus timer, seamless task backlog linkage, audio notifications, and distraction-free fullscreen Zen mode.</p>
                  </div>

                  <div class="grid grid-cols-1 md:grid-cols-3 gap-4 mb-10">
                    <div class="p-4 rounded-xl bg-slate-50 border border-slate-200">
                      <div class="text-xs font-bold text-slate-500 uppercase">Focus Intervals</div>
                      <div class="text-lg font-bold text-slate-900 mt-1"><i class="fa-solid fa-stopwatch text-red-600 mr-2"></i>25m / 5m / 15m</div>
                    </div>
                    <div class="p-4 rounded-xl bg-slate-50 border border-slate-200">
                      <div class="text-xs font-bold text-slate-500 uppercase">Task Integration</div>
                      <div class="text-lg font-bold text-slate-900 mt-1"><i class="fa-solid fa-link text-indigo-600 mr-2"></i>Direct Linkage</div>
                    </div>
                    <div class="p-4 rounded-xl bg-slate-50 border border-slate-200">
                      <div class="text-xs font-bold text-slate-500 uppercase">Environment</div>
                      <div class="text-lg font-bold text-slate-900 mt-1"><i class="fa-solid fa-expand text-emerald-600 mr-2"></i>Zen Fullscreen</div>
                    </div>
                  </div>

                  <div class="space-y-8 text-sm">
                    <div class="p-6 rounded-2xl bg-white border border-slate-200 shadow-sm">
                      <h3 class="text-lg font-bold text-slate-900 flex items-center gap-2 mb-3">
                        <i class="fa-solid fa-clock text-red-600"></i> Interactive Pomodoro Focus Engine
                      </h3>
                      <p class="text-slate-600 mb-3">Structured time management adhering to classic Pomodoro technique: 25 minutes of deep focus followed by automated rest cycles.</p>
                      <ul class="list-disc pl-5 space-y-1.5 text-slate-600">
                        <li><strong>Schema Delivery:</strong> Added via migrations <code class="text-xs bg-slate-100 px-1.5 py-0.5 rounded">AddPomodoroFeature</code> and <code class="text-xs bg-slate-100 px-1.5 py-0.5 rounded">ConnectPomodoroToTaskAndNullableHabitTarget</code>.</li>
                        <li><strong>Custom Durations:</strong> Configure personal focus, short break, and long break durations at <a href="/Pomodoro" class="text-indigo-600 font-semibold hover:underline">/Pomodoro</a>.</li>
                        <li><strong>Session Telemetry:</strong> Log actual minutes focused, interrupted sessions, and completion reflections in SQL database.</li>
                      </ul>
                    </div>

                    <div class="p-6 rounded-2xl bg-white border border-slate-200 shadow-sm">
                      <h3 class="text-lg font-bold text-slate-900 flex items-center gap-2 mb-3">
                        <i class="fa-solid fa-list-check text-indigo-600"></i> Task Backlog Linkage
                      </h3>
                      <p class="text-slate-600 mb-3">Directly bind active focus sessions to actionable items from your personal task board.</p>
                      <ul class="list-disc pl-5 space-y-1.5 text-slate-600">
                        <li><strong>Target Task Selection:</strong> Pick any pending task from <a href="/Task" class="text-indigo-600 font-semibold hover:underline">/Task</a> to display directly inside the timer.</li>
                        <li><strong>Automatic Task Completion:</strong> Option to mark associated task as done upon finishing designated focus intervals.</li>
                        <li><strong>Effort Accounting:</strong> Aggregates total Pomodoros invested per project or ticket.</li>
                      </ul>
                    </div>

                    <div class="p-6 rounded-2xl bg-white border border-slate-200 shadow-sm">
                      <h3 class="text-lg font-bold text-slate-900 flex items-center gap-2 mb-3">
                        <i class="fa-solid fa-mountain-sun text-emerald-600"></i> Distraction-Free Fullscreen Zen Mode
                      </h3>
                      <p class="text-slate-600 mb-3">Eliminates UI noise to help you enter and sustain uninterrupted flow state.</p>
                      <ul class="list-disc pl-5 space-y-1.5 text-slate-600">
                        <li><strong>Zen Mode:</strong> 1-click transition hiding sidebars, banners, and notifications.</li>
                        <li><strong>Ambient Audio Alerts:</strong> Crisp chime audio signals indicating interval completions.</li>
                      </ul>
                    </div>
                  </div>
                </div>
                """
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Version 1.5 — Fitness Engine, Workout Logging & Body Composition Tracking",
                Slug = "v1-5-fitness-workouts",
                Status = PageStatus.Published,
                CreatedAt = new DateTime(2026, 10, 20, 14, 0, 0, DateTimeKind.Utc),
                Content = """
                <div class="py-6 px-4 max-w-4xl mx-auto font-sans leading-relaxed text-slate-800">
                  <div class="border-b border-slate-200 pb-6 mb-8">
                    <div class="flex items-center gap-2 mb-3">
                      <span class="px-3 py-1 bg-cyan-50 text-cyan-700 font-bold text-xs rounded-full border border-cyan-200">Release v1.5</span>
                      <span class="text-xs text-slate-500"><i class="fa-regular fa-calendar mr-1"></i> October 2026</span>
                    </div>
                    <h1 class="text-3xl sm:text-4xl font-black text-slate-900 tracking-tight">Fitness Engine, Workout Logging & Body Composition Tracking</h1>
                    <p class="text-base text-slate-600 mt-2">A holistic fitness tracking ecosystem including gym course catalogs, session duration tracking, exercise volume logs, and time-series body weight tracking with automatic BMI calculations.</p>
                  </div>

                  <div class="grid grid-cols-1 md:grid-cols-3 gap-4 mb-10">
                    <div class="p-4 rounded-xl bg-slate-50 border border-slate-200">
                      <div class="text-xs font-bold text-slate-500 uppercase">Workout Hub</div>
                      <div class="text-lg font-bold text-slate-900 mt-1"><i class="fa-solid fa-dumbbell text-cyan-600 mr-2"></i>Session Tracking</div>
                    </div>
                    <div class="p-4 rounded-xl bg-slate-50 border border-slate-200">
                      <div class="text-xs font-bold text-slate-500 uppercase">Gym Courses</div>
                      <div class="text-lg font-bold text-slate-900 mt-1"><i class="fa-solid fa-person-running text-emerald-600 mr-2"></i>Curated Routines</div>
                    </div>
                    <div class="p-4 rounded-xl bg-slate-50 border border-slate-200">
                      <div class="text-xs font-bold text-slate-500 uppercase">Body Composition</div>
                      <div class="text-lg font-bold text-slate-900 mt-1"><i class="fa-solid fa-weight-scale text-indigo-600 mr-2"></i>Weight & BMI</div>
                    </div>
                  </div>

                  <div class="space-y-8 text-sm">
                    <div class="p-6 rounded-2xl bg-white border border-slate-200 shadow-sm">
                      <h3 class="text-lg font-bold text-slate-900 flex items-center gap-2 mb-3">
                        <i class="fa-solid fa-heart-pulse text-cyan-600"></i> Workout Sessions & Muscle Group Targeting
                      </h3>
                      <p class="text-slate-600 mb-3">Record comprehensive workout logs with exercise names, sets, reps, resistance weights, and overall intensity.</p>
                      <ul class="list-disc pl-5 space-y-1.5 text-slate-600">
                        <li><strong>Workout Dashboard:</strong> Active session recording, exercise volume metrics, and personal records at <a href="/Workout" class="text-indigo-600 font-semibold hover:underline">/Workout</a>.</li>
                        <li><strong>Time-Series Storage:</strong> Workouts persisted in MongoDB <code class="text-xs bg-slate-100 px-1.5 py-0.5 rounded">UserTrainingSession</code> with automatic user scoping.</li>
                        <li><strong>Muscle Group Breakdown:</strong> Tag sets by Chest, Back, Legs, Shoulders, Core, or Cardio for balanced split tracking.</li>
                      </ul>
                    </div>

                    <div class="p-6 rounded-2xl bg-white border border-slate-200 shadow-sm">
                      <h3 class="text-lg font-bold text-slate-900 flex items-center gap-2 mb-3">
                        <i class="fa-solid fa-award text-emerald-600"></i> Gym Courses & Global Training Programs
                      </h3>
                      <p class="text-slate-600 mb-3">Explore curated fitness routines designed by professionals or manage organizational course catalogs.</p>
                      <ul class="list-disc pl-5 space-y-1.5 text-slate-600">
                        <li><strong>Course Catalog:</strong> Browse available workout programs and enroll at <a href="/GymCourses" class="text-indigo-600 font-semibold hover:underline">/GymCourses</a>.</li>
                        <li><strong>Course Administration:</strong> Admin portal for curating structured programs, difficulty levels, and syllabus at <a href="/GymAdmin" class="text-indigo-600 font-semibold hover:underline">/GymAdmin</a>.</li>
                      </ul>
                    </div>

                    <div class="p-6 rounded-2xl bg-white border border-slate-200 shadow-sm">
                      <h3 class="text-lg font-bold text-slate-900 flex items-center gap-2 mb-3">
                        <i class="fa-solid fa-chart-line text-indigo-600"></i> Body Weight Journal & Real-Time BMI Calculation
                      </h3>
                      <p class="text-slate-600 mb-3">Track weight fluctuations over time to measure muscle gain, fat loss, and body composition changes.</p>
                      <ul class="list-disc pl-5 space-y-1.5 text-slate-600">
                        <li><strong>Weight Progression:</strong> Log daily or weekly weigh-ins stored in <code class="text-xs bg-slate-100 px-1.5 py-0.5 rounded">UserWeightLog</code>.</li>
                        <li><strong>BMI Categorization:</strong> Automatically calculates Body Mass Index and displays normal, overweight, or athletic health brackets.</li>
                      </ul>
                    </div>
                  </div>
                </div>
                """
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Version 2.0 — Enterprise Multi-Tenancy, Tenant Isolation & Hangfire Automation",
                Slug = "v2-0-multi-tenancy",
                Status = PageStatus.Published,
                CreatedAt = new DateTime(2026, 10, 20, 15, 0, 0, DateTimeKind.Utc),
                Content = """
                <div class="py-6 px-4 max-w-4xl mx-auto font-sans leading-relaxed text-slate-800">
                  <div class="border-b border-slate-200 pb-6 mb-8">
                    <div class="flex items-center gap-2 mb-3">
                      <span class="px-3 py-1 bg-purple-50 text-purple-700 font-bold text-xs rounded-full border border-purple-200">Release v2.0</span>
                      <span class="text-xs text-slate-500"><i class="fa-regular fa-calendar mr-1"></i> October 2026</span>
                    </div>
                    <h1 class="text-3xl sm:text-4xl font-black text-slate-900 tracking-tight">Enterprise Multi-Tenancy, Tenant Isolation & Hangfire Automation</h1>
                    <p class="text-base text-slate-600 mt-2">A foundational architectural evolution introducing tenant workspace isolation across PostgreSQL and MongoDB, admin tenant switching, and background distributed job processing via Hangfire.</p>
                  </div>

                  <div class="grid grid-cols-1 md:grid-cols-3 gap-4 mb-10">
                    <div class="p-4 rounded-xl bg-slate-50 border border-slate-200">
                      <div class="text-xs font-bold text-slate-500 uppercase">Tenant Isolation</div>
                      <div class="text-lg font-bold text-slate-900 mt-1"><i class="fa-solid fa-layer-group text-purple-600 mr-2"></i>Global Query Filters</div>
                    </div>
                    <div class="p-4 rounded-xl bg-slate-50 border border-slate-200">
                      <div class="text-xs font-bold text-slate-500 uppercase">Admin Control</div>
                      <div class="text-lg font-bold text-slate-900 mt-1"><i class="fa-solid fa-user-secret text-indigo-600 mr-2"></i>Impersonation</div>
                    </div>
                    <div class="p-4 rounded-xl bg-slate-50 border border-slate-200">
                      <div class="text-xs font-bold text-slate-500 uppercase">Background Jobs</div>
                      <div class="text-lg font-bold text-slate-900 mt-1"><i class="fa-solid fa-gears text-emerald-600 mr-2"></i>Hangfire Engine</div>
                    </div>
                  </div>

                  <div class="space-y-8 text-sm">
                    <div class="p-6 rounded-2xl bg-white border border-slate-200 shadow-sm">
                      <h3 class="text-lg font-bold text-slate-900 flex items-center gap-2 mb-3">
                        <i class="fa-solid fa-shield-virus text-purple-600"></i> Hybrid Multi-Tenancy Engine (SQL & NoSQL)
                      </h3>
                      <p class="text-slate-600 mb-3">Complete architectural isolation guaranteeing that users only see and modify data belonging to their active workspace or tenant context.</p>
                      <ul class="list-disc pl-5 space-y-1.5 text-slate-600">
                        <li><strong>EF Core Global Query Filters:</strong> Entities implementing <code class="text-xs bg-slate-100 px-1.5 py-0.5 rounded">ITenantEntity</code> are automatically filtered by <code class="text-xs bg-slate-100 px-1.5 py-0.5 rounded">TenantId == _currentTenant.TenantId</code> in PostgreSQL.</li>
                        <li><strong>MongoDB Scoped Repository:</strong> Document repositories enforce tenant scoping on read/write operations while providing seamless fallback to <code class="text-xs bg-slate-100 px-1.5 py-0.5 rounded">UserId</code> for pre-existing documents.</li>
                        <li><strong>Automatic Interception:</strong> <code class="text-xs bg-slate-100 px-1.5 py-0.5 rounded">SaveChangesInterceptor</code> stamps <code class="text-xs bg-slate-100 px-1.5 py-0.5 rounded">TenantId</code> on newly created entities automatically.</li>
                        <li><strong>Schema Migration:</strong> Applied via migration <code class="text-xs bg-slate-100 px-1.5 py-0.5 rounded">20261020143950_AddMultiTenancySupport</code> with optimized indexes on all user-scoped tables.</li>
                      </ul>
                    </div>

                    <div class="p-6 rounded-2xl bg-white border border-slate-200 shadow-sm">
                      <h3 class="text-lg font-bold text-slate-900 flex items-center gap-2 mb-3">
                        <i class="fa-solid fa-right-left text-indigo-600"></i> Admin Tenant Switching & User Impersonation
                      </h3>
                      <p class="text-slate-600 mb-3">Empowers system administrators to diagnose issues and assist users by switching workspace contexts seamlessly.</p>
                      <ul class="list-disc pl-5 space-y-1.5 text-slate-600">
                        <li><strong>Tenant Control Center:</strong> Accessible to Administrators at <a href="/Tenant" class="text-indigo-600 font-semibold hover:underline">/Tenant</a> with live tenant metrics and active context badges.</li>
                        <li><strong>Cookie-Based Switching:</strong> Context switched instantly via encrypted cookie (<code class="text-xs bg-slate-100 px-1.5 py-0.5 rounded">X-Selected-Tenant</code>) without requiring account logouts.</li>
                        <li><strong>One-Click Context Reset:</strong> Instantly revert back to personal administrator workspace with a single click.</li>
                      </ul>
                    </div>

                    <div class="p-6 rounded-2xl bg-white border border-slate-200 shadow-sm">
                      <h3 class="text-lg font-bold text-slate-900 flex items-center gap-2 mb-3">
                        <i class="fa-solid fa-server text-emerald-600"></i> Hangfire Background Automation & Tenant Backfill
                      </h3>
                      <p class="text-slate-600 mb-3">Enterprise background worker integration enabled in production environments with strict administrator security.</p>
                      <ul class="list-disc pl-5 space-y-1.5 text-slate-600">
                        <li><strong>Production Dashboard Security:</strong> Protected by <code class="text-xs bg-slate-100 px-1.5 py-0.5 rounded">HangfireAdminAuthorizationFilter</code> ensuring only authenticated Admins can view <a href="/hangfire" class="text-indigo-600 font-semibold hover:underline">/hangfire</a>.</li>
                        <li><strong>Batched Zero-Lock Backfill:</strong> Automated job (<code class="text-xs bg-slate-100 px-1.5 py-0.5 rounded">ITenantBackfillJob</code>) processes historical SQL rows and MongoDB documents in 500-item chunks.</li>
                        <li><strong>Admin Trigger & Recurring Schedule:</strong> Admins can trigger backfill immediately via <a href="/Tenant" class="text-indigo-600 font-semibold hover:underline">/Tenant</a> or let it run automatically daily at 02:00 UTC.</li>
                      </ul>
                    </div>
                  </div>
                </div>
                """
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Version 2.1 — Smart Kitchen Pantry, Stock Movement Ledger & Real-Time Shopping List",
                Slug = "v2-1-nutrition-pantry",
                Status = PageStatus.Published,
                CreatedAt = new DateTime(2026, 10, 20, 16, 0, 0, DateTimeKind.Utc),
                Content = """
                <div class="py-6 px-4 max-w-4xl mx-auto font-sans leading-relaxed text-slate-800">
                  <div class="border-b border-slate-200 pb-6 mb-8">
                    <div class="flex items-center gap-2 mb-3">
                      <span class="px-3 py-1 bg-teal-50 text-teal-700 font-bold text-xs rounded-full border border-teal-200">Release v2.1</span>
                      <span class="text-xs text-slate-500"><i class="fa-regular fa-calendar mr-1"></i> October 2026</span>
                    </div>
                    <h1 class="text-3xl sm:text-4xl font-black text-slate-900 tracking-tight">Smart Kitchen Pantry, Stock Movement Ledger & Real-Time Shopping List</h1>
                    <p class="text-base text-slate-600 mt-2">Transforming household nutrition management with automated stock movement ledger, smart replenishment shopping lists, multi-user real-time sync, and the comprehensive design UI kit.</p>
                  </div>

                  <div class="grid grid-cols-1 md:grid-cols-3 gap-4 mb-10">
                    <div class="p-4 rounded-xl bg-slate-50 border border-slate-200">
                      <div class="text-xs font-bold text-slate-500 uppercase">Pantry Ledger</div>
                      <div class="text-lg font-bold text-slate-900 mt-1"><i class="fa-solid fa-boxes-stacked text-teal-600 mr-2"></i>Stock Movements</div>
                    </div>
                    <div class="p-4 rounded-xl bg-slate-50 border border-slate-200">
                      <div class="text-xs font-bold text-slate-500 uppercase">Shopping List</div>
                      <div class="text-lg font-bold text-slate-900 mt-1"><i class="fa-solid fa-cart-shopping text-blue-600 mr-2"></i>Smart Replenish</div>
                    </div>
                    <div class="p-4 rounded-xl bg-slate-50 border border-slate-200">
                      <div class="text-xs font-bold text-slate-500 uppercase">Design System</div>
                      <div class="text-lg font-bold text-slate-900 mt-1"><i class="fa-solid fa-cubes text-purple-600 mr-2"></i>UI Component Kit</div>
                    </div>
                  </div>

                  <div class="space-y-8 text-sm">
                    <div class="p-6 rounded-2xl bg-white border border-slate-200 shadow-sm">
                      <h3 class="text-lg font-bold text-slate-900 flex items-center gap-2 mb-3">
                        <i class="fa-solid fa-kitchen-set text-teal-600"></i> Kitchen Stock & Double-Entry Movement Ledger
                      </h3>
                      <p class="text-slate-600 mb-3">Track ingredient inventory across physical kitchen zones (Fridge, Freezer, Pantry) with strict movement auditing.</p>
                      <ul class="list-disc pl-5 space-y-1.5 text-slate-600">
                        <li><strong>Zone Classification:</strong> Items partitioned by physical location, optional purchase costs, and expiration dates.</li>
                        <li><strong>Movement Ledger:</strong> Quantity changes tracked as immutable movements: Purchases, MealLog consumption, Recipe deductions, Waste tracking, and Manual adjustments.</li>
                        <li><strong>Automated Meal Deductions:</strong> Logging a meal at <a href="/Food" class="text-indigo-600 font-semibold hover:underline">/Food</a> automatically deducts used ingredients from stock, with full reversibility upon edit or deletion.</li>
                        <li><strong>Unit Conversion:</strong> Automatic normalization across grams, kilograms, pieces, milliliters, and liters.</li>
                      </ul>
                    </div>

                    <div class="p-6 rounded-2xl bg-white border border-slate-200 shadow-sm">
                      <h3 class="text-lg font-bold text-slate-900 flex items-center gap-2 mb-3">
                        <i class="fa-solid fa-basket-shopping text-blue-600"></i> Smart Shopping List & Real-Time Multi-User Sync
                      </h3>
                      <p class="text-slate-600 mb-3">Automated shopping list generation powered by minimum safety levels and upcoming recipe plans.</p>
                      <ul class="list-disc pl-5 space-y-1.5 text-slate-600">
                        <li><strong>Auto-Generation Engine:</strong> Reorder points automatically calculate missing quantities from planned meals minus current pantry stock, consolidating duplicate items.</li>
                        <li><strong>Aisle Organization:</strong> Items categorized by store section (Produce, Dairy, Bakery, Spices) for rapid in-store navigation.</li>
                        <li><strong>Finish Shopping Flow:</strong> 1-click conversion transferring bought cart items directly into stock inventory as Purchase movements.</li>
                        <li><strong>Real-Time Household Sync:</strong> Multi-user live cart updates via SignalR ensuring family members shopping simultaneously never purchase duplicates.</li>
                      </ul>
                    </div>

                    <div class="p-6 rounded-2xl bg-white border border-slate-200 shadow-sm">
                      <h3 class="text-lg font-bold text-slate-900 flex items-center gap-2 mb-3">
                        <i class="fa-solid fa-shapes text-purple-600"></i> Comprehensive UI Component Showcase
                      </h3>
                      <p class="text-slate-600 mb-3">A unified design system and interactive component showcase living at <a href="/UiComponents" class="text-indigo-600 font-semibold hover:underline">/UiComponents</a>.</p>
                      <ul class="list-disc pl-5 space-y-1.5 text-slate-600">
                        <li><strong>Form Controls:</strong> Searchable multi-select dropdowns, custom date pickers, custom time pickers, and validated inputs.</li>
                        <li><strong>Navigation Elements:</strong> Breadcrumb trails, multi-level tabs, tab groups, and collapsible accordions.</li>
                        <li><strong>Data Display:</strong> Sortable header tables, pagination controls, interactive modals, toasts, and loading skeletons.</li>
                        <li><strong>Icon Showcase:</strong> Centralized icon gallery and utility tools at <a href="/Icon" class="text-indigo-600 font-semibold hover:underline">/Icon</a>.</li>
                      </ul>
                    </div>
                  </div>
                </div>
                """
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Version 2.2 — Modern UI Component Architecture & Design System",
                Slug = "new-ui-architecture",
                Status = PageStatus.Published,
                CreatedAt = new DateTime(2026, 10, 20, 20, 0, 0, DateTimeKind.Utc),
                Content = """
                <div class="py-6 px-4 max-w-4xl mx-auto font-sans leading-relaxed text-slate-800">
                  <div class="border-b border-slate-200 pb-6 mb-8">
                    <div class="flex items-center gap-2 mb-3">
                      <span class="px-3 py-1 bg-purple-50 text-purple-700 font-bold text-xs rounded-full border border-purple-200">Release v2.2</span>
                      <span class="text-xs text-slate-500"><i class="fa-regular fa-calendar mr-1"></i> October 2026</span>
                    </div>
                    <h1 class="text-3xl sm:text-4xl font-black text-slate-900 tracking-tight">Modern UI Component Architecture & Standardized Design System</h1>
                    <p class="text-base text-slate-600 mt-2">Comprehensive UI unification replacing raw controls across all application views with modular, responsive components from the /UiComponents and /Icon design system.</p>
                  </div>

                  <div class="grid grid-cols-1 md:grid-cols-3 gap-4 mb-10">
                    <div class="p-4 rounded-xl bg-slate-50 border border-slate-200">
                      <div class="text-xs font-bold text-slate-500 uppercase">Component Kit</div>
                      <div class="text-lg font-bold text-slate-900 mt-1"><i class="fa-solid fa-cubes text-purple-600 mr-2"></i>20+ Partials</div>
                    </div>
                    <div class="p-4 rounded-xl bg-slate-50 border border-slate-200">
                      <div class="text-xs font-bold text-slate-500 uppercase">Icon Standard</div>
                      <div class="text-lg font-bold text-slate-900 mt-1"><i class="fa-solid fa-icons text-indigo-600 mr-2"></i>FontAwesome 6</div>
                    </div>
                    <div class="p-4 rounded-xl bg-slate-50 border border-slate-200">
                      <div class="text-xs font-bold text-slate-500 uppercase">Design Philosophy</div>
                      <div class="text-lg font-bold text-slate-900 mt-1"><i class="fa-solid fa-palette text-emerald-600 mr-2"></i>Clean & Accessible</div>
                    </div>
                  </div>

                  <div class="space-y-8 text-sm">
                    <div class="p-6 rounded-2xl bg-white border border-slate-200 shadow-sm">
                      <h3 class="text-lg font-bold text-slate-900 flex items-center gap-2 mb-3">
                        <i class="fa-solid fa-sliders text-purple-600"></i> Standardized Inputs, Selectors & Form Controls
                      </h3>
                      <p class="text-slate-600 mb-3">Re-engineered all input controls across Razor views with consistent focus states, validation styling, and assistive icons.</p>
                      <ul class="list-disc pl-5 space-y-1.5 text-slate-600">
                        <li><strong>Input Fields:</strong> Text, email, password, and number inputs styled with <code>rounded-xl border-slate-200 focus:ring-2 focus:ring-brand-500/20</code>.</li>
                        <li><strong>Instant Clear Action:</strong> Search and filter boxes feature built-in clear buttons for rapid form resetting.</li>
                        <li><strong>Custom Date & Time Pickers:</strong> Native-friendly and TomSelect dropdowns offering high precision without clunky legacy controls.</li>
                        <li><strong>Zero-Emoji Rule:</strong> Strict replacement of all Unicode emoji icons with sharp, professional FontAwesome 6 vector glyphs.</li>
                      </ul>
                    </div>

                    <div class="p-6 rounded-2xl bg-white border border-slate-200 shadow-sm">
                      <h3 class="text-lg font-bold text-slate-900 flex items-center gap-2 mb-3">
                        <i class="fa-solid fa-table text-indigo-600"></i> Responsive Data Tables, Filters & Pagination
                      </h3>
                      <p class="text-slate-600 mb-3">Uniform table presentation ensures large datasets across Gym, Food, Expenses, Debts, and Tenants render smoothly on mobile and desktop viewports.</p>
                      <ul class="list-disc pl-5 space-y-1.5 text-slate-600">
                        <li><strong>Table Shells:</strong> Rounded container cards with subtle borders, sticky column headers, and alternating hover rows.</li>
                        <li><strong>Pagination Controls:</strong> Dynamic page-size selectors, current window indicators, and accessible next/previous pagination buttons.</li>
                        <li><strong>Filter Bars:</strong> Clean pill filters and expandable filter drawers with active filter count badges.</li>
                        <li><strong>Empty States:</strong> Informative illustrated fallback placeholders with direct action buttons when query results are empty.</li>
                      </ul>
                    </div>

                    <div class="p-6 rounded-2xl bg-white border border-slate-200 shadow-sm">
                      <h3 class="text-lg font-bold text-slate-900 flex items-center gap-2 mb-3">
                        <i class="fa-solid fa-window-restore text-blue-600"></i> Breadcrumbs, Modals & Side Panel Quick Navigation
                      </h3>
                      <p class="text-slate-600 mb-3">Structural layout enhancements providing fluid navigation and focused contextual workflows.</p>
                      <ul class="list-disc pl-5 space-y-1.5 text-slate-600">
                        <li><strong>Breadcrumb Trails:</strong> Reusable <code>UiComponents/_Breadcrumb</code> providing hierarchical navigation links across all pages.</li>
                        <li><strong>Modal Dialogs:</strong> Accessible modal system with <code>backdrop-blur-xs</code> overlays, focus traps, and ESC key dismissal.</li>
                        <li><strong>Side Panel Search:</strong> Real-time interactive search in the sidebar menu for instantaneous section and controller filtering.</li>
                        <li><strong>Pages Relocation:</strong> Reorganized side panel layout placing published documentation and page management at the end of the menu.</li>
                      </ul>
                    </div>
                  </div>
                </div>
                """
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Version 2.3 — Media Hub & Entertainment Platform (TMDb Integration)",
                Slug = "media-controller-features",
                Status = PageStatus.Published,
                CreatedAt = new DateTime(2026, 10, 20, 22, 0, 0, DateTimeKind.Utc),
                Content = """
                <div class="py-6 px-4 max-w-4xl mx-auto font-sans leading-relaxed text-slate-800">
                  <div class="border-b border-slate-200 pb-6 mb-8">
                    <div class="flex items-center gap-2 mb-3">
                      <span class="px-3 py-1 bg-amber-50 text-amber-700 font-bold text-xs rounded-full border border-amber-200">Release v2.3</span>
                      <span class="text-xs text-slate-500"><i class="fa-regular fa-calendar mr-1"></i> October 2026</span>
                    </div>
                    <h1 class="text-3xl sm:text-4xl font-black text-slate-900 tracking-tight">Media Hub & Entertainment Platform (TMDb Integration)</h1>
                    <p class="text-base text-slate-600 mt-2">A personal tracking and discovery hub for movies, series, and anime powered by The Movie Database (TMDb) API, featuring watchlists, actor following, release calendars, and watch analytics.</p>
                  </div>

                  <div class="grid grid-cols-1 md:grid-cols-3 gap-4 mb-10">
                    <div class="p-4 rounded-xl bg-slate-50 border border-slate-200">
                      <div class="text-xs font-bold text-slate-500 uppercase">Provider</div>
                      <div class="text-lg font-bold text-slate-900 mt-1"><i class="fa-solid fa-film text-amber-600 mr-2"></i>TMDb API v3</div>
                    </div>
                    <div class="p-4 rounded-xl bg-slate-50 border border-slate-200">
                      <div class="text-xs font-bold text-slate-500 uppercase">Media Scope</div>
                      <div class="text-lg font-bold text-slate-900 mt-1"><i class="fa-solid fa-tv text-blue-600 mr-2"></i>Movies, Series & Anime</div>
                    </div>
                    <div class="p-4 rounded-xl bg-slate-50 border border-slate-200">
                      <div class="text-xs font-bold text-slate-500 uppercase">Storage</div>
                      <div class="text-lg font-bold text-slate-900 mt-1"><i class="fa-solid fa-database text-emerald-600 mr-2"></i>NoSQL MongoDB</div>
                    </div>
                  </div>

                  <div class="space-y-8 text-sm">
                    <div class="p-6 rounded-2xl bg-white border border-slate-200 shadow-sm">
                      <h3 class="text-lg font-bold text-slate-900 flex items-center gap-2 mb-3">
                        <i class="fa-solid fa-list-check text-amber-600"></i> Track What You Watch
                      </h3>
                      <p class="text-slate-600 mb-3">Manage movies, series, and anime with status workflows and progress counters.</p>
                      <ul class="list-disc pl-5 space-y-1.5 text-slate-600">
                        <li><strong>Watch Statuses:</strong> Categorize titles under <em>Want to Watch (Wishlist)</em>, <em>Currently Watching</em>, <em>Finished</em>, or <em>Stopped (Dropped)</em>.</li>
                        <li><strong>Episode Progress:</strong> Save the exact season and episode where you stopped watching series or anime.</li>
                        <li><strong>Next Episode Guide:</strong> Computes and prominently displays "Next episode to watch" with a single-click increment button.</li>
                        <li><strong>Ratings & Reviews:</strong> Personal 1 to 10 score rating, written short review notes, and watched date logging at <a href="/Media" class="text-amber-600 font-semibold hover:underline">/Media</a>.</li>
                      </ul>
                    </div>

                    <div class="p-6 rounded-2xl bg-white border border-slate-200 shadow-sm">
                      <h3 class="text-lg font-bold text-slate-900 flex items-center gap-2 mb-3">
                        <i class="fa-solid fa-compass text-blue-600"></i> Find Movies, Series & Anime
                      </h3>
                      <p class="text-slate-600 mb-3">Explore millions of titles with real-time TMDb data, trailers, and streaming provider intelligence.</p>
                      <ul class="list-disc pl-5 space-y-1.5 text-slate-600">
                        <li><strong>Live Search:</strong> Unified multi-entity search supporting Movies, TV Shows, Anime, and Actors/Directors at <a href="/Media/Search" class="text-amber-600 font-semibold hover:underline">/Media/Search</a>.</li>
                        <li><strong>Comprehensive Details:</strong> Story synopsis, cast members, directing crew, genre pills, and embedded YouTube trailers at <a href="/Media/Details" class="text-amber-600 font-semibold hover:underline">/Media/Details</a>.</li>
                        <li><strong>Curated Discovery:</strong> Explore Trending Today, Popular Titles, Top Rated Masterpieces, and Coming Soon releases at <a href="/Media/Discover" class="text-amber-600 font-semibold hover:underline">/Media/Discover</a>.</li>
                        <li><strong>Streaming Availability:</strong> Real-time "Where to Watch" integration displaying provider badges (Netflix, Disney+, Apple TV+, Prime Video).</li>
                      </ul>
                    </div>

                    <div class="p-6 rounded-2xl bg-white border border-slate-200 shadow-sm">
                      <h3 class="text-lg font-bold text-slate-900 flex items-center gap-2 mb-3">
                        <i class="fa-solid fa-users text-purple-600"></i> Follow Actors & Directors
                      </h3>
                      <p class="text-slate-600 mb-3">Never miss a new release from your favorite creators and cast members.</p>
                      <ul class="list-disc pl-5 space-y-1.5 text-slate-600">
                        <li><strong>Follow Roster:</strong> Follow actors and directors with personal notes at <a href="/Media/People" class="text-amber-600 font-semibold hover:underline">/Media/People</a>.</li>
                        <li><strong>Filmography Tracker:</strong> Full credit lists showing how many titles you have already watched (e.g. "5 of 20 watched").</li>
                        <li><strong>Upcoming Project Alerts:</strong> Highlighting announced or in-production projects with premiere dates.</li>
                      </ul>
                    </div>

                    <div class="p-6 rounded-2xl bg-white border border-slate-200 shadow-sm">
                      <h3 class="text-lg font-bold text-slate-900 flex items-center gap-2 mb-3">
                        <i class="fa-solid fa-calendar-days text-emerald-600"></i> Calendar, Industry News & Analytics
                      </h3>
                      <p class="text-slate-600 mb-3">Stay up to date with releases, industry updates, custom playlists, and watch statistics.</p>
                      <ul class="list-disc pl-5 space-y-1.5 text-slate-600">
                        <li><strong>Release Calendar:</strong> Visual monthly calendar for upcoming wishlist releases and season returns at <a href="/Media/Calendar" class="text-amber-600 font-semibold hover:underline">/Media/Calendar</a>.</li>
                        <li><strong>Entertainment News:</strong> Fresh news articles curated from trusted entertainment industry sources at <a href="/Media/News" class="text-amber-600 font-semibold hover:underline">/Media/News</a>.</li>
                        <li><strong>Custom Playlists:</strong> Curate custom collections (e.g., "Friday Night Movies", "Anime Marathon") at <a href="/Media/Lists" class="text-amber-600 font-semibold hover:underline">/Media/Lists</a>.</li>
                        <li><strong>Watch Statistics:</strong> Visual breakdown of hours watched, top genres, favorite actors, and rating score distribution at <a href="/Media/Stats" class="text-amber-600 font-semibold hover:underline">/Media/Stats</a>.</li>
                        <li><strong>Data Portability:</strong> 1-click JSON export and import for seamless personal data backups.</li>
                      </ul>
                    </div>
                  </div>
                </div>
                """
            }
        };
    }
}
