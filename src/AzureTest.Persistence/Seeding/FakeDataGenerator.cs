using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
/// Generates reproducible, realistic fake data for all SQL entities and NoSQL documents
/// using the Bogus library. Uses entity factory methods to respect domain invariants.
/// </summary>
public static class FakeDataGenerator
{
    // Fixed seed = same data every run → deterministic test scenarios.
    private const int Seed = 8675309;

    public static async Task SeedAsync(
        AzureTestDbContext sqlContext,
        INoSqlUnitOfWork noSql,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager)
    {
        Randomizer.Seed = new Random(Seed);

        // ── 1. Ensure "Admin" role exists ────────────────────────────────────
        if (!await roleManager.RoleExistsAsync("Admin"))
            await roleManager.CreateAsync(new ApplicationRole { Name = "Admin" });

        if (!await roleManager.RoleExistsAsync("User"))
            await roleManager.CreateAsync(new ApplicationRole { Name = "User" });

        // ── 2. Ensure seed admin user exists ─────────────────────────────────
        const string adminEmail = "admin@simulate.com";
        const string adminPassword = "Admin123!";

        var admin = await userManager.FindByEmailAsync(adminEmail);
        if (admin == null)
        {
            admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FirstName = "Simulate",
                LastName = "Admin"
            };
            await userManager.CreateAsync(admin, adminPassword);
            await userManager.AddToRoleAsync(admin, "Admin");
        }

        // Seed a few regular users as well
        var regularUsers = new List<ApplicationUser>();
        var userFaker = new Faker<ApplicationUser>()
            .RuleFor(u => u.FirstName, f => f.Name.FirstName())
            .RuleFor(u => u.LastName, f => f.Name.LastName())
            .RuleFor(u => u.Email, (f, u) => f.Internet.Email(u.FirstName, u.LastName))
            .RuleFor(u => u.UserName, (f, u) => u.Email)
            .RuleFor(u => u.EmailConfirmed, _ => true);

        foreach (var fakeUser in userFaker.Generate(5))
        {
            if (await userManager.FindByEmailAsync(fakeUser.Email!) == null)
            {
                await userManager.CreateAsync(fakeUser, "User123!");
                await userManager.AddToRoleAsync(fakeUser, "User");
                regularUsers.Add(fakeUser);
            }
        }

        // All user IDs we can assign data to
        var allUserIds = regularUsers.Select(u => u.Id).Append(admin.Id).ToList();
        var adminId = admin.Id;

        // ── 3. NoSQL Documents ────────────────────────────────────────────────

        // Companies (global, no UserId)
        var companyFaker = new Faker<Company>()
            .RuleFor(c => c.CompanyName, f => f.Company.CompanyName())
            .RuleFor(c => c.KvkNumber, f => f.Random.ReplaceNumbers("########"))
            .RuleFor(c => c.IsINDMember, f => f.Random.Bool(0.3f))
            .RuleFor(c => c.LinkedinURL, f => $"https://linkedin.com/company/{f.Internet.DomainWord()}")
            .RuleFor(c => c.IndeedUrl, f => $"https://indeed.com/cmp/{f.Internet.DomainWord()}")
            .RuleFor(c => c.Location, f => f.Address.City());

        var companies = companyFaker.Generate(20);
        foreach (var c in companies) await noSql.Companies.AddAsync(c);

        // DebtPersons (per user)
        var debtPersonFaker = new Faker<DebtPerson>()
            .RuleFor(p => p.FullName, f => f.Name.FullName());

        var allDebtPersons = new List<DebtPerson>();
        foreach (var uid in allUserIds)
        {
            var persons = debtPersonFaker.Generate(3);
            foreach (var p in persons) { p.UserId = uid; }
            allDebtPersons.AddRange(persons);
            foreach (var p in persons) await noSql.DebtPersons.AddAsync(p);
        }

        // ExpenseCategories (per user)
        var categoryNames = new[] { "Food", "Transport", "Housing", "Entertainment", "Healthcare", "Education", "Clothing", "Utilities", "Savings", "Other" };
        var allCategories = new List<ExpenseCategory>();
        foreach (var uid in allUserIds)
        {
            foreach (var name in categoryNames.Take(new Faker().Random.Int(4, 8)))
            {
                var cat = new ExpenseCategory { UserId = uid, Name = name };
                allCategories.Add(cat);
                await noSql.ExpenseCategories.AddAsync(cat);
            }
        }

