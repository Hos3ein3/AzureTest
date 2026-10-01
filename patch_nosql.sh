sed -i '' '/private IUserDashboardRepository _userDashboards;/a\
    private IJobSearchHistoryRepository _jobSearchHistories;\
' /Users/mohammadhosein/Development/AzureTest/src/AzureTest.Persistence/Repositories/NoSql/NoSqlUnitOfWork.cs

sed -i '' '/public IUserDashboardRepository UserDashboards/a\
    public IJobSearchHistoryRepository JobSearchHistories => _jobSearchHistories ??= new JobSearchHistoryRepository(_database.Client, Microsoft.Extensions.Options.Options.Create(new AzureTest.Infrastructure.Configuration.MongoDbSettings { DatabaseName = _database.DatabaseNamespace.DatabaseName }));\
' /Users/mohammadhosein/Development/AzureTest/src/AzureTest.Persistence/Repositories/NoSql/NoSqlUnitOfWork.cs
