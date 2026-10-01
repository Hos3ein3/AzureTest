sed -i '' 's/ICommandHandler<CreateUserDashboardCommand> createDashboardHandler,/ICommandHandler<CreateUserDashboardCommand> createDashboardHandler,\n        FluentValidation.IValidator<CreateUserDashboardCommand> createValidator,\n        FluentValidation.IValidator<UpdateUserDashboardCommand> updateValidator,/g' /Users/mohammadhosein/Development/AzureTest/src/AzureTest.MVC/Controllers/DashboardController.cs

sed -i '' '/private readonly ICommandHandler<CreateUserDashboardCommand> _createDashboardHandler;/a\
    private readonly FluentValidation.IValidator<CreateUserDashboardCommand> _createValidator;\
    private readonly FluentValidation.IValidator<UpdateUserDashboardCommand> _updateValidator;
' /Users/mohammadhosein/Development/AzureTest/src/AzureTest.MVC/Controllers/DashboardController.cs

sed -i '' '/_createDashboardHandler = createDashboardHandler;/a\
        _createValidator = createValidator;\
        _updateValidator = updateValidator;
' /Users/mohammadhosein/Development/AzureTest/src/AzureTest.MVC/Controllers/DashboardController.cs

