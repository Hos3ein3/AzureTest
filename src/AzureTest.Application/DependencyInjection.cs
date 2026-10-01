

using AzureTest.Application.Companies;
using AzureTest.Application.Debts;
using AzureTest.Application.Debts.Interfaces;
using AzureTest.Application.Expenses.Units;
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

        // SampleNote dev playground
        services.AddScoped<ISampleNoteCommandUnit, SampleNoteCommandUnit>();
        services.AddScoped<ISampleNoteQueryUnit, SampleNoteQueryUnit>();

        return services;
    }
}
