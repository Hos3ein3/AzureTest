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
                var existing = existingPages.FirstOrDefault(p => p.Slug.Equals(page.Slug, StringComparison.OrdinalIgnoreCase));
                if (existing == null)
                {
                    await noSqlUnitOfWork.Pages.AddAsync(page);
                    anyAdded = true;
                }
                else if (existing.Content != page.Content || existing.Title != page.Title)
                {
                    existing.Title = page.Title;
                    existing.Content = page.Content;
                    existing.Status = page.Status;
                    await noSqlUnitOfWork.Pages.UpdateAsync(existing.Id, existing);
                    anyAdded = true;
                }
            }

            if (anyAdded)
            {
                await noSqlUnitOfWork.SaveChangesAsync();
                logger.LogInformation("Documentation pages seeded and synchronized successfully.");
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
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "User Guide: Media & Anime Tracking System",
                Slug = "how-to-use-media-and-anime",
                Status = PageStatus.Published,
                CreatedAt = new DateTime(2026, 10, 21, 10, 0, 0, DateTimeKind.Utc),
                Content = """
                <div class="py-6 px-4 max-w-4xl mx-auto font-sans leading-relaxed text-slate-800">
                  <div class="border-b border-slate-200 pb-6 mb-8">
                    <div class="flex items-center gap-2 mb-3">
                      <span class="px-3 py-1 bg-purple-50 text-purple-700 font-bold text-xs rounded-full border border-purple-200">System Guide</span>
                      <span class="text-xs text-slate-500"><i class="fa-regular fa-clock mr-1"></i> 8 min read</span>
                    </div>
                    <h1 class="text-3xl sm:text-4xl font-black text-slate-900 tracking-tight">How to Use the Media &amp; Anime Tracking System</h1>
                    <p class="text-base text-slate-600 mt-2">A complete guide to searching titles, tracking TV and anime episodes with accordions, following actors and directors, managing custom playlists, and viewing watch analytics.</p>
                  </div>

                  <div class="grid grid-cols-1 md:grid-cols-3 gap-4 mb-10">
                    <div class="p-4 rounded-2xl bg-slate-50 border border-slate-200">
                      <div class="text-xs font-bold text-slate-500 uppercase">Search &amp; Discovery</div>
                      <div class="text-base font-bold text-slate-900 mt-1"><i class="fa-solid fa-magnifying-glass text-purple-600 mr-2"></i>Multi-Entity Search</div>
                      <p class="text-2xs text-slate-500 mt-1">Search movies, series, anime, and people.</p>
                    </div>
                    <div class="p-4 rounded-2xl bg-slate-50 border border-slate-200">
                      <div class="text-xs font-bold text-slate-500 uppercase">Episode Progress</div>
                      <div class="text-base font-bold text-slate-900 mt-1"><i class="fa-solid fa-layer-group text-brand-600 mr-2"></i>Accordion Seasons</div>
                      <p class="text-2xs text-slate-500 mt-1">Track series, whole seasons, or single episodes.</p>
                    </div>
                    <div class="p-4 rounded-2xl bg-slate-50 border border-slate-200">
                      <div class="text-xs font-bold text-slate-500 uppercase">Filmography Sync</div>
                      <div class="text-base font-bold text-slate-900 mt-1"><i class="fa-solid fa-user-check text-emerald-600 mr-2"></i>Follow Creators</div>
                      <p class="text-2xs text-slate-500 mt-1">Track career filmographies and upcoming premieres.</p>
                    </div>
                  </div>

                  <div class="space-y-8 text-sm">
                    <div class="p-6 rounded-2xl bg-white border border-slate-200 shadow-sm space-y-3">
                      <h3 class="text-lg font-bold text-slate-900 flex items-center gap-2">
                        <i class="fa-solid fa-magnifying-glass text-purple-600"></i> 1. Searching &amp; Finding Titles
                      </h3>
                      <p class="text-slate-600 leading-relaxed">
                        Navigate to <a href="/Media/Search" class="text-purple-600 font-semibold hover:underline">/Media/Search</a> to search across movies, series, anime, and people simultaneously.
                      </p>
                      <ul class="list-disc pl-5 space-y-1.5 text-slate-600">
                        <li><strong>Entity Filtering:</strong> Switch between <em>All</em>, <em>Movies</em>, <em>TV Series</em>, <em>Anime</em>, and <em>People</em> pill tabs.</li>
                        <li><strong>Friendly URLs:</strong> Title detail pages support clean slug-based links, e.g. <code>/Media/Details/1429/attack-on-titan?type=Anime</code> or <code>/Media/Details/125988/silo?type=TvSeries</code>.</li>
                        <li><strong>Discover Hub:</strong> Visit <a href="/Media/Discover" class="text-purple-600 font-semibold hover:underline">/Media/Discover</a> to see Trending Today, Top-Rated Classics, and Upcoming Releases.</li>
                      </ul>
                    </div>

                    <div class="p-6 rounded-2xl bg-white border border-slate-200 shadow-sm space-y-3">
                      <h3 class="text-lg font-bold text-slate-900 flex items-center gap-2">
                        <i class="fa-solid fa-list-check text-brand-600"></i> 2. Managing Watch Statuses
                      </h3>
                      <p class="text-slate-600 leading-relaxed">
                        Every title on <a href="/Media" class="text-purple-600 font-semibold hover:underline">/Media</a> can be assigned one of four distinct status states:
                      </p>
                      <div class="grid grid-cols-1 sm:grid-cols-2 gap-3 pt-1">
                        <div class="p-3.5 rounded-xl bg-slate-50 border border-slate-200">
                          <span class="px-2.5 py-1 rounded-lg text-xs font-bold bg-amber-600 text-white">Want to Watch</span>
                          <p class="text-xs text-slate-600 mt-2">Wishlist for titles you plan to watch later. Shows upcoming premiere badges on your calendar.</p>
                        </div>
                        <div class="p-3.5 rounded-xl bg-slate-50 border border-slate-200">
                          <span class="px-2.5 py-1 rounded-lg text-xs font-bold bg-blue-600 text-white">Currently Watching</span>
                          <p class="text-xs text-slate-600 mt-2">Active shows and anime. Prominently displays your next episode to watch with 1-click increment.</p>
                        </div>
                        <div class="p-3.5 rounded-xl bg-slate-50 border border-slate-200">
                          <span class="px-2.5 py-1 rounded-lg text-xs font-bold bg-emerald-600 text-white">Finished (Completed)</span>
                          <p class="text-xs text-slate-600 mt-2">Completed titles. Counts towards your total time spent and genre distribution analytics.</p>
                        </div>
                        <div class="p-3.5 rounded-xl bg-slate-50 border border-slate-200">
                          <span class="px-2.5 py-1 rounded-lg text-xs font-bold bg-rose-600 text-white">Stopped (Dropped)</span>
                          <p class="text-xs text-slate-600 mt-2">Titles you discontinued. Keeps your history intact without cluttering currently watching queues.</p>
                        </div>
                      </div>
                    </div>

                    <div class="p-6 rounded-2xl bg-white border border-slate-200 shadow-sm space-y-3">
                      <h3 class="text-lg font-bold text-slate-900 flex items-center gap-2">
                        <i class="fa-solid fa-layer-group text-purple-600"></i> 3. Tracking Seasons &amp; Episodes with Accordions
                      </h3>
                      <p class="text-slate-600 leading-relaxed">
                        On any TV show or anime page, seasons and episodes are organized into an interactive accordion structure:
                      </p>
                      <ul class="list-disc pl-5 space-y-2 text-slate-600">
                        <li><strong>Mark Entire Series / Anime Watched:</strong> Use the green button in the section header to mark all seasons and episodes finished at once.</li>
                        <li><strong>Mark Season Watched:</strong> Each season accordion header features a direct <em>Mark Season Watched</em> button. Clicking it updates progress to the end of that season without needing to check episodes one by one.</li>
                        <li><strong>Mark Single Episode:</strong> Expand any season accordion to reveal episode thumbnails, official episode names, air dates, and individual <em>Mark Watched</em> buttons.</li>
                        <li><strong>Next Episode Guide:</strong> When a show is marked as <em>Currently Watching</em>, the hero banner automatically highlights your next episode (e.g. <code>Next: S02E05</code>).</li>
                      </ul>
                    </div>

                    <div class="p-6 rounded-2xl bg-white border border-slate-200 shadow-sm space-y-3">
                      <h3 class="text-lg font-bold text-slate-900 flex items-center gap-2">
                        <i class="fa-solid fa-star text-amber-500"></i> 4. Ratings, Reviews &amp; Watched Dates
                      </h3>
                      <p class="text-slate-600 leading-relaxed">
                        Click the <em>Rate &amp; Date</em> button on any title card to open the assessment modal:
                      </p>
                      <ul class="list-disc pl-5 space-y-1.5 text-slate-600">
                        <li><strong>Score:</strong> Rate from 1 to 10 stars with visual highlighting.</li>
                        <li><strong>Review:</strong> Write personal notes or critical reviews that display prominently on your title card.</li>
                        <li><strong>Watched Date Presets:</strong> Quickly pick dates using presets like <em>Today</em>, <em>1 Year Ago</em>, or <em>3 Years Ago</em>.</li>
                      </ul>
                    </div>

                    <div class="p-6 rounded-2xl bg-white border border-slate-200 shadow-sm space-y-3">
                      <h3 class="text-lg font-bold text-slate-900 flex items-center gap-2">
                        <i class="fa-solid fa-tv text-sky-600"></i> 5. Streaming Providers &amp; Followed Creators
                      </h3>
                      <ul class="list-disc pl-5 space-y-1.5 text-slate-600">
                        <li><strong>Where to Watch:</strong> Real-time streaming availability cards indicate where a title can be streamed (Netflix, Apple TV+, Prime Video, Disney+) or rented.</li>
                        <li><strong>Follow People:</strong> Click <em>Follow</em> on any actor or director at <a href="/Media/People" class="text-purple-600 font-semibold hover:underline">/Media/People</a>. The button acts as an instant toggle.</li>
                        <li><strong>Filmography Tracker:</strong> Open a creator profile to view how many titles in their entire filmography you have watched (e.g. 8 of 22 watched).</li>
                      </ul>
                    </div>

                    <div class="p-6 rounded-2xl bg-white border border-slate-200 shadow-sm space-y-3">
                      <h3 class="text-lg font-bold text-slate-900 flex items-center gap-2">
                        <i class="fa-solid fa-chart-pie text-emerald-600"></i> 6. Calendar, Playlists &amp; Analytics
                      </h3>
                      <ul class="list-disc pl-5 space-y-1.5 text-slate-600">
                        <li><strong>Release Calendar:</strong> View scheduled movie premieres and TV season returns at <a href="/Media/Calendar" class="text-purple-600 font-semibold hover:underline">/Media/Calendar</a>.</li>
                        <li><strong>Custom Playlists:</strong> Create custom collections at <a href="/Media/Lists" class="text-purple-600 font-semibold hover:underline">/Media/Lists</a> (e.g. "Weekend Anime", "Sci-Fi Favorites").</li>
                        <li><strong>Watch Statistics:</strong> Inspect total hours spent, anime vs TV vs movie breakdowns, and rating distribution at <a href="/Media/Stats" class="text-purple-600 font-semibold hover:underline">/Media/Stats</a>.</li>
                        <li><strong>Backup &amp; Restore:</strong> Export your full watch history as a JSON backup or import from a file at any time.</li>
                      </ul>
                    </div>
                  </div>
                </div>
                """
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "User Guide: Food, Nutrition & Meal Planning System",
                Slug = "how-to-use-food-and-nutrition",
                Status = PageStatus.Published,
                CreatedAt = new DateTime(2026, 10, 21, 10, 0, 0, DateTimeKind.Utc),
                Content = """
                <div class="py-6 px-4 max-w-4xl mx-auto font-sans leading-relaxed text-slate-800">
                  <div class="border-b border-slate-200 pb-6 mb-8">
                    <div class="flex items-center gap-2 mb-3">
                      <span class="px-3 py-1 bg-emerald-50 text-emerald-700 font-bold text-xs rounded-full border border-emerald-200">System Guide</span>
                      <span class="text-xs text-slate-500"><i class="fa-regular fa-clock mr-1"></i> 7 min read</span>
                    </div>
                    <h1 class="text-3xl sm:text-4xl font-black text-slate-900 tracking-tight">How to Use the Food &amp; Nutrition System</h1>
                    <p class="text-base text-slate-600 mt-2">A complete guide to managing recipes, scheduling weekly meals, tracking daily calories and macronutrients, automatic pantry inventory decrements, and real-time shopping lists.</p>
                  </div>

                  <div class="grid grid-cols-1 md:grid-cols-3 gap-4 mb-10">
                    <div class="p-4 rounded-2xl bg-slate-50 border border-slate-200">
                      <div class="text-xs font-bold text-slate-500 uppercase">Recipes &amp; Macros</div>
                      <div class="text-base font-bold text-slate-900 mt-1"><i class="fa-solid fa-utensils text-emerald-600 mr-2"></i>Recipe Management</div>
                      <p class="text-2xs text-slate-500 mt-1">Full ingredient breakdown with automated macronutrient calculation.</p>
                    </div>
                    <div class="p-4 rounded-2xl bg-slate-50 border border-slate-200">
                      <div class="text-xs font-bold text-slate-500 uppercase">Pantry Ledger</div>
                      <div class="text-base font-bold text-slate-900 mt-1"><i class="fa-solid fa-boxes-stacked text-brand-600 mr-2"></i>Auto-Decrement</div>
                      <p class="text-2xs text-slate-500 mt-1">Ingredients auto-deduct from stock when meals are logged.</p>
                    </div>
                    <div class="p-4 rounded-2xl bg-slate-50 border border-slate-200">
                      <div class="text-xs font-bold text-slate-500 uppercase">Collaboration</div>
                      <div class="text-base font-bold text-slate-900 mt-1"><i class="fa-solid fa-cart-shopping text-sky-600 mr-2"></i>Real-Time List</div>
                      <p class="text-2xs text-slate-500 mt-1">SignalR synchronized shopping cart for family members.</p>
                    </div>
                  </div>

                  <div class="space-y-8 text-sm">
                    <div class="p-6 rounded-2xl bg-white border border-slate-200 shadow-sm space-y-3">
                      <h3 class="text-lg font-bold text-slate-900 flex items-center gap-2">
                        <i class="fa-solid fa-book-bookmark text-emerald-600"></i> 1. Managing Recipes &amp; Ingredients
                      </h3>
                      <p class="text-slate-600 leading-relaxed">
                        Navigate to <a href="/Food" class="text-emerald-600 font-semibold hover:underline">/Food</a> to create and browse your culinary recipe collection.
                      </p>
                      <ul class="list-disc pl-5 space-y-1.5 text-slate-600">
                        <li><strong>Ingredient Quantities:</strong> Define each ingredient with its unit (grams, ml, pieces) and exact measurement per portion.</li>
                        <li><strong>Macronutrient Profiles:</strong> Specify Calories, Protein (g), Carbohydrates (g), and Fat (g) per serving. The system automatically tallies total nutrition.</li>
                        <li><strong>Tags &amp; Timers:</strong> Add preparation time, cooking time, and dietary tags (e.g. High-Protein, Low-Carb, Vegan, Quick Prep).</li>
                      </ul>
                    </div>

                    <div class="p-6 rounded-2xl bg-white border border-slate-200 shadow-sm space-y-3">
                      <h3 class="text-lg font-bold text-slate-900 flex items-center gap-2">
                        <i class="fa-solid fa-calendar-week text-brand-600"></i> 2. Weekly Meal Planning
                      </h3>
                      <p class="text-slate-600 leading-relaxed">
                        Use the interactive meal planner to organize your weekly eating schedule:
                      </p>
                      <ul class="list-disc pl-5 space-y-1.5 text-slate-600">
                        <li><strong>Meal Slots:</strong> Plan meals across four daily slots: Breakfast, Lunch, Dinner, and Snacks for all 7 days of the week.</li>
                        <li><strong>Macro Targets:</strong> As you assign recipes to days, a top summary bar computes your scheduled daily calories and macronutrient breakdown compared against your daily nutrition goals.</li>
                        <li><strong>Drag &amp; Reorder:</strong> Easily swap meals between days or clear slots as plans change.</li>
                      </ul>
                    </div>

                    <div class="p-6 rounded-2xl bg-white border border-slate-200 shadow-sm space-y-3">
                      <h3 class="text-lg font-bold text-slate-900 flex items-center gap-2">
                        <i class="fa-solid fa-clipboard-check text-purple-600"></i> 3. Meal Logging &amp; Real-Time Macro Intake
                      </h3>
                      <p class="text-slate-600 leading-relaxed">
                        Log what you actually consumed during the day:
                      </p>
                      <ul class="list-disc pl-5 space-y-1.5 text-slate-600">
                        <li><strong>Quick Logging:</strong> Choose from planned meals or pick any recipe from your collection.</li>
                        <li><strong>Portion Scaling:</strong> Adjust the portion multiplier (e.g. 1.5 servings) to accurately capture intake.</li>
                        <li><strong>Daily Progress Rings:</strong> Track consumed calories and macros against your personalized daily targets in real time.</li>
                      </ul>
                    </div>

                    <div class="p-6 rounded-2xl bg-white border border-slate-200 shadow-sm space-y-3">
                      <h3 class="text-lg font-bold text-slate-900 flex items-center gap-2">
                        <i class="fa-solid fa-boxes-stacked text-amber-600"></i> 4. Automatic Pantry Stock Decrement
                      </h3>
                      <p class="text-slate-600 leading-relaxed">
                        The system bridges your food logs with your kitchen inventory through an automated stock reduction ledger:
                      </p>
                      <div class="p-4 rounded-xl bg-slate-50 border border-slate-200 space-y-2">
                        <p class="text-xs text-slate-700"><strong>How it works:</strong></p>
                        <ol class="list-decimal pl-5 text-xs text-slate-600 space-y-1">
                          <li>When you log a meal that was prepared at home, the system looks up all ingredients in the recipe.</li>
                          <li>For each ingredient linked to your inventory, a stock decrement ledger movement is recorded automatically.</li>
                          <li>Your available pantry quantities update instantly without manual deduction.</li>
                        </ol>
                      </div>
                    </div>

                    <div class="p-6 rounded-2xl bg-white border border-slate-200 shadow-sm space-y-3">
                      <h3 class="text-lg font-bold text-slate-900 flex items-center gap-2">
                        <i class="fa-solid fa-warehouse text-blue-600"></i> 5. Pantry &amp; Inventory Management
                      </h3>
                      <ul class="list-disc pl-5 space-y-1.5 text-slate-600">
                        <li><strong>Storage Zones:</strong> Categorize items by location (Refrigerator, Freezer, Dry Pantry, Spice Rack).</li>
                        <li><strong>Minimum Stock Alerts:</strong> Set low-stock threshold alerts so you are notified before essential staples run out.</li>
                        <li><strong>Expiration Tracking:</strong> Monitor expiration dates to minimize food waste and optimize recipe scheduling.</li>
                      </ul>
                    </div>

                    <div class="p-6 rounded-2xl bg-white border border-slate-200 shadow-sm space-y-3">
                      <h3 class="text-lg font-bold text-slate-900 flex items-center gap-2">
                        <i class="fa-solid fa-cart-shopping text-emerald-600"></i> 6. Smart Shopping List &amp; SignalR Real-Time Collaboration
                      </h3>
                      <ul class="list-disc pl-5 space-y-1.5 text-slate-600">
                        <li><strong>Auto-Generate from Meal Plan:</strong> Generate your grocery list in 1 click. The system compares meal plan requirements against current pantry inventory and only adds missing items.</li>
                        <li><strong>Real-Time SignalR Synchronization:</strong> When shopping at the supermarket, any item checked off or added broadcasts instantly to all family or household members without refreshing the page.</li>
                        <li><strong>Aisle Organization:</strong> Items are automatically grouped into supermarket aisles (Produce, Dairy, Meat &amp; Seafood, Bakery, Pantry).</li>
                      </ul>
                    </div>
                  </div>
                </div>
                """
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "User Guide: Gym Courses, Workouts & Fitness Analytics",
                Slug = "how-to-use-gym-and-workouts",
                Status = PageStatus.Published,
                CreatedAt = new DateTime(2026, 10, 21, 10, 0, 0, DateTimeKind.Utc),
                Content = """
                <div class="py-6 px-4 max-w-4xl mx-auto font-sans leading-relaxed text-slate-800">
                  <div class="border-b border-slate-200 pb-6 mb-8">
                    <div class="flex items-center gap-2 mb-3">
                      <span class="px-3 py-1 bg-blue-50 text-blue-700 font-bold text-xs rounded-full border border-blue-200">System Guide</span>
                      <span class="text-xs text-slate-500"><i class="fa-regular fa-clock mr-1"></i> 8 min read</span>
                    </div>
                    <h1 class="text-3xl sm:text-4xl font-black text-slate-900 tracking-tight">How to Use Gym Courses, Training &amp; Workout Analytics</h1>
                    <p class="text-base text-slate-600 mt-2">A complete guide to enrolling in gym courses, logging active workouts with sets and rest timers, reviewing personal records (PRs), tracking weight and body composition, and mobile PWA usage.</p>
                  </div>

                  <div class="grid grid-cols-1 md:grid-cols-3 gap-4 mb-10">
                    <div class="p-4 rounded-2xl bg-slate-50 border border-slate-200">
                      <div class="text-xs font-bold text-slate-500 uppercase">Courses &amp; Classes</div>
                      <div class="text-base font-bold text-slate-900 mt-1"><i class="fa-solid fa-graduation-cap text-blue-600 mr-2"></i>Gym Courses</div>
                      <p class="text-2xs text-slate-500 mt-1">Structured courses with schedules, coaches, and enrollments.</p>
                    </div>
                    <div class="p-4 rounded-2xl bg-slate-50 border border-slate-200">
                      <div class="text-xs font-bold text-slate-500 uppercase">Workout Logging</div>
                      <div class="text-base font-bold text-slate-900 mt-1"><i class="fa-solid fa-dumbbell text-purple-600 mr-2"></i>Active Workouts</div>
                      <p class="text-2xs text-slate-500 mt-1">Track sets, reps, weight kg, RPE, rest timers, and PRs.</p>
                    </div>
                    <div class="p-4 rounded-2xl bg-slate-50 border border-slate-200">
                      <div class="text-xs font-bold text-slate-500 uppercase">Body Composition</div>
                      <div class="text-base font-bold text-slate-900 mt-1"><i class="fa-solid fa-weight-scale text-emerald-600 mr-2"></i>Weight &amp; BMI</div>
                      <p class="text-2xs text-slate-500 mt-1">Track muscle %, fat %, bone %, and interactive BMI simulator.</p>
                    </div>
                  </div>

                  <div class="space-y-8 text-sm">
                    <div class="p-6 rounded-2xl bg-white border border-slate-200 shadow-sm space-y-3">
                      <h3 class="text-lg font-bold text-slate-900 flex items-center gap-2">
                        <i class="fa-solid fa-graduation-cap text-blue-600"></i> 1. Gym Courses &amp; Enrollments
                      </h3>
                      <p class="text-slate-600 leading-relaxed">
                        Navigate to <a href="/GymCourses" class="text-blue-600 font-semibold hover:underline">/GymCourses</a> to browse structured fitness classes:
                      </p>
                      <ul class="list-disc pl-5 space-y-1.5 text-slate-600">
                        <li><strong>Course Catalog:</strong> Browse classes across disciplines including HIIT, Powerlifting, Functional Mobility, Yoga, and Core Strength.</li>
                        <li><strong>Class Details:</strong> Review instructor profiles, difficulty level (Beginner, Intermediate, Advanced), maximum participant capacity, and required equipment.</li>
                        <li><strong>Enrollment:</strong> Enroll in active cohorts and manage upcoming scheduled class sessions.</li>
                      </ul>
                    </div>

                    <div class="p-6 rounded-2xl bg-white border border-slate-200 shadow-sm space-y-3">
                      <h3 class="text-lg font-bold text-slate-900 flex items-center gap-2">
                        <i class="fa-solid fa-person-running text-brand-600"></i> 2. Exercises &amp; Training Library
                      </h3>
                      <p class="text-slate-600 leading-relaxed">
                        Access the exercise movement catalog at <a href="/GymCourses/Trainings" class="text-blue-600 font-semibold hover:underline">/GymCourses/Trainings</a>:
                      </p>
                      <ul class="list-disc pl-5 space-y-1.5 text-slate-600">
                        <li><strong>Muscle Groups:</strong> Filter movements by target muscle anatomy: Chest, Back, Legs, Shoulders, Biceps, Triceps, and Core.</li>
                        <li><strong>Training Categories:</strong> Exercises are grouped into Strength, Hypertrophy, Endurance, Cardio, and Mobility.</li>
                        <li><strong>Calorie Formulas:</strong> Each exercise defines a base calorie burn rate and required gym equipment (Barbell, Dumbbells, Cables, Bodyweight).</li>
                      </ul>
                    </div>

                    <div class="p-6 rounded-2xl bg-white border border-slate-200 shadow-sm space-y-3">
                      <h3 class="text-lg font-bold text-slate-900 flex items-center gap-2">
                        <i class="fa-solid fa-stopwatch text-purple-600"></i> 3. Active Workout Logging &amp; Rest Timers
                      </h3>
                      <p class="text-slate-600 leading-relaxed">
                        Start a workout session at <a href="/Workout" class="text-blue-600 font-semibold hover:underline">/Workout</a>:
                      </p>
                      <ul class="list-disc pl-5 space-y-1.5 text-slate-600">
                        <li><strong>Set Logging:</strong> For each exercise, log completed Sets, Reps, and Resistance Weight in kg.</li>
                        <li><strong>RPE Scale:</strong> Record Rate of Perceived Exertion (1 to 10) to gauge fatigue and training intensity.</li>
                        <li><strong>Rest Timers:</strong> Built-in 60s, 90s, and 120s countdown timers help maintain optimal pacing between sets.</li>
                        <li><strong>Personal Records:</strong> The system automatically detects when you exceed previous bests and celebrates new PRs.</li>
                      </ul>
                    </div>

                    <div class="p-6 rounded-2xl bg-white border border-slate-200 shadow-sm space-y-3">
                      <h3 class="text-lg font-bold text-slate-900 flex items-center gap-2">
                        <i class="fa-solid fa-clock-rotate-left text-amber-600"></i> 4. Workout History &amp; Volume Progression
                      </h3>
                      <p class="text-slate-600 leading-relaxed">
                        Review past training logs at <a href="/Workout/History" class="text-blue-600 font-semibold hover:underline">/Workout/History</a>:
                      </p>
                      <ul class="list-disc pl-5 space-y-1.5 text-slate-600">
                        <li><strong>Volume Summary:</strong> Tracks total tonnage lifted per workout (Sets &times; Reps &times; Weight).</li>
                        <li><strong>Exercise Timelines:</strong> View progression over time for key compound lifts (Bench Press, Squat, Deadlift, Overhead Press).</li>
                        <li><strong>Calorie Burn Estimation:</strong> Summary of estimated calories burned across workout duration.</li>
                      </ul>
                    </div>

                    <div class="p-6 rounded-2xl bg-white border border-slate-200 shadow-sm space-y-3">
                      <h3 class="text-lg font-bold text-slate-900 flex items-center gap-2">
                        <i class="fa-solid fa-chart-line text-emerald-600"></i> 5. Weight Tracking, BMI &amp; Body Composition
                      </h3>
                      <p class="text-slate-600 leading-relaxed">
                        Track body transformation at <a href="/Workout/Analytics" class="text-blue-600 font-semibold hover:underline">/Workout/Analytics</a>:
                      </p>
                      <ul class="list-disc pl-5 space-y-1.5 text-slate-600">
                        <li><strong>Mobile-Responsive Weight Entry:</strong> Tap <em>Log Weight</em> to open the mobile-optimized modal. Record weight in kg, height in cm, date, and optional notes.</li>
                        <li><strong>Automatic BMI Calculation:</strong> Computes Body Mass Index instantly and categorizes into WHO standards (Underweight, Normal, Overweight, Obese).</li>
                        <li><strong>Body Composition:</strong> Track Muscle %, Body Fat %, and Bone % from smart scales or DEXA scans.</li>
                        <li><strong>Interactive BMI Simulator:</strong> Use the live height and weight slider simulator to calculate what weight is required to hit target BMI ranges.</li>
                      </ul>
                    </div>

                    <div class="p-6 rounded-2xl bg-white border border-slate-200 shadow-sm space-y-3">
                      <h3 class="text-lg font-bold text-slate-900 flex items-center gap-2">
                        <i class="fa-solid fa-mobile-screen-button text-sky-600"></i> 6. Mobile PWA Installation &amp; Gym Floor Tracking
                      </h3>
                      <p class="text-slate-600 leading-relaxed">
                        TrackerPro is a full Progressive Web App (PWA) designed for quick logging directly on the gym floor:
                      </p>
                      <ul class="list-disc pl-5 space-y-1.5 text-slate-600">
                        <li><strong>iPhone / iPad (iOS Safari):</strong> Tap the <em>Share</em> button in Safari &rarr; select <em>Add to Home Screen</em> &rarr; tap <em>Add</em>.</li>
                        <li><strong>Android (Chrome):</strong> Tap <em>Install Mobile App</em> in the left sidebar or select <em>Install app</em> from Chrome's top right menu.</li>
                        <li><strong>Standalone Mode:</strong> Runs full screen without browser toolbars for a distraction-free workout logging experience.</li>
                      </ul>
                    </div>
                  </div>
                </div>
                """
            }
        };
    }
}
