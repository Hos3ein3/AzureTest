

using System;
using System.Threading;
using System.Threading.Tasks;

namespace AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Domain.Documents.Pages;

public interface IPageRepository : IDocumentRepository<PageDocument> { }

public interface INoSqlUnitOfWork : IDisposable
{
    IResumeFilesRepository ResumeFiles { get; }
    ICompanyRepository Companies { get; }
    IResumeStatusLogRepository ResumeStatusLogs { get; }
    IDebtPersonRepository DebtPersons { get; }
    IExpenseCategoryRepository ExpenseCategories { get; }
    IUserPreferenceRepository UserPreferences { get; }
    IDashboardComponentRepository DashboardComponents { get; }
    IDashboardRawComponentRepository DashboardRawComponents { get; }
    IUserDashboardRepository UserDashboards { get; }
    IJobSearchHistoryRepository JobSearchHistories { get; }
    IIdeaRepository Ideas { get; }
    IIdeaPageRepository IdeaPages { get; }
    IPageRepository Pages { get; }
    IIdeaSectionRepository IdeaSections { get; }

    IHouseSearchHistoryRepository HouseSearchHistories { get; }
    IHouseCrawlerCredentialRepository HouseCrawlerCredentials { get; }

    ITrainingRepository Trainings { get; }
    ITrainingCourseRepository TrainingCourses { get; }
    IUserGymCourseRepository UserGymCourses { get; }
    IUserTrainingSessionRepository UserTrainingSessions { get; }
    IUserWeightLogRepository UserWeightLogs { get; }

    IIngredientRepository Ingredients { get; }
    IRecipeRepository Recipes { get; }
    IMealLogRepository MealLogs { get; }
    IDailyNutritionTargetRepository NutritionTargets { get; }

    IIdentitySettingRepository IdentitySettings { get; }
    Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default);
}
