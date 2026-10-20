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
        var targetDoc = (await _uow.NutritionTargets.FindAsync(x => x.UserId == request.UserId)).FirstOrDefault();
        var pantryNames = targetDoc?.Pantry.Select(p => p.Name.ToLowerInvariant().Trim()).ToHashSet() ?? new();

        var allRecipes = await _uow.Recipes.GetAllAsync();

        // Sort recipes by how many ingredients match pantry items
        var matched = allRecipes
            .Where(r => r.Ingredients.Any(i => pantryNames.Contains(i.IngredientName.ToLowerInvariant().Trim())))
            .OrderByDescending(r => r.Ingredients.Count(i => pantryNames.Contains(i.IngredientName.ToLowerInvariant().Trim())))
            .Select(FoodMappers.MapToDto)
            .ToList();

        return Result.Success(matched);
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

    public FoodQueryUnit(
        GetAllIngredientsQueryHandler getAllIngredients,
        GetIngredientByIdQueryHandler getIngredientById,
        GetAllRecipesQueryHandler getAllRecipes,
        GetRecipeByIdQueryHandler getRecipeById,
        GetDailyNutritionSummaryQueryHandler getDailyNutritionSummary,
        GetNutritionTargetQueryHandler getNutritionTarget,
        GetAthleteProfileQueryHandler getAthleteProfile,
        GetChefScalingQueryHandler getChefScaling,
        GetPantryMatchRecipesQueryHandler getPantryMatchRecipes)
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
    }
}
