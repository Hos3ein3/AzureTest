

using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data.NoSql;
using MongoDB.Driver;

namespace AzureTest.Persistence.Repositories.NoSql;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Domain.Documents.Pages;

public class PageRepository : DocumentRepository<PageDocument>, IPageRepository { public PageRepository(IMongoDatabase db) : base(db, "Pages") { } }

public class NoSqlUnitOfWork : INoSqlUnitOfWork
{
    private readonly IMongoClient _mongoClient;
    private readonly IMongoDatabase _database;
    private readonly AzureTest.Application.Interfaces.Services.ITenantProvider? _tenantProvider;
    private IIdentitySettingRepository? _identitySettings;
    private IClientSessionHandle? _session;

    private IResumeFilesRepository? _resumeFiles;
    private ICompanyRepository? _companies;
    private IResumeStatusLogRepository? _resumeStatusLogs;
    private IDebtPersonRepository? _debtPersons;
    private IExpenseCategoryRepository? _expenseCategories;
    private IUserPreferenceRepository? _userPreferences;
    private IDashboardComponentRepository? _dashboardComponents;
    private IDashboardRawComponentRepository? _dashboardRawComponents;
    private IUserDashboardRepository? _userDashboards;
    private IJobSearchHistoryRepository? _jobSearchHistories;
    private IIdeaRepository? _ideas;
    private IIdeaPageRepository? _ideaPages;
    private IIdeaSectionRepository? _ideaSections;
    private IHouseSearchHistoryRepository? _houseSearchHistories;
    private IHouseCrawlerCredentialRepository? _houseCrawlerCredentials;



    public NoSqlUnitOfWork(IMongoClient mongoClient, IMongoDatabase database, AzureTest.Application.Interfaces.Services.ITenantProvider? tenantProvider = null)
    {
        _mongoClient = mongoClient ?? throw new ArgumentNullException(nameof(mongoClient));
        _database = database ?? throw new ArgumentNullException(nameof(database));
        _tenantProvider = tenantProvider;
    }

    public IResumeFilesRepository ResumeFiles => _resumeFiles ??= new ResumeFilesRepository(_database);
    public ICompanyRepository Companies => _companies ??= new CompanyRepository(_database);
    public IResumeStatusLogRepository ResumeStatusLogs => _resumeStatusLogs ??= new ResumeStatusLogRepository(_database);
    public IDebtPersonRepository DebtPersons => _debtPersons ??= new DebtPersonRepository(_database);
    public IExpenseCategoryRepository ExpenseCategories => _expenseCategories ??= new ExpenseCategoryRepository(_database);
    public IUserPreferenceRepository UserPreferences => _userPreferences ??= new UserPreferenceRepository(_database);
    public IDashboardComponentRepository DashboardComponents => _dashboardComponents ??= new DashboardComponentRepository(_database);
    public IDashboardRawComponentRepository DashboardRawComponents => _dashboardRawComponents ??= new DashboardRawComponentRepository(_database);
    public IUserDashboardRepository UserDashboards => _userDashboards ??= new UserDashboardRepository(_database);
    public IJobSearchHistoryRepository JobSearchHistories => _jobSearchHistories ??= new JobSearchHistoryRepository(_database);
    public IIdeaRepository Ideas => _ideas ??= new IdeaRepository(_database);
    public IIdeaPageRepository IdeaPages => _ideaPages ??= new IdeaPageRepository(_database);
    private IPageRepository? _pages;
    public IPageRepository Pages => _pages ??= new PageRepository(_database);

    public IIdeaSectionRepository IdeaSections => _ideaSections ??= new IdeaSectionRepository(_database);
    private ITrainingRepository? _trainings;
    private ITrainingCourseRepository? _trainingCourses;
    private IUserGymCourseRepository? _userGymCourses;
    private IUserTrainingSessionRepository? _userTrainingSessions;
    private IUserWeightLogRepository? _userWeightLogs;

    public ITrainingRepository Trainings => _trainings ??= new TrainingRepository(_database);
    public ITrainingCourseRepository TrainingCourses => _trainingCourses ??= new TrainingCourseRepository(_database);
    public IUserGymCourseRepository UserGymCourses => _userGymCourses ??= new UserGymCourseRepository(_database);
    public IUserTrainingSessionRepository UserTrainingSessions => _userTrainingSessions ??= new UserTrainingSessionRepository(_database);
    public IUserWeightLogRepository UserWeightLogs => _userWeightLogs ??= new UserWeightLogRepository(_database);

    private IIngredientRepository? _ingredients;
    private IRecipeRepository? _recipes;
    private IMealLogRepository? _mealLogs;
    private IDailyNutritionTargetRepository? _nutritionTargets;
    private IPantryStockItemRepository? _pantryStockItems;
    private IStockMovementRepository? _stockMovements;
    private IShoppingListItemRepository? _shoppingListItems;

    public IIngredientRepository Ingredients => _ingredients ??= new IngredientRepository(_database);
    public IRecipeRepository Recipes => _recipes ??= new RecipeRepository(_database);
    public IMealLogRepository MealLogs => _mealLogs ??= new MealLogRepository(_database);
    public IDailyNutritionTargetRepository NutritionTargets => _nutritionTargets ??= new DailyNutritionTargetRepository(_database);
    public IPantryStockItemRepository PantryStockItems => _pantryStockItems ??= new PantryStockItemRepository(_database, _tenantProvider);
    public IStockMovementRepository StockMovements => _stockMovements ??= new StockMovementRepository(_database);
    public IShoppingListItemRepository ShoppingListItems => _shoppingListItems ??= new ShoppingListItemRepository(_database, _tenantProvider);

    public IHouseSearchHistoryRepository HouseSearchHistories => _houseSearchHistories ??= new HouseSearchHistoryRepository(_database);
    public IHouseCrawlerCredentialRepository HouseCrawlerCredentials => _houseCrawlerCredentials ??= new HouseCrawlerCredentialRepository(_database);



    public async Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // MongoDB doesn't have a direct "SaveChanges" like EF Core unless using explicit transactions.
        // If a transaction session is active, commit it.
        if (_session != null && _session.IsInTransaction)
        {
            await _session.CommitTransactionAsync(cancellationToken);
            return true;
        }

        // If no explicit transaction, modifications are already saved incrementally.
        return true;
    }

    public IIdentitySettingRepository IdentitySettings => _identitySettings ??= new IdentitySettingRepository(_database);

    public void Dispose()
    {
        _session?.Dispose();
        GC.SuppressFinalize(this);
    }
}
