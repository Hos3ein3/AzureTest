sed -i '' '/services.AddScoped<ICurrentUser, CurrentUser>();/a\
        services.AddScoped<AzureTest.Application.Interfaces.Services.IValidationService, AzureTest.Infrastructure.Services.ValidationService>();
' /Users/mohammadhosein/Development/AzureTest/src/AzureTest.Infrastructure/DependencyInjection.cs
