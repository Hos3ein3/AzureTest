using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Common;
using AzureTest.Contracts.DTOs.Food;
using AzureTest.Domain.Documents.Food;
using AzureTest.Domain.Documents.Food.Enums;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.Food;

// ======================== QUERIES ========================

public record GetAllIngredientsQuery(IngredientFilter Filter) : IQuery<Result<PagedResult<IngredientDto, IngredientFilter>>>;
public record GetIngredientByIdQuery(Guid Id) : IQuery<Result<IngredientDto>>;
public record GetAllRecipesQuery(RecipeFilter Filter) : IQuery<Result<PagedResult<RecipeDto, RecipeFilter>>>;
public record GetRecipeByIdQuery(Guid Id) : IQuery<Result<RecipeDto>>;
public record GetDailyNutritionSummaryQuery(Guid UserId, DateTime Date) : IQuery<Result<DailyNutritionSummaryDto>>;
public record GetNutritionTargetQuery(Guid UserId) : IQuery<Result<NutritionTargetDto>>;
public record GetAthleteProfileQuery(Guid UserId) : IQuery<Result<AthleteProfileDto>>;
public record GetChefScalingQuery(Guid RecipeId, double TargetServings) : IQuery<Result<ChefScalingResultDto>>;
public record GetPantryMatchRecipesQuery(Guid UserId) : IQuery<Result<List<RecipeDto>>>;

// Stock & Shopping Queries
public record GetStockInventoryQuery(StockFilter Filter) : IQuery<Result<PagedResult<PantryStockItemDto, StockFilter>>>;
public record GetStockOverviewQuery(Guid UserId) : IQuery<Result<StockInventoryOverviewDto>>;
public record GetStockMovementsQuery(StockMovementFilter Filter) : IQuery<Result<PagedResult<StockMovementDto, StockMovementFilter>>>;
public record GetStockItemByIdQuery(Guid Id, Guid UserId) : IQuery<Result<PantryStockItemDto>>;
public record GetShoppingListQuery(ShoppingListFilter Filter) : IQuery<Result<ShoppingListGroupedDto>>;
public record GetMissingRecipeIngredientsQuery(Guid UserId, Guid RecipeId, double Servings) : IQuery<Result<List<GroceryShoppingItemDto>>>;

// ======================== MAPPERS ========================

public static class FoodMappers
{
    public static IngredientDto MapToDto(IngredientDocument doc) => new(
        doc.Id,
        doc.Name,
        doc.Category,
        doc.Description,
        doc.Calories,
        doc.ProteinGrams,
        doc.CarbsGrams,
        doc.FatGrams,
        doc.FiberGrams,
        doc.SugarGrams,
        doc.SodiumMg,
        doc.DefaultServingSizeGrams,
        doc.DefaultUnit,
        doc.DensityGramsPerMl,
        doc.GramsPerPiece,
        doc.GramsPerTbsp,
        doc.GramsPerCup,
        doc.AllowedUnits ?? new(),
        doc.DietaryTags ?? new(),
        doc.AllergenInfo,
        doc.ApproximateCostPer100g,
        doc.CreatedByUserId,
        doc.CreatedAt
    );

    public static RecipeIngredientDto MapToDto(RecipeIngredientItem item) => new(
        item.IngredientId,
        item.IngredientName,
        item.Quantity,
        item.Unit,
        item.Notes,
        item.SubstitutionNote,
        item.ComputedGrams,
        item.Calories,
        item.Protein,
        item.Carbs,
        item.Fat,
        item.Fiber,
        item.Sugar,
        item.Sodium
    );

    public static RecipeStepDto MapToDto(RecipeStepItem item) => new(
        item.StepNumber,
        item.Instruction,
        item.TimerMinutes,
        item.ChefTip
    );

    public static RecipeDto MapToDto(RecipeDocument doc) => new(
        doc.Id,
        doc.UserId,
        doc.Name,
        doc.Description,
        doc.Category,
        doc.Servings,
        doc.PrepTimeMinutes,
        doc.CookTimeMinutes,
        doc.TotalTimeMinutes,
        doc.Difficulty,
        doc.Tags ?? new(),
        doc.DietaryTags ?? new(),
        doc.Ingredients?.Select(MapToDto).ToList() ?? new(),
        doc.Steps?.Select(MapToDto).ToList() ?? new(),
        doc.CookingYieldFactor,
        doc.RawWeightGrams,
        doc.CookedWeightGrams,
        doc.TotalCalories,
        doc.TotalProtein,
        doc.TotalCarbs,
        doc.TotalFat,
        doc.TotalFiber,
        doc.TotalSugar,
        doc.TotalSodium,
        doc.PerServingCalories,
        doc.PerServingProtein,
        doc.PerServingCarbs,
        doc.PerServingFat,
        doc.PerServingFiber,
        doc.PerServingCookedWeightGrams,
        doc.Per100gCookedCalories,
        doc.Per100gCookedProtein,
        doc.Per100gCookedCarbs,
        doc.Per100gCookedFat,
        doc.IsFavorite,
        doc.ImageUrl,
        doc.EstimatedCost,
        doc.CreatedAt,
        doc.UpdatedAt
    );

