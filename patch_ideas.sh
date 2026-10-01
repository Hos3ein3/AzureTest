# Update INoSqlUnitOfWork
sed -i '' '/IJobSearchHistoryRepository JobSearchHistories/a\
    IIdeaRepository Ideas { get; }\
' /Users/mohammadhosein/Development/AzureTest/src/AzureTest.Application/Interfaces/Data/NoSql/INoSqlUnitOfWork.cs

# Update NoSqlUnitOfWork
sed -i '' '/private IJobSearchHistoryRepository? _jobSearchHistories;/a\
    private IIdeaRepository? _ideas;\
' /Users/mohammadhosein/Development/AzureTest/src/AzureTest.Persistence/Repositories/NoSql/NoSqlUnitOfWork.cs

sed -i '' '/public IJobSearchHistoryRepository JobSearchHistories/a\
    public IIdeaRepository Ideas => _ideas ??= new IdeaRepository(_database);\
' /Users/mohammadhosein/Development/AzureTest/src/AzureTest.Persistence/Repositories/NoSql/NoSqlUnitOfWork.cs