        // UserPreferences (one per user)
        var languages = new[] { "en", "nl", "de", "fr", "es" };
        var themes = new[] { "blue", "green", "purple", "orange", "slate" };
        var prefFaker = new Faker();
        foreach (var uid in allUserIds)
        {
            await noSql.UserPreferences.AddAsync(new UserPreference
            {
                UserId = uid,
                Language = prefFaker.PickRandom(languages),
                ThemeColor = prefFaker.PickRandom(themes)
            });
        }

        await noSql.SaveChangesAsync();

        // ── 4. SQL Entities ───────────────────────────────────────────────────

        // Resumes (use Create factory — all private setters)
        var resumeFaker = new Faker();
        var resumes = new List<Resume>();
        foreach (var uid in allUserIds)
        {
            for (int i = 0; i < 10; i++)
            {
                var company = resumeFaker.PickRandom(companies);
                var start = resumeFaker.Date.Past(1);
                var resume = Resume.Create(
                    positionTitle: resumeFaker.Name.JobTitle(),
                    description: resumeFaker.Lorem.Paragraphs(2),
                    location: resumeFaker.Address.City(),
                    jobLink: resumeFaker.Internet.Url(),
                    note: resumeFaker.Lorem.Sentence(),
                    chatUrl: resumeFaker.Internet.Url(),
                    companyId: company.Id,
                    userId: uid,
                    source: resumeFaker.PickRandom<ResumeSource>(),
                    status: resumeFaker.PickRandom<ResumeStatus>()
                );
                resumes.Add(resume);
            }
        }
        await sqlContext.Resumes.AddRangeAsync(resumes);
        await sqlContext.SaveChangesAsync();

        // ResumeStatusLogs (NoSQL, after resumes are saved)
        var statusLogFaker = new Faker();
        foreach (var resume in resumes.Take(30))
        {
            for (int i = 0; i < statusLogFaker.Random.Int(1, 3); i++)
            {
                await noSql.ResumeStatusLogs.AddAsync(new ResumeStatusLog
                {
                    ResumeId = resume.Id,
                    Date = statusLogFaker.Date.Recent(60),
                    OldStatus = statusLogFaker.PickRandom<ResumeStatus>(),
                    NewStatus = statusLogFaker.PickRandom<ResumeStatus>(),
                    Note = statusLogFaker.Lorem.Sentence()
                });
            }
        }
        await noSql.SaveChangesAsync();

        // Tickets (use Create factory)
        var ticketFaker = new Faker();
        var tickets = new List<Ticket>();
        foreach (var uid in allUserIds)
        {
            for (int i = 0; i < 6; i++)
            {
                tickets.Add(Ticket.Create(
                    subject: ticketFaker.Lorem.Sentence(4),
                    content: ticketFaker.Lorem.Paragraphs(1),
                    priority: ticketFaker.PickRandom<TicketPriority>(),
                    userId: uid
                ));
            }
        }
        await sqlContext.Tickets.AddRangeAsync(tickets);
        await sqlContext.SaveChangesAsync();

        // TicketMessages (use Create factory, linked to saved tickets)
        var msgFaker = new Faker();
        var ticketMessages = new List<TicketMessage>();
        foreach (var ticket in tickets)
        {
            for (int i = 0; i < msgFaker.Random.Int(1, 4); i++)
            {
                ticketMessages.Add(TicketMessage.Create(
                    ticketId: ticket.Id,
                    userId: ticket.UserId,
                    content: msgFaker.Lorem.Sentences(2)
                ));
            }
        }
        await sqlContext.TicketMessages.AddRangeAsync(ticketMessages);

        // TaskItems (use Create factory)
        var taskFaker = new Faker();
        var taskItems = new List<TaskItem>();
        foreach (var uid in allUserIds)
        {
            for (int i = 0; i < 15; i++)
            {
                var start = taskFaker.Date.Soon(14);
                taskItems.Add(TaskItem.Create(
                    title: taskFaker.Lorem.Sentence(3),
                    description: taskFaker.Lorem.Sentence(),
                    startDate: start,
                    endDate: start.AddDays(taskFaker.Random.Int(1, 5)),
                    priority: taskFaker.PickRandom("Low", "Medium", "High", "Critical"),
                    userId: uid
                ));
            }
        }
        await sqlContext.Tasks.AddRangeAsync(taskItems);