    public static MealLogItemDto MapToDto(MealLogItem item) => new(
        item.ItemId,
        item.IsRecipe,
        item.ReferenceId,
        item.Name,
        item.Quantity,
        item.Unit,
        item.EquivalentGramsOrServings,
        item.Calories,
        item.Protein,
        item.Carbs,
        item.Fat,
        item.Fiber,
        item.Sugar,
        item.Sodium
    );

    public static MealLogDto MapToDto(MealLogDocument doc) => new(
        doc.Id,
        doc.UserId,
        doc.Date,
        doc.MealType,
        doc.Items?.Select(MapToDto).ToList() ?? new(),
        doc.TotalCalories,
        doc.TotalProtein,
        doc.TotalCarbs,
        doc.TotalFat,
        doc.TotalFiber,
        doc.TotalSugar,
        doc.TotalSodium,
        doc.WaterMl,
        doc.Notes,
        doc.LoggedAt
    );

    public static NutritionTargetDto MapToDto(DailyNutritionTargetDocument doc) => new(
        doc.Id,
        doc.UserId,
        doc.CalorieTarget,
        doc.ProteinGramsTarget,
        doc.CarbsGramsTarget,
        doc.FatGramsTarget,
        doc.WaterMlTarget,
        doc.GoalType,
        doc.TargetProteinPerKg,
        doc.EnableCarbCycling,
        doc.TrainingDayCalories,
        doc.TrainingDayCarbs,
        doc.RestDayCalories,
        doc.RestDayCarbs,
        doc.Pantry?.Select(p => new PantryItemDto(p.Id, p.IngredientId, p.Name, p.Quantity, p.Unit, p.ExpiryDate, p.Location)).ToList() ?? new(),
        doc.ShoppingList?.Select(s => new ShoppingListItemDto(s.Id, s.IngredientId, s.Name, s.Quantity, s.Unit, s.IsPurchased, s.Category, s.Notes)).ToList() ?? new(),
        doc.WeeklyPlan?.Select(w => new WeeklyMealPlanSlotDto(w.Id, w.DayOfWeek, w.MealType, w.RecipeId, w.RecipeName, w.Servings, w.Notes)).ToList() ?? new(),
        doc.UpdatedAt
    );

    public static PantryStockItemDto MapToDto(PantryStockItemDocument doc)
    {
        var now = DateTime.UtcNow;
        int? daysUntil = doc.ExpiryDate.HasValue ? (int)Math.Ceiling((doc.ExpiryDate.Value.Date - now.Date).TotalDays) : null;
        return new PantryStockItemDto(
            doc.Id,
            doc.UserId,
            doc.IngredientId,
            doc.IngredientName,
            doc.Location,
            doc.CurrentQuantity,
            doc.Unit,
            doc.CurrentNormalizedGrams,
            doc.ExpiryDate,
            doc.BatchCode,
            doc.PurchaseDate,
            doc.UnitPrice,
            doc.MinimumParLevel,
            doc.IsStaple,
            doc.StapleStatus,
            doc.IsDepleted,
            doc.NeedsRestockReconciliation,
            doc.IsExpired(now),
            doc.IsExpiringSoon(3, now),
            doc.IsBelowParLevel(),
            daysUntil,
            doc.CreatedAt,
            doc.UpdatedAt
        );
    }

    public static StockMovementDto MapToDto(StockMovementDocument doc) => new(
        doc.Id,
        doc.UserId,
        doc.StockItemId,
        doc.IngredientId,
        doc.IngredientName,
        doc.MovementType,
        doc.QuantityDelta,
        doc.Unit,
        doc.NormalizedGramsDelta,
        doc.Timestamp,
        doc.CorrelationId,
        doc.WasteReason,
        doc.Cost,
        doc.Location,
        doc.Notes
    );

    public static GroceryShoppingItemDto MapToDto(ShoppingListItemDocument doc) => new(
        doc.Id,
        doc.UserId,
        doc.IngredientId,
        doc.Name,
        doc.Quantity,
        doc.Unit,
        doc.NormalizedGrams,
        doc.Status,
        doc.StoreSection,
        doc.Source,
        doc.TargetLocation,
        doc.EstimatedCost,
        doc.Notes,
        doc.CreatedAt,
        doc.IsShared,
        doc.CreatedByUserName,
        doc.LastModifiedByUserName
    );
}

// ======================== HANDLERS ========================

public class GetAllIngredientsQueryHandler : BaseQueryHandler<GetAllIngredientsQuery, Result<PagedResult<IngredientDto, IngredientFilter>>>
{
    private readonly INoSqlUnitOfWork _uow;
    public GetAllIngredientsQueryHandler(INoSqlUnitOfWork uow, ILogger<GetAllIngredientsQueryHandler> logger) : base(logger) => _uow = uow;

    protected override async Task<Result<PagedResult<IngredientDto, IngredientFilter>>> ExecuteAsync(GetAllIngredientsQuery request, CancellationToken ct)
    {
        var filter = request.Filter ?? new IngredientFilter();

        Expression<Func<IngredientDocument, bool>>? predicate = x =>
            (filter.Category == null || x.Category == filter.Category.Value) &&
            (filter.DietaryTag == null || (x.DietaryTags != null && x.DietaryTags.Contains(filter.DietaryTag.Value))) &&
            (filter.UserId == null || x.CreatedByUserId == null || x.CreatedByUserId == filter.UserId.Value) &&
            (string.IsNullOrWhiteSpace(filter.SearchTerm) || (x.Name != null && x.Name.Contains(filter.SearchTerm)));

        var paged = await _uow.Ingredients.GetPagedAsync(filter, predicate, ct);
        var dtos = paged.Items.Select(FoodMappers.MapToDto).ToList();

        return Result.Success(new PagedResult<IngredientDto, IngredientFilter>(dtos, paged.TotalCount, paged.PageNumber, paged.PageSize, filter));
    }
}

