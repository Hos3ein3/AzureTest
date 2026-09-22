using AzureTest.Application.Companies;
using AzureTest.Application.Resumes;
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

        // Register Resume unit aggregators
        services.AddScoped<IResumeCommandUnit, ResumeCommandUnit>();
        services.AddScoped<IResumeQueryUnit, ResumeQueryUnit>();
        services.AddScoped<ICompanyQueryUnit, CompanyQueryUnit>();
        services.AddScoped<ICompanyCommandUnit, CompanyCommandUnit>();
        services.AddScoped<IResumeValidationUnit, ResumeValidationUnit>();
        services.AddScoped<AzureTest.Application.Interfaces.Services.IResumeDocumentService, AzureTest.Application.Services.ResumeDocumentService>();
        services.AddScoped<IResumeFileQueryUnit, ResumeFileQueryUnit>();

        return services;
    }
}