        // Events (use Create factory)
        var eventFaker = new Faker();
        var events = new List<Event>();
        var colors = new[] { "#3b82f6", "#10b981", "#f59e0b", "#ef4444", "#8b5cf6", "#ec4899" };
        foreach (var uid in allUserIds)
        {
            for (int i = 0; i < 10; i++)
            {
                var start = eventFaker.Date.Soon(30);
                events.Add(Event.Create(
                    title: eventFaker.Lorem.Word(),
                    description: eventFaker.Lorem.Sentence(),
                    startDate: start,
                    endDate: start.AddHours(eventFaker.Random.Int(1, 8)),
                    isAllDay: eventFaker.Random.Bool(0.2f),
                    color: eventFaker.PickRandom(colors),
                    userId: uid
                ));
            }
        }
        await sqlContext.Events.AddRangeAsync(events);

        // HabitTrackers + HabitLogs (use Create factory)
        var habitFaker = new Faker();
        var habitTrackers = new List<HabitTracker>();
        foreach (var uid in allUserIds)
        {
            for (int i = 0; i < 4; i++)
            {
                var habitType = habitFaker.PickRandom<HabitType>();
                habitTrackers.Add(HabitTracker.Create(
                    name: habitFaker.Lorem.Word(),
                    description: habitFaker.Lorem.Sentence(),
                    type: habitType,
                    frequency: habitFaker.PickRandom<HabitFrequency>(),
                    goal: habitType == HabitType.Boolean ? 1 : habitFaker.Random.Double(1, 100),
                    userId: uid
                ));
            }
        }
        await sqlContext.HabitTrackers.AddRangeAsync(habitTrackers);
        await sqlContext.SaveChangesAsync();

        var habitLogs = new List<HabitLog>();
        var logFaker = new Faker();
        foreach (var tracker in habitTrackers)
        {
            for (int i = 0; i < 14; i++)
            {
                habitLogs.Add(HabitLog.Create(
                    habitTrackerId: tracker.Id,
                    logDate: logFaker.Date.Recent(14),
                    booleanValue: logFaker.Random.Bool(),
                    numericValue: logFaker.Random.Double(0, 100),
                    notes: logFaker.Random.Bool(0.3f) ? logFaker.Lorem.Sentence() : null
                ));
            }
        }
        await sqlContext.HabitLogs.AddRangeAsync(habitLogs);

        // Expenses (use Create factory — private setters)
        var expFaker = new Faker();
        var expenses = new List<Expense>();
        foreach (var uid in allUserIds)
        {
            var userCategories = allCategories.Where(c => c.UserId == uid).ToList();
            for (int i = 0; i < 40; i++)
            {
                var cat = userCategories.Any() ? (Guid?)expFaker.PickRandom(userCategories).Id : null;
                expenses.Add(Expense.Create(
                    userId: uid,
                    amount: expFaker.Finance.Amount(5, 500),
                    date: expFaker.Date.Recent(90),
                    description: expFaker.Commerce.ProductName(),
                    categoryId: cat
                ));
            }
        }
        await sqlContext.Expenses.AddRangeAsync(expenses);

        // Debts (public setters — object initializer)
        var debtFaker = new Faker();
        var debts = new List<Debt>();
        foreach (var uid in allUserIds)
        {
            var userPersons = allDebtPersons.Where(p => p.UserId == uid).ToList();
            foreach (var person in userPersons)
            {
                var borrowDate = debtFaker.Date.Past(1);
                debts.Add(new Debt
                {
                    UserId = uid,
                    DebtPersonId = person.Id,
                    Amount = debtFaker.Finance.Amount(50, 2000),
                    DateOfBorrow = borrowDate,
                    DateOfReturn = debtFaker.Random.Bool(0.4f) ? borrowDate.AddMonths(debtFaker.Random.Int(1, 6)) : null,
                    PromissedReturnDate = borrowDate.AddMonths(debtFaker.Random.Int(1, 12)),
                    Profit = debtFaker.Finance.Amount(0, 100),
                    IsDone = debtFaker.Random.Bool(0.3f)
                });
            }
        }
        await sqlContext.Debts.AddRangeAsync(debts);

        // Final save for all remaining entities
        await sqlContext.SaveChangesAsync();
    }
}