public class GetIngredientByIdQueryHandler : BaseQueryHandler<GetIngredientByIdQuery, Result<IngredientDto>>
{
    private readonly INoSqlUnitOfWork _uow;
    public GetIngredientByIdQueryHandler(INoSqlUnitOfWork uow, ILogger<GetIngredientByIdQueryHandler> logger) : base(logger) => _uow = uow;

    protected override async Task<Result<IngredientDto>> ExecuteAsync(GetIngredientByIdQuery request, CancellationToken ct)
    {
        var item = await _uow.Ingredients.GetByIdAsync(request.Id, ct);
        if (item == null) return Result.Failure<IngredientDto>("Ingredient not found.");
        return Result.Success(FoodMappers.MapToDto(item));
    }
}

public class GetAllRecipesQueryHandler : BaseQueryHandler<GetAllRecipesQuery, Result<PagedResult<RecipeDto, RecipeFilter>>>
{
    private readonly INoSqlUnitOfWork _uow;
    public GetAllRecipesQueryHandler(INoSqlUnitOfWork uow, ILogger<GetAllRecipesQueryHandler> logger) : base(logger) => _uow = uow;

    protected override async Task<Result<PagedResult<RecipeDto, RecipeFilter>>> ExecuteAsync(GetAllRecipesQuery request, CancellationToken ct)
    {
        var filter = request.Filter ?? new RecipeFilter();

        Expression<Func<RecipeDocument, bool>>? predicate = x =>
            (filter.UserId == null || x.UserId == filter.UserId.Value) &&
            (string.IsNullOrWhiteSpace(filter.Category) || x.Category == filter.Category) &&
            (filter.Difficulty == null || x.Difficulty == filter.Difficulty.Value) &&
            (filter.FavoritesOnly != true || x.IsFavorite) &&
            (filter.DietaryTag == null || (x.DietaryTags != null && x.DietaryTags.Contains(filter.DietaryTag.Value))) &&
            (string.IsNullOrWhiteSpace(filter.SearchTerm) || (x.Name != null && x.Name.Contains(filter.SearchTerm)));

        var paged = await _uow.Recipes.GetPagedAsync(filter, predicate, ct);
        var dtos = paged.Items.Select(FoodMappers.MapToDto).ToList();

        return Result.Success(new PagedResult<RecipeDto, RecipeFilter>(dtos, paged.TotalCount, paged.PageNumber, paged.PageSize, filter));
    }
}

public class GetRecipeByIdQueryHandler : BaseQueryHandler<GetRecipeByIdQuery, Result<RecipeDto>>
{
    private readonly INoSqlUnitOfWork _uow;
    public GetRecipeByIdQueryHandler(INoSqlUnitOfWork uow, ILogger<GetRecipeByIdQueryHandler> logger) : base(logger) => _uow = uow;

    protected override async Task<Result<RecipeDto>> ExecuteAsync(GetRecipeByIdQuery request, CancellationToken ct)
    {
        var item = await _uow.Recipes.GetByIdAsync(request.Id, ct);
        if (item == null) return Result.Failure<RecipeDto>("Recipe not found.");
        return Result.Success(FoodMappers.MapToDto(item));
    }
}

public class GetDailyNutritionSummaryQueryHandler : BaseQueryHandler<GetDailyNutritionSummaryQuery, Result<DailyNutritionSummaryDto>>
{
    private readonly INoSqlUnitOfWork _uow;
    public GetDailyNutritionSummaryQueryHandler(INoSqlUnitOfWork uow, ILogger<GetDailyNutritionSummaryQueryHandler> logger) : base(logger) => _uow = uow;

    protected override async Task<Result<DailyNutritionSummaryDto>> ExecuteAsync(GetDailyNutritionSummaryQuery request, CancellationToken ct)
    {
        var targetDoc = (await _uow.NutritionTargets.FindAsync(x => x.UserId == request.UserId)).FirstOrDefault();
        if (targetDoc == null)
        {
            targetDoc = DailyNutritionTargetDocument.CreateDefault(request.UserId);
            await _uow.NutritionTargets.AddAsync(targetDoc, ct);
        }

        var startOfDay = request.Date.Date;
        var endOfDay = startOfDay.AddDays(1);

        var mealLogs = await _uow.MealLogs.FindAsync(x => x.UserId == request.UserId && x.Date >= startOfDay && x.Date < endOfDay);
        var mealDtos = mealLogs.OrderBy(m => m.MealType).Select(FoodMappers.MapToDto).ToList();

        double totalCalories = Math.Round(mealLogs.Sum(m => m.TotalCalories), 1);
        double totalProtein = Math.Round(mealLogs.Sum(m => m.TotalProtein), 1);
        double totalCarbs = Math.Round(mealLogs.Sum(m => m.TotalCarbs), 1);
        double totalFat = Math.Round(mealLogs.Sum(m => m.TotalFat), 1);
        double totalFiber = Math.Round(mealLogs.Sum(m => m.TotalFiber), 1);
        double totalSugar = Math.Round(mealLogs.Sum(m => m.TotalSugar), 1);
        double totalSodium = Math.Round(mealLogs.Sum(m => m.TotalSodium), 1);
        int totalWaterMl = mealLogs.Sum(m => m.WaterMl);

        var summary = new DailyNutritionSummaryDto(
            request.Date.Date,
            totalCalories,
            totalProtein,
            totalCarbs,
            totalFat,
            totalFiber,
            totalSugar,
            totalSodium,
            totalWaterMl,
            targetDoc.CalorieTarget,
            targetDoc.ProteinGramsTarget,
            targetDoc.CarbsGramsTarget,
            targetDoc.FatGramsTarget,
            targetDoc.WaterMlTarget,
            mealDtos
        );

        return Result.Success(summary);
    }
}

