

using AzureTest.Application.Companies;
using AzureTest.Application.Debts;
using AzureTest.Application.Debts.Interfaces;
using AzureTest.Application.Expenses.Units;
using AzureTest.Application.Media;
using AzureTest.Application.Resumes;
using AzureTest.Application.SampleNotes;
using AzureTest.Application.Users;
using AzureTest.Contracts.Validators.Resumes;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace AzureTest.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScrutorConfig();

        // Register all FluentValidation validators from the Contracts assembly
        services.AddValidatorsFromAssemblyContaining<CreateResumeDtoValidator>();
        // Register FluentValidation validators from the Application assembly (Commands/Queries)
        services.AddValidatorsFromAssembly(System.Reflection.Assembly.GetExecutingAssembly());

        // Register Resume unit aggregators
        services.AddScoped<IResumeCommandUnit, ResumeCommandUnit>();
        services.AddScoped<IResumeQueryUnit, ResumeQueryUnit>();
        services.AddScoped<ICompanyQueryUnit, CompanyQueryUnit>();
        services.AddScoped<ICompanyCommandUnit, CompanyCommandUnit>();
        services.AddScoped<IUserCommandUnit, UserCommandUnit>();
        services.AddScoped<IUserQueryUnit, UserQueryUnit>();
        services.AddScoped<IResumeValidationUnit, ResumeValidationUnit>();
        services.AddScoped<AzureTest.Application.Interfaces.Services.IResumeDocumentService, AzureTest.Application.Services.ResumeDocumentService>();
        services.AddScoped<IResumeFileQueryUnit, ResumeFileQueryUnit>();

        // New Feature Domains
        services.AddScoped<AzureTest.Application.Events.IEventCommandUnit, AzureTest.Application.Events.EventCommandUnit>();
        services.AddScoped<AzureTest.Application.Events.IEventQueryUnit, AzureTest.Application.Events.EventQueryUnit>();
        services.AddScoped<AzureTest.Application.Tasks.ITaskCommandUnit, AzureTest.Application.Tasks.TaskCommandUnit>();
        services.AddScoped<AzureTest.Application.Tasks.ITaskQueryUnit, AzureTest.Application.Tasks.TaskQueryUnit>();
        services.AddScoped<AzureTest.Application.Habits.IHabitCommandUnit, AzureTest.Application.Habits.HabitCommandUnit>();
        services.AddScoped<AzureTest.Application.Habits.IHabitQueryUnit, AzureTest.Application.Habits.HabitQueryUnit>();
        services.AddScoped<AzureTest.Application.Tickets.ITicketCommandUnit, AzureTest.Application.Tickets.TicketCommandUnit>();
        services.AddScoped<AzureTest.Application.Tickets.ITicketQueryUnit, AzureTest.Application.Tickets.TicketQueryUnit>();
        
        services.AddScoped<IDebtCommandUnit, DebtCommandUnit>();
        services.AddScoped<IDebtQueryUnit, DebtQueryUnit>();
        services.AddScoped<IExpenseQueryUnit, ExpenseQueryUnit>();
        services.AddScoped<IExpenseCommandUnit, ExpenseCommandUnit>();
        services.AddScoped<AzureTest.Application.Permissions.IPermissionQueryUnit, AzureTest.Application.Permissions.PermissionQueryUnit>();
        services.AddScoped<AzureTest.Application.Permissions.IPermissionCommandUnit, AzureTest.Application.Permissions.PermissionCommandUnit>();

        // SampleNote dev playground
        services.AddScoped<ISampleNoteCommandUnit, SampleNoteCommandUnit>();
        services.AddScoped<ISampleNoteQueryUnit, SampleNoteQueryUnit>();

        // Job Crawler
        services.AddScoped<AzureTest.Application.JobCrawler.IJobCrawlerCommandUnit, AzureTest.Application.JobCrawler.JobCrawlerCommandUnit>();
        services.AddScoped<AzureTest.Application.JobCrawler.IJobCrawlerQueryUnit, AzureTest.Application.JobCrawler.JobCrawlerQueryUnit>();

        // House Crawler
        services.AddScoped<AzureTest.Application.HouseCrawler.Interfaces.IHouseCrawlerCommandUnit, AzureTest.Application.HouseCrawler.HouseCrawlerCommandUnit>();
        services.AddScoped<AzureTest.Application.HouseCrawler.Interfaces.IHouseCrawlerQueryUnit, AzureTest.Application.HouseCrawler.HouseCrawlerQueryUnit>();

        // Idea Tracker
        services.AddScoped<AzureTest.Application.IdeaTracker.IIdeaCommandUnit, AzureTest.Application.IdeaTracker.IdeaCommandUnit>();
        services.AddScoped<AzureTest.Application.IdeaTracker.IIdeaQueryUnit, AzureTest.Application.IdeaTracker.IdeaQueryUnit>();
        services.AddScoped<AzureTest.Application.IdeaTracker.IIdeaPageCommandUnit, AzureTest.Application.IdeaTracker.IdeaPageCommandUnit>();
        services.AddScoped<AzureTest.Application.IdeaTracker.IIdeaPageQueryUnit, AzureTest.Application.IdeaTracker.IdeaPageQueryUnit>();
        services.AddScoped<AzureTest.Application.IdeaTracker.IIdeaSectionCommandUnit, AzureTest.Application.IdeaTracker.IdeaSectionCommandUnit>();
        services.AddScoped<AzureTest.Application.IdeaTracker.IIdeaSectionQueryUnit, AzureTest.Application.IdeaTracker.IdeaSectionQueryUnit>();



        // Pomodoro
        services.AddScoped<AzureTest.Application.Pomodoro.IPomodoroCommandUnit, AzureTest.Application.Pomodoro.PomodoroCommandUnit>();
        services.AddScoped<AzureTest.Application.Pomodoro.IPomodoroQueryUnit, AzureTest.Application.Pomodoro.PomodoroQueryUnit>();

        // Football Fantasy
        services.AddScoped<AzureTest.Application.FootballFantasy.IFootballFantasyQueryUnit, AzureTest.Application.FootballFantasy.FootballFantasyQueryUnit>();
        services.AddScoped<AzureTest.Application.FootballFantasy.IFootballFantasyCommandUnit, AzureTest.Application.FootballFantasy.FootballFantasyCommandUnit>();

        // Food & Cooking
        services.AddScoped<AzureTest.Application.Food.IFoodQueryUnit, AzureTest.Application.Food.FoodQueryUnit>();
        services.AddScoped<AzureTest.Application.Food.IFoodCommandUnit, AzureTest.Application.Food.FoodCommandUnit>();

        // Media (TMDb Movies, Series, Anime, Actors)
        services.AddMediaModule();

        return services;
    }
}
