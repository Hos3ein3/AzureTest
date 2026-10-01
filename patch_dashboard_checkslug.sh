sed -i '' 's/ICommandHandler<UpdateUserPreferenceCommand> updatePreferenceHandler,/ICommandHandler<UpdateUserPreferenceCommand> updatePreferenceHandler,\n        Microsoft.Extensions.Options.IOptions<AzureTest.Infrastructure.Configuration.DashboardSettings> dashboardSettings,/g' /Users/mohammadhosein/Development/AzureTest/src/AzureTest.MVC/Controllers/DashboardController.cs

sed -i '' '/private readonly ICommandHandler<UpdateUserPreferenceCommand> _updatePreferenceHandler;/a\
    private readonly Microsoft.Extensions.Options.IOptions<AzureTest.Infrastructure.Configuration.DashboardSettings> _dashboardSettings;
' /Users/mohammadhosein/Development/AzureTest/src/AzureTest.MVC/Controllers/DashboardController.cs

sed -i '' '/_updatePreferenceHandler = updatePreferenceHandler;/a\
        _dashboardSettings = dashboardSettings;
' /Users/mohammadhosein/Development/AzureTest/src/AzureTest.MVC/Controllers/DashboardController.cs