public class GetNutritionTargetQueryHandler : BaseQueryHandler<GetNutritionTargetQuery, Result<NutritionTargetDto>>
{
    private readonly INoSqlUnitOfWork _uow;
    public GetNutritionTargetQueryHandler(INoSqlUnitOfWork uow, ILogger<GetNutritionTargetQueryHandler> logger) : base(logger) => _uow = uow;

    protected override async Task<Result<NutritionTargetDto>> ExecuteAsync(GetNutritionTargetQuery request, CancellationToken ct)
    {
        var targetDoc = (await _uow.NutritionTargets.FindAsync(x => x.UserId == request.UserId)).FirstOrDefault();
        if (targetDoc == null)
        {
            targetDoc = DailyNutritionTargetDocument.CreateDefault(request.UserId);
            await _uow.NutritionTargets.AddAsync(targetDoc, ct);
        }

        return Result.Success(FoodMappers.MapToDto(targetDoc));
    }
}

public class GetAthleteProfileQueryHandler : BaseQueryHandler<GetAthleteProfileQuery, Result<AthleteProfileDto>>
{
    private readonly INoSqlUnitOfWork _uow;
    public GetAthleteProfileQueryHandler(INoSqlUnitOfWork uow, ILogger<GetAthleteProfileQueryHandler> logger) : base(logger) => _uow = uow;

    protected override async Task<Result<AthleteProfileDto>> ExecuteAsync(GetAthleteProfileQuery request, CancellationToken ct)
    {
        var targetDoc = (await _uow.NutritionTargets.FindAsync(x => x.UserId == request.UserId)).FirstOrDefault();
        if (targetDoc == null)
        {
            targetDoc = DailyNutritionTargetDocument.CreateDefault(request.UserId);
            await _uow.NutritionTargets.AddAsync(targetDoc, ct);
        }

        // Fetch latest weight log
        var weightLogs = await _uow.UserWeightLogs.FindAsync(x => x.UserId == request.UserId);
        var latestWeight = weightLogs.OrderByDescending(w => w.Date).FirstOrDefault();

        double? weightKg = latestWeight?.WeightKg;
        double? heightCm = latestWeight?.HeightCm;
        double? bmi = latestWeight?.Bmi;
        string? bmiCat = latestWeight?.BmiCategory;

        // Actual protein ratio per kg body weight
        double proteinRatio = (weightKg.HasValue && weightKg.Value > 0)
            ? Math.Round(targetDoc.ProteinGramsTarget / weightKg.Value, 2)
            : targetDoc.TargetProteinPerKg;

        // Fetch past 7 days of daily summaries for trend analytics
        var recentDays = new List<DailyNutritionSummaryDto>();
        for (int i = 6; i >= 0; i--)
        {
            var date = DateTime.UtcNow.Date.AddDays(-i);
            var nextDate = date.AddDays(1);
            var dayLogs = await _uow.MealLogs.FindAsync(x => x.UserId == request.UserId && x.Date >= date && x.Date < nextDate);
            var dCalories = Math.Round(dayLogs.Sum(m => m.TotalCalories), 1);
            var dProtein = Math.Round(dayLogs.Sum(m => m.TotalProtein), 1);
            var dCarbs = Math.Round(dayLogs.Sum(m => m.TotalCarbs), 1);
            var dFat = Math.Round(dayLogs.Sum(m => m.TotalFat), 1);
            var dFiber = Math.Round(dayLogs.Sum(m => m.TotalFiber), 1);
            var dSugar = Math.Round(dayLogs.Sum(m => m.TotalSugar), 1);
            var dSodium = Math.Round(dayLogs.Sum(m => m.TotalSodium), 1);
            var dWater = dayLogs.Sum(m => m.WaterMl);

            recentDays.Add(new DailyNutritionSummaryDto(
                date,
                dCalories,
                dProtein,
                dCarbs,
                dFat,
                dFiber,
                dSugar,
                dSodium,
                dWater,
                targetDoc.CalorieTarget,
                targetDoc.ProteinGramsTarget,
                targetDoc.CarbsGramsTarget,
                targetDoc.FatGramsTarget,
                targetDoc.WaterMlTarget,
                dayLogs.Select(FoodMappers.MapToDto).ToList()
            ));
        }

        var profile = new AthleteProfileDto(
            weightKg,
            heightCm,
            bmi,
            bmiCat,
            targetDoc.CalorieTarget,
            targetDoc.ProteinGramsTarget,
            targetDoc.CarbsGramsTarget,
            targetDoc.FatGramsTarget,
            proteinRatio,
            targetDoc.GoalType,
            targetDoc.EnableCarbCycling,
            targetDoc.TrainingDayCalories,
            targetDoc.TrainingDayCarbs,
            targetDoc.RestDayCalories,
            targetDoc.RestDayCarbs,
            recentDays
        );

        return Result.Success(profile);
    }
}

