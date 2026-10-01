# Fix Command
sed -i '' 's/InsertAsync/AddAsync/g' /Users/mohammadhosein/Development/AzureTest/src/AzureTest.Application/JobCrawler/Commands/CreateJobSearchHistoryCommandHandler.cs
sed -i '' 's/return history.Id;/return history.Id.ToString();/g' /Users/mohammadhosein/Development/AzureTest/src/AzureTest.Application/JobCrawler/Commands/CreateJobSearchHistoryCommandHandler.cs

# Fix Query
sed -i '' 's/x => x.UserId == query.Filter.UserId, cancellationToken/x => x.UserId == query.Filter.UserId/g' /Users/mohammadhosein/Development/AzureTest/src/AzureTest.Application/JobCrawler/Queries/GetUserJobSearchHistoryQueryHandler.cs
sed -i '' 's/doc.Id,/doc.Id.ToString(),/g' /Users/mohammadhosein/Development/AzureTest/src/AzureTest.Application/JobCrawler/Queries/GetUserJobSearchHistoryQueryHandler.cs

