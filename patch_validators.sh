sed -i '' '/services.AddScoped<FluentValidation.IValidator<UpdateSampleNoteDto>, UpdateSampleNoteDtoValidator>();/a\
        services.AddScoped<FluentValidation.IValidator<AzureTest.Contracts.DTOs.IdeaTracker.CreateIdeaDto>, AzureTest.Contracts.Validators.IdeaTracker.CreateIdeaDtoValidator>();\
        services.AddScoped<FluentValidation.IValidator<AzureTest.Contracts.DTOs.IdeaTracker.UpdateIdeaDto>, AzureTest.Contracts.Validators.IdeaTracker.UpdateIdeaDtoValidator>();\
' /Users/mohammadhosein/Development/AzureTest/src/AzureTest.Infrastructure/DependencyInjection.cs