public class GetChefScalingQueryHandler : BaseQueryHandler<GetChefScalingQuery, Result<ChefScalingResultDto>>
{
    private readonly INoSqlUnitOfWork _uow;
    public GetChefScalingQueryHandler(INoSqlUnitOfWork uow, ILogger<GetChefScalingQueryHandler> logger) : base(logger) => _uow = uow;

    protected override async Task<Result<ChefScalingResultDto>> ExecuteAsync(GetChefScalingQuery request, CancellationToken ct)
    {
        var recipe = await _uow.Recipes.GetByIdAsync(request.RecipeId, ct);
        if (recipe == null) return Result.Failure<ChefScalingResultDto>("Recipe not found.");

        double targetServings = request.TargetServings > 0 ? request.TargetServings : recipe.Servings;
        double scaleFactor = recipe.Servings > 0 ? (targetServings / recipe.Servings) : 1.0;

        var scaledIngredients = recipe.Ingredients.Select(i => new RecipeIngredientDto(
            i.IngredientId,
            i.IngredientName,
            Math.Round(i.Quantity * scaleFactor, 2),
            i.Unit,
            i.Notes,
            i.SubstitutionNote,
            Math.Round(i.ComputedGrams * scaleFactor, 1),
            Math.Round(i.Calories * scaleFactor, 1),
            Math.Round(i.Protein * scaleFactor, 1),
            Math.Round(i.Carbs * scaleFactor, 1),
            Math.Round(i.Fat * scaleFactor, 1),
            Math.Round(i.Fiber * scaleFactor, 1),
            Math.Round(i.Sugar * scaleFactor, 1),
            Math.Round(i.Sodium * scaleFactor, 1)
        )).ToList();

        var result = new ChefScalingResultDto(
            recipe.Id,
            recipe.Name,
            recipe.Servings,
            targetServings,
            Math.Round(scaleFactor, 2),
            Math.Round(recipe.RawWeightGrams * scaleFactor, 1),
            Math.Round(recipe.CookedWeightGrams * scaleFactor, 1),
            Math.Round(recipe.TotalCalories * scaleFactor, 1),
            Math.Round(recipe.TotalProtein * scaleFactor, 1),
            Math.Round(recipe.TotalCarbs * scaleFactor, 1),
            Math.Round(recipe.TotalFat * scaleFactor, 1),
            scaledIngredients
        );

        return Result.Success(result);
    }
}

public class GetPantryMatchRecipesQueryHandler : BaseQueryHandler<GetPantryMatchRecipesQuery, Result<List<RecipeDto>>>
{
    private readonly INoSqlUnitOfWork _uow;
    public GetPantryMatchRecipesQueryHandler(INoSqlUnitOfWork uow, ILogger<GetPantryMatchRecipesQueryHandler> logger) : base(logger) => _uow = uow;

    protected override async Task<Result<List<RecipeDto>>> ExecuteAsync(GetPantryMatchRecipesQuery request, CancellationToken ct)
    {
        // First check first-class stock items
        var stockItems = await _uow.PantryStockItems.FindAsync(x => x.UserId == request.UserId && !x.IsDepleted);
        var stockIngredientIds = stockItems.Select(s => s.IngredientId).ToHashSet();
        var stockNames = stockItems.Select(s => s.IngredientName.ToLowerInvariant().Trim()).ToHashSet();

        // Fallback to legacy embedded pantry if empty
        if (!stockIngredientIds.Any())
        {
            var targetDoc = (await _uow.NutritionTargets.FindAsync(x => x.UserId == request.UserId)).FirstOrDefault();
            if (targetDoc?.Pantry != null)
            {
                foreach (var p in targetDoc.Pantry)
                {
                    stockNames.Add(p.Name.ToLowerInvariant().Trim());
                    if (p.IngredientId.HasValue) stockIngredientIds.Add(p.IngredientId.Value);
                }
            }
        }

        var allRecipes = await _uow.Recipes.GetAllAsync();

        // Sort recipes by how many ingredients match available stock
        var matched = allRecipes
            .Where(r => r.Ingredients.Any(i => stockIngredientIds.Contains(i.IngredientId) || stockNames.Contains(i.IngredientName.ToLowerInvariant().Trim())))
            .OrderByDescending(r => r.Ingredients.Count(i => stockIngredientIds.Contains(i.IngredientId) || stockNames.Contains(i.IngredientName.ToLowerInvariant().Trim())))
            .Select(FoodMappers.MapToDto)
            .ToList();

        return Result.Success(matched);
    }
}

