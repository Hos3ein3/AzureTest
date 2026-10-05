

namespace AzureTest.Application.Configuration;

/// <summary>
/// Configuration settings for local simulation environment,
/// controlling database drop behaviors on shutdown and data seeding toggles.
/// </summary>
public class SimulateSetting
{
    public const string SectionName = "SimulateSetting";

    // ── Database cleanup settings on application shutdown ────────────────────
    public bool DropDatabasesOnShutdown { get; set; } = true;
    public bool DropSqlDatabaseOnShutdown
    {
        get => DropDatabasesOnShutdown;
        set => DropDatabasesOnShutdown = value;
    }
    public bool DropNoSqlOnShutdown { get; set; } = true;
    public bool DropNoSQlOnShutdown
    {
        get => DropNoSqlOnShutdown;
        set => DropNoSqlOnShutdown = value;
    }
    public bool DropRedisCacheOnShutdown { get; set; } = true;

    // ── Seeding settings: Identity / Users ───────────────────────────────────
    public bool AddSeedUser { get; set; } = true;
    public bool AddSeedUsers
    {
        get => AddSeedUser;
        set => AddSeedUser = value;
    }

    // ── Seeding settings: NoSQL Documents ────────────────────────────────────
    public bool AddSeedCompany { get; set; } = true;
    public bool AddSeedCompanies
    {
        get => AddSeedCompany;
        set => AddSeedCompany = value;
    }

    public bool AddSeedDebtPerson { get; set; } = true;
    public bool AddSeedDebtPersons
    {
        get => AddSeedDebtPerson;
        set => AddSeedDebtPerson = value;
    }

    public bool AddSeedExpenseCategory { get; set; } = true;
    public bool AddSeedExpenseCategories
    {
        get => AddSeedExpenseCategory;
        set => AddSeedExpenseCategory = value;
    }

    public bool AddSeedUserPreference { get; set; } = true;
    public bool AddSeedUserPreferences
    {
        get => AddSeedUserPreference;
        set => AddSeedUserPreference = value;
    }

    public bool AddSeedResumeStatusLog { get; set; } = true;
    public bool AddSeedResumeStatusLogs
    {
        get => AddSeedResumeStatusLog;
        set => AddSeedResumeStatusLog = value;
    }
    public bool AddSeedStatusLog
    {
        get => AddSeedResumeStatusLog;
        set => AddSeedResumeStatusLog = value;
    }

    // ── Seeding settings: SQL Entities ───────────────────────────────────────
    public bool AddSeedResume { get; set; } = true;
    public bool AddSeedResumes
    {
        get => AddSeedResume;
        set => AddSeedResume = value;
    }

    public bool AddSeedExpense { get; set; } = true;
    public bool AddSeedExpenses
    {
        get => AddSeedExpense;
        set => AddSeedExpense = value;
    }

    public bool AddSeedDebt { get; set; } = true;
    public bool AddSeedDebts
    {
        get => AddSeedDebt;
        set => AddSeedDebt = value;
    }

    public bool AddSeedTaskItem { get; set; } = true;
    public bool AddSeedTaskItems
    {
        get => AddSeedTaskItem;
        set => AddSeedTaskItem = value;
    }
    public bool AddSeedTask
    {
        get => AddSeedTaskItem;
        set => AddSeedTaskItem = value;
    }
    public bool AddSeedTasks
    {
        get => AddSeedTaskItem;
        set => AddSeedTaskItem = value;
    }

    public bool AddSeedEvent { get; set; } = true;
    public bool AddSeedEvents
    {
        get => AddSeedEvent;
        set => AddSeedEvent = value;
    }

    public bool AddSeedHabitTracker { get; set; } = true;
    public bool AddSeedHabitTrackers
    {
        get => AddSeedHabitTracker;
        set => AddSeedHabitTracker = value;
    }
    public bool AddSeedHabit
    {
        get => AddSeedHabitTracker;
        set => AddSeedHabitTracker = value;
    }
    public bool AddSeedHabits
    {
        get => AddSeedHabitTracker;
        set => AddSeedHabitTracker = value;
    }

    public bool AddSeedHabitLog { get; set; } = true;
    public bool AddSeedHabitLogs
    {
        get => AddSeedHabitLog;
        set => AddSeedHabitLog = value;
    }

    public bool AddSeedTicket { get; set; } = true;
    public bool AddSeedTickets
    {
        get => AddSeedTicket;
        set => AddSeedTicket = value;
    }

    public bool AddSeedTicketMessage { get; set; } = true;
    public bool AddSeedTicketMessages
    {
        get => AddSeedTicketMessage;
        set => AddSeedTicketMessage = value;
    }

    public bool AddSeedIdea { get; set; } = true;
    public bool AddSeedIdeas
    {
        get => AddSeedIdea;
        set => AddSeedIdea = value;
    }
}