public class GetStockInventoryQueryHandler : BaseQueryHandler<GetStockInventoryQuery, Result<PagedResult<PantryStockItemDto, StockFilter>>>
{
    private readonly INoSqlUnitOfWork _uow;
    public GetStockInventoryQueryHandler(INoSqlUnitOfWork uow, ILogger<GetStockInventoryQueryHandler> logger) : base(logger) => _uow = uow;

    protected override async Task<Result<PagedResult<PantryStockItemDto, StockFilter>>> ExecuteAsync(GetStockInventoryQuery request, CancellationToken ct)
    {
        var filter = request.Filter ?? new StockFilter { UserId = Guid.Empty };
        var items = await _uow.PantryStockItems.FindAsync(x => x.UserId == filter.UserId);

        if (!filter.IncludeDepleted)
        {
            items = items.Where(x => !x.IsDepleted).ToList();
        }

        if (filter.Location.HasValue)
        {
            items = items.Where(x => x.Location == filter.Location.Value).ToList();
        }

        if (filter.OnlyLowStock)
        {
            items = items.Where(x => x.IsBelowParLevel()).ToList();
        }

        if (filter.OnlyExpiringSoon)
        {
            items = items.Where(x => x.IsExpiringSoon(3) || x.IsExpired()).ToList();
        }

        if (filter.OnlyStaples)
        {
            items = items.Where(x => x.IsStaple).ToList();
        }

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var search = filter.SearchTerm.Trim().ToLowerInvariant();
            items = items.Where(x => x.IngredientName.ToLowerInvariant().Contains(search) || (x.BatchCode != null && x.BatchCode.ToLowerInvariant().Contains(search))).ToList();
        }

        int totalCount = items.Count;

        items = filter.SortBy?.ToLowerInvariant() switch
        {
            "expirydate" => filter.SortDescending ? items.OrderByDescending(x => x.ExpiryDate ?? DateTime.MaxValue).ToList() : items.OrderBy(x => x.ExpiryDate ?? DateTime.MaxValue).ToList(),
            "quantity" => filter.SortDescending ? items.OrderByDescending(x => x.CurrentQuantity).ToList() : items.OrderBy(x => x.CurrentQuantity).ToList(),
            "location" => filter.SortDescending ? items.OrderByDescending(x => x.Location).ToList() : items.OrderBy(x => x.Location).ToList(),
            _ => filter.SortDescending ? items.OrderByDescending(x => x.IngredientName).ToList() : items.OrderBy(x => x.IngredientName).ToList()
        };

        if (filter.PageSize > 0)
        {
            items = items.Skip((filter.PageNumber - 1) * filter.PageSize).Take(filter.PageSize).ToList();
        }

        var dtos = items.Select(FoodMappers.MapToDto).ToList();
        return Result.Success(new PagedResult<PantryStockItemDto, StockFilter>(dtos, totalCount, filter.PageNumber, filter.PageSize, filter));
    }
}

public class GetStockOverviewQueryHandler : BaseQueryHandler<GetStockOverviewQuery, Result<StockInventoryOverviewDto>>
{
    private readonly INoSqlUnitOfWork _uow;
    public GetStockOverviewQueryHandler(INoSqlUnitOfWork uow, ILogger<GetStockOverviewQueryHandler> logger) : base(logger) => _uow = uow;

    protected override async Task<Result<StockInventoryOverviewDto>> ExecuteAsync(GetStockOverviewQuery request, CancellationToken ct)
    {
        var allItems = await _uow.PantryStockItems.FindAsync(x => x.UserId == request.UserId && !x.IsDepleted);
        var recentMovements = (await _uow.StockMovements.FindAsync(x => x.UserId == request.UserId))
            .OrderByDescending(m => m.Timestamp)
            .Take(15)
            .Select(FoodMappers.MapToDto)
            .ToList();

        int total = allItems.Count;
        int lowStock = allItems.Count(x => x.IsBelowParLevel());
        int expiringSoon = allItems.Count(x => x.IsExpiringSoon(3));
        int expired = allItems.Count(x => x.IsExpired());
        decimal totalValue = allItems.Sum(x => (x.UnitPrice ?? 0m) * (decimal)Math.Max(0, x.CurrentQuantity));

        var dtos = allItems.OrderBy(x => x.ExpiryDate ?? DateTime.MaxValue).Select(FoodMappers.MapToDto).ToList();

        var overview = new StockInventoryOverviewDto(
            total,
            lowStock,
            expiringSoon,
            expired,
            Math.Round(totalValue, 2),
            dtos,
            recentMovements
        );

        return Result.Success(overview);
    }
}

public class GetStockMovementsQueryHandler : BaseQueryHandler<GetStockMovementsQuery, Result<PagedResult<StockMovementDto, StockMovementFilter>>>
{
    private readonly INoSqlUnitOfWork _uow;
    public GetStockMovementsQueryHandler(INoSqlUnitOfWork uow, ILogger<GetStockMovementsQueryHandler> logger) : base(logger) => _uow = uow;

    protected override async Task<Result<PagedResult<StockMovementDto, StockMovementFilter>>> ExecuteAsync(GetStockMovementsQuery request, CancellationToken ct)
    {
        var filter = request.Filter ?? new StockMovementFilter { UserId = Guid.Empty };
        var movements = await _uow.StockMovements.FindAsync(x => x.UserId == filter.UserId);

        if (filter.IngredientId.HasValue)
        {
            movements = movements.Where(x => x.IngredientId == filter.IngredientId.Value).ToList();
        }

        if (filter.MovementType.HasValue)
        {
            movements = movements.Where(x => x.MovementType == filter.MovementType.Value).ToList();
        }

        if (filter.FromDate.HasValue)
        {
            movements = movements.Where(x => x.Timestamp >= filter.FromDate.Value).ToList();
        }

        if (filter.ToDate.HasValue)
        {
            movements = movements.Where(x => x.Timestamp <= filter.ToDate.Value).ToList();
        }

        int total = movements.Count;
        movements = movements.OrderByDescending(x => x.Timestamp).ToList();

        if (filter.PageSize > 0)
        {
            movements = movements.Skip((filter.PageNumber - 1) * filter.PageSize).Take(filter.PageSize).ToList();
        }

        var dtos = movements.Select(FoodMappers.MapToDto).ToList();
        return Result.Success(new PagedResult<StockMovementDto, StockMovementFilter>(dtos, total, filter.PageNumber, filter.PageSize, filter));
    }
}

public class GetStockItemByIdQueryHandler : BaseQueryHandler<GetStockItemByIdQuery, Result<PantryStockItemDto>>
{
    private readonly INoSqlUnitOfWork _uow;
    public GetStockItemByIdQueryHandler(INoSqlUnitOfWork uow, ILogger<GetStockItemByIdQueryHandler> logger) : base(logger) => _uow = uow;

    protected override async Task<Result<PantryStockItemDto>> ExecuteAsync(GetStockItemByIdQuery request, CancellationToken ct)
    {
        var item = await _uow.PantryStockItems.GetByIdAsync(request.Id, ct);
        if (item == null || item.UserId != request.UserId) return Result.Failure<PantryStockItemDto>("Stock item not found.");
        return Result.Success(FoodMappers.MapToDto(item));
    }
}

public class GetShoppingListQueryHandler : BaseQueryHandler<GetShoppingListQuery, Result<ShoppingListGroupedDto>>
{
    private readonly INoSqlUnitOfWork _uow;
    public GetShoppingListQueryHandler(INoSqlUnitOfWork uow, ILogger<GetShoppingListQueryHandler> logger) : base(logger) => _uow = uow;

    protected override async Task<Result<ShoppingListGroupedDto>> ExecuteAsync(GetShoppingListQuery request, CancellationToken ct)
    {
        var filter = request.Filter ?? new ShoppingListFilter { UserId = Guid.Empty };
        var items = filter.IncludeShared
            ? await _uow.ShoppingListItems.FindAsync(x => x.UserId == filter.UserId || x.IsShared == true)
            : await _uow.ShoppingListItems.FindAsync(x => x.UserId == filter.UserId);

        if (filter.Status.HasValue)
        {
            items = items.Where(x => x.Status == filter.Status.Value).ToList();
        }

        if (filter.StoreSection.HasValue)
        {
            items = items.Where(x => x.StoreSection == filter.StoreSection.Value).ToList();
        }

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var search = filter.SearchTerm.Trim().ToLowerInvariant();
            items = items.Where(x => x.Name.ToLowerInvariant().Contains(search) || (x.Notes != null && x.Notes.ToLowerInvariant().Contains(search))).ToList();
        }

        int total = items.Count;
        int toBuy = items.Count(x => x.Status == ShoppingItemStatus.ToBuy);
        int inCart = items.Count(x => x.Status == ShoppingItemStatus.InCart);
        int bought = items.Count(x => x.Status == ShoppingItemStatus.Bought);
        decimal estimatedCost = items.Sum(x => x.EstimatedCost ?? 0m);

        var grouped = items
            .OrderBy(x => x.StoreSection)
            .ThenBy(x => x.Status)
            .ThenBy(x => x.Name)
            .GroupBy(x => x.StoreSection)
            .ToDictionary(g => g.Key, g => g.Select(FoodMappers.MapToDto).ToList());

        var result = new ShoppingListGroupedDto(total, toBuy, inCart, bought, Math.Round(estimatedCost, 2), grouped);
        return Result.Success(result);
    }
}

public class GetMissingRecipeIngredientsQueryHandler : BaseQueryHandler<GetMissingRecipeIngredientsQuery, Result<List<GroceryShoppingItemDto>>>
{
    private readonly INoSqlUnitOfWork _uow;
    public GetMissingRecipeIngredientsQueryHandler(INoSqlUnitOfWork uow, ILogger<GetMissingRecipeIngredientsQueryHandler> logger) : base(logger) => _uow = uow;

    protected override async Task<Result<List<GroceryShoppingItemDto>>> ExecuteAsync(GetMissingRecipeIngredientsQuery request, CancellationToken ct)
    {
        var recipe = await _uow.Recipes.GetByIdAsync(request.RecipeId, ct);
        if (recipe == null) return Result.Failure<List<GroceryShoppingItemDto>>("Recipe not found.");

        double servings = Math.Max(1.0, request.Servings);
        double scale = servings / Math.Max(1.0, recipe.Servings);

        var activeStock = await _uow.PantryStockItems.FindAsync(x => x.UserId == request.UserId && !x.IsDepleted);
        var missingList = new List<GroceryShoppingItemDto>();

        foreach (var ing in recipe.Ingredients)
        {
            double neededQty = ing.Quantity * scale;
            var availableLots = activeStock.Where(s => s.IngredientId == ing.IngredientId).ToList();
            double totalAvailable = availableLots.Sum(s => s.CurrentQuantity);

            if (totalAvailable < neededQty)
            {
                double deficit = Math.Round(neededQty - totalAvailable, 2);
                missingList.Add(new GroceryShoppingItemDto(
                    Guid.NewGuid(),
                    request.UserId,
                    ing.IngredientId,
                    ing.IngredientName,
                    deficit,
                    ing.Unit,
                    0,
                    ShoppingItemStatus.ToBuy,
                    StoreSection.Produce,
                    ShoppingItemSource.RecipeMissing,
                    StockLocation.Pantry,
                    null,
                    $"Needed for {recipe.Name} ({servings} servings)",
                    DateTime.UtcNow
                ));
            }
        }

        return Result.Success(missingList);
    }
}

// ======================== UNIT INTERFACE ========================

public interface IFoodQueryUnit
{
    GetAllIngredientsQueryHandler GetAllIngredients { get; }
    GetIngredientByIdQueryHandler GetIngredientById { get; }
    GetAllRecipesQueryHandler GetAllRecipes { get; }
    GetRecipeByIdQueryHandler GetRecipeById { get; }
    GetDailyNutritionSummaryQueryHandler GetDailyNutritionSummary { get; }
    GetNutritionTargetQueryHandler GetNutritionTarget { get; }
    GetAthleteProfileQueryHandler GetAthleteProfile { get; }
    GetChefScalingQueryHandler GetChefScaling { get; }
    GetPantryMatchRecipesQueryHandler GetPantryMatchRecipes { get; }

    GetStockInventoryQueryHandler GetStockInventory { get; }
    GetStockOverviewQueryHandler GetStockOverview { get; }
    GetStockMovementsQueryHandler GetStockMovements { get; }
    GetStockItemByIdQueryHandler GetStockItemById { get; }
    GetShoppingListQueryHandler GetShoppingList { get; }
    GetMissingRecipeIngredientsQueryHandler GetMissingRecipeIngredients { get; }
}

public class FoodQueryUnit : IFoodQueryUnit
{
    public GetAllIngredientsQueryHandler GetAllIngredients { get; }
    public GetIngredientByIdQueryHandler GetIngredientById { get; }
    public GetAllRecipesQueryHandler GetAllRecipes { get; }
    public GetRecipeByIdQueryHandler GetRecipeById { get; }
    public GetDailyNutritionSummaryQueryHandler GetDailyNutritionSummary { get; }
    public GetNutritionTargetQueryHandler GetNutritionTarget { get; }
    public GetAthleteProfileQueryHandler GetAthleteProfile { get; }
    public GetChefScalingQueryHandler GetChefScaling { get; }
    public GetPantryMatchRecipesQueryHandler GetPantryMatchRecipes { get; }

    public GetStockInventoryQueryHandler GetStockInventory { get; }
    public GetStockOverviewQueryHandler GetStockOverview { get; }
    public GetStockMovementsQueryHandler GetStockMovements { get; }
    public GetStockItemByIdQueryHandler GetStockItemById { get; }
    public GetShoppingListQueryHandler GetShoppingList { get; }
    public GetMissingRecipeIngredientsQueryHandler GetMissingRecipeIngredients { get; }

    public FoodQueryUnit(
        GetAllIngredientsQueryHandler getAllIngredients,
        GetIngredientByIdQueryHandler getIngredientById,
        GetAllRecipesQueryHandler getAllRecipes,
        GetRecipeByIdQueryHandler getRecipeById,
        GetDailyNutritionSummaryQueryHandler getDailyNutritionSummary,
        GetNutritionTargetQueryHandler getNutritionTarget,
        GetAthleteProfileQueryHandler getAthleteProfile,
        GetChefScalingQueryHandler getChefScaling,
        GetPantryMatchRecipesQueryHandler getPantryMatchRecipes,
        GetStockInventoryQueryHandler getStockInventory,
        GetStockOverviewQueryHandler getStockOverview,
        GetStockMovementsQueryHandler getStockMovements,
        GetStockItemByIdQueryHandler getStockItemById,
        GetShoppingListQueryHandler getShoppingList,
        GetMissingRecipeIngredientsQueryHandler getMissingRecipeIngredients)
    {
        GetAllIngredients = getAllIngredients;
        GetIngredientById = getIngredientById;
        GetAllRecipes = getAllRecipes;
        GetRecipeById = getRecipeById;
        GetDailyNutritionSummary = getDailyNutritionSummary;
        GetNutritionTarget = getNutritionTarget;
        GetAthleteProfile = getAthleteProfile;
        GetChefScaling = getChefScaling;
        GetPantryMatchRecipes = getPantryMatchRecipes;

        GetStockInventory = getStockInventory;
        GetStockOverview = getStockOverview;
        GetStockMovements = getStockMovements;
        GetStockItemById = getStockItemById;
        GetShoppingList = getShoppingList;
        GetMissingRecipeIngredients = getMissingRecipeIngredients;
    }
}
