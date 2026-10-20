using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Food;
using AzureTest.Domain.Documents.Food;
using AzureTest.Domain.Documents.Food.Enums;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.Food;

// ======================== COMMANDS ========================

public record CreateIngredientCommand(Guid? UserId, CreateIngredientDto Dto) : ICommand<Result<Guid>>;
public record UpdateIngredientCommand(Guid? UserId, UpdateIngredientDto Dto) : ICommand<Result>;
public record DeleteIngredientCommand(Guid? UserId, Guid Id) : ICommand<Result>;

public record CreateRecipeCommand(Guid UserId, CreateRecipeDto Dto) : ICommand<Result<Guid>>;
public record UpdateRecipeCommand(Guid UserId, UpdateRecipeDto Dto) : ICommand<Result>;
public record DeleteRecipeCommand(Guid UserId, Guid Id) : ICommand<Result>;
public record ToggleRecipeFavoriteCommand(Guid UserId, Guid Id) : ICommand<Result<bool>>;

public record LogMealItemCommand(Guid UserId, LogMealItemDto Dto) : ICommand<Result<Guid>>;
public record RemoveMealItemCommand(Guid UserId, Guid MealLogId, Guid ItemId) : ICommand<Result>;
public record LogWaterCommand(Guid UserId, LogWaterDto Dto) : ICommand<Result>;

public record UpdateNutritionTargetCommand(Guid UserId, UpdateNutritionTargetDto Dto) : ICommand<Result>;
public record UpsertPantryItemCommand(Guid UserId, UpsertPantryItemDto Dto) : ICommand<Result>;
public record DeletePantryItemCommand(Guid UserId, Guid ItemId) : ICommand<Result>;

public record UpsertShoppingListItemCommand(Guid UserId, UpsertShoppingListItemDto Dto) : ICommand<Result>;
public record ToggleShoppingItemPurchasedCommand(Guid UserId, Guid ItemId) : ICommand<Result>;
public record DeleteShoppingListItemCommand(Guid UserId, Guid ItemId) : ICommand<Result>;
public record AddRecipeToShoppingListCommand(Guid UserId, Guid RecipeId) : ICommand<Result>;

public record UpsertWeeklyMealPlanSlotCommand(Guid UserId, UpsertWeeklyMealPlanSlotDto Dto) : ICommand<Result>;
public record DeleteWeeklyMealPlanSlotCommand(Guid UserId, Guid SlotId) : ICommand<Result>;
public record SeedDefaultIngredientsCommand() : ICommand<Result>;

// ======================== HANDLERS ========================

public class CreateIngredientCommandHandler : ICommandHandler<CreateIngredientCommand, Result<Guid>>
{
    private readonly INoSqlUnitOfWork _uow;
    public CreateIngredientCommandHandler(INoSqlUnitOfWork uow) => _uow = uow;

    public async Task<Result<Guid>> HandleAsync(CreateIngredientCommand request, CancellationToken ct = default)
    {
        var doc = IngredientDocument.Create(
            request.Dto.Name,
            request.Dto.Category,
            request.Dto.Calories,
            request.Dto.ProteinGrams,
            request.Dto.CarbsGrams,
            request.Dto.FatGrams,
            request.Dto.FiberGrams,
            request.Dto.SugarGrams,
            request.Dto.SodiumMg,
            request.Dto.DefaultServingSizeGrams,
            request.Dto.DefaultUnit,
            request.Dto.DensityGramsPerMl,
            request.Dto.GramsPerPiece,
            request.Dto.GramsPerTbsp,
            request.Dto.GramsPerCup,
            request.Dto.AllowedUnits,
            request.Dto.DietaryTags,
            request.Dto.AllergenInfo,
            request.Dto.ApproximateCostPer100g,
            request.UserId
        );
        doc.Description = request.Dto.Description;

        await _uow.Ingredients.AddAsync(doc, ct);
        return Result.Success(doc.Id);
    }
}

public class UpdateIngredientCommandHandler : ICommandHandler<UpdateIngredientCommand, Result>
{
    private readonly INoSqlUnitOfWork _uow;
    public UpdateIngredientCommandHandler(INoSqlUnitOfWork uow) => _uow = uow;

    public async Task<Result> HandleAsync(UpdateIngredientCommand request, CancellationToken ct = default)
    {
        var existing = await _uow.Ingredients.GetByIdAsync(request.Dto.Id, ct);
        if (existing == null) return Result.Failure("Ingredient not found.");

        existing.Name = request.Dto.Name.Trim();
        existing.Category = request.Dto.Category;
        existing.Description = request.Dto.Description;
        existing.Calories = Math.Round(request.Dto.Calories, 1);
        existing.ProteinGrams = Math.Round(request.Dto.ProteinGrams, 1);
        existing.CarbsGrams = Math.Round(request.Dto.CarbsGrams, 1);
        existing.FatGrams = Math.Round(request.Dto.FatGrams, 1);
        existing.FiberGrams = Math.Round(request.Dto.FiberGrams, 1);
        existing.SugarGrams = Math.Round(request.Dto.SugarGrams, 1);
        existing.SodiumMg = Math.Round(request.Dto.SodiumMg, 1);
        existing.DefaultServingSizeGrams = request.Dto.DefaultServingSizeGrams > 0 ? request.Dto.DefaultServingSizeGrams : 100;
        existing.DefaultUnit = request.Dto.DefaultUnit;
        existing.DensityGramsPerMl = request.Dto.DensityGramsPerMl > 0 ? request.Dto.DensityGramsPerMl : 1.0;
        existing.GramsPerPiece = request.Dto.GramsPerPiece;
        existing.GramsPerTbsp = request.Dto.GramsPerTbsp;
        existing.GramsPerCup = request.Dto.GramsPerCup;
        existing.AllowedUnits = request.Dto.AllowedUnits ?? new();
        existing.DietaryTags = request.Dto.DietaryTags ?? new();
        existing.AllergenInfo = request.Dto.AllergenInfo?.Trim();
        existing.ApproximateCostPer100g = request.Dto.ApproximateCostPer100g;

        await _uow.Ingredients.UpdateAsync(existing.Id, existing, ct);
        return Result.Success();
    }
}

public class DeleteIngredientCommandHandler : ICommandHandler<DeleteIngredientCommand, Result>
{
    private readonly INoSqlUnitOfWork _uow;
    public DeleteIngredientCommandHandler(INoSqlUnitOfWork uow) => _uow = uow;

    public async Task<Result> HandleAsync(DeleteIngredientCommand request, CancellationToken ct = default)
    {
        var existing = await _uow.Ingredients.GetByIdAsync(request.Id, ct);
        if (existing == null) return Result.Failure("Ingredient not found.");

        await _uow.Ingredients.RemoveAsync(request.Id, ct);
        return Result.Success();
    }
}

public class CreateRecipeCommandHandler : ICommandHandler<CreateRecipeCommand, Result<Guid>>
{
    private readonly INoSqlUnitOfWork _uow;
    public CreateRecipeCommandHandler(INoSqlUnitOfWork uow) => _uow = uow;

    public async Task<Result<Guid>> HandleAsync(CreateRecipeCommand request, CancellationToken ct = default)
    {
        var recipe = new RecipeDocument
        {
            UserId = request.UserId,
            Name = request.Dto.Name.Trim(),
            Description = request.Dto.Description,
            Category = string.IsNullOrWhiteSpace(request.Dto.Category) ? "Main Dish" : request.Dto.Category.Trim(),
            Servings = Math.Max(1, request.Dto.Servings),
            PrepTimeMinutes = Math.Max(0, request.Dto.PrepTimeMinutes),
            CookTimeMinutes = Math.Max(0, request.Dto.CookTimeMinutes),
            Difficulty = request.Dto.Difficulty,
            Tags = request.Dto.Tags ?? new(),
            DietaryTags = request.Dto.DietaryTags ?? new(),
            CookingYieldFactor = request.Dto.CookingYieldFactor > 0 ? request.Dto.CookingYieldFactor : 1.0,
            ImageUrl = request.Dto.ImageUrl,
            EstimatedCost = request.Dto.EstimatedCost,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var itemDto in request.Dto.Ingredients)
        {
            var ingDoc = await _uow.Ingredients.GetByIdAsync(itemDto.IngredientId, ct);
            if (ingDoc == null) continue;

            double grams = ingDoc.CalculateGrams(itemDto.Quantity, itemDto.Unit);
            var (cal, pro, carb, fat, fib, sug, sod) = ingDoc.CalculateNutritionFor(itemDto.Quantity, itemDto.Unit);

            recipe.Ingredients.Add(new RecipeIngredientItem
            {
                IngredientId = ingDoc.Id,
                IngredientName = ingDoc.Name,
                Quantity = itemDto.Quantity,
                Unit = itemDto.Unit,
                Notes = itemDto.Notes,
                SubstitutionNote = itemDto.SubstitutionNote,
                ComputedGrams = grams,
                Calories = cal,
                Protein = pro,
                Carbs = carb,
                Fat = fat,
                Fiber = fib,
                Sugar = sug,
                Sodium = sod
            });
        }

        if (request.Dto.Steps != null)
        {
            int stepNum = 1;
            foreach (var step in request.Dto.Steps.OrderBy(s => s.StepNumber))
            {
                recipe.Steps.Add(new RecipeStepItem
                {
                    StepNumber = stepNum++,
                    Instruction = step.Instruction.Trim(),
                    TimerMinutes = step.TimerMinutes,
                    ChefTip = step.ChefTip
                });
            }
        }

        recipe.RecalculateTotals();
        await _uow.Recipes.AddAsync(recipe, ct);
        return Result.Success(recipe.Id);
    }
}

public class UpdateRecipeCommandHandler : ICommandHandler<UpdateRecipeCommand, Result>
{
    private readonly INoSqlUnitOfWork _uow;
    public UpdateRecipeCommandHandler(INoSqlUnitOfWork uow) => _uow = uow;

    public async Task<Result> HandleAsync(UpdateRecipeCommand request, CancellationToken ct = default)
    {
        var recipe = await _uow.Recipes.GetByIdAsync(request.Dto.Id, ct);
        if (recipe == null) return Result.Failure("Recipe not found.");
        if (recipe.UserId != request.UserId) return Result.Failure("Unauthorized.");

        recipe.Name = request.Dto.Name.Trim();
        recipe.Description = request.Dto.Description;
        recipe.Category = string.IsNullOrWhiteSpace(request.Dto.Category) ? "Main Dish" : request.Dto.Category.Trim();
        recipe.Servings = Math.Max(1, request.Dto.Servings);
        recipe.PrepTimeMinutes = Math.Max(0, request.Dto.PrepTimeMinutes);
        recipe.CookTimeMinutes = Math.Max(0, request.Dto.CookTimeMinutes);
        recipe.Difficulty = request.Dto.Difficulty;
        recipe.Tags = request.Dto.Tags ?? new();
        recipe.DietaryTags = request.Dto.DietaryTags ?? new();
        recipe.CookingYieldFactor = request.Dto.CookingYieldFactor > 0 ? request.Dto.CookingYieldFactor : 1.0;
        recipe.ImageUrl = request.Dto.ImageUrl;
        recipe.EstimatedCost = request.Dto.EstimatedCost;
        recipe.UpdatedAt = DateTime.UtcNow;

        recipe.Ingredients.Clear();
        foreach (var itemDto in request.Dto.Ingredients)
        {
            var ingDoc = await _uow.Ingredients.GetByIdAsync(itemDto.IngredientId, ct);
            if (ingDoc == null) continue;

            double grams = ingDoc.CalculateGrams(itemDto.Quantity, itemDto.Unit);
            var (cal, pro, carb, fat, fib, sug, sod) = ingDoc.CalculateNutritionFor(itemDto.Quantity, itemDto.Unit);

            recipe.Ingredients.Add(new RecipeIngredientItem
            {
                IngredientId = ingDoc.Id,
                IngredientName = ingDoc.Name,
                Quantity = itemDto.Quantity,
                Unit = itemDto.Unit,
                Notes = itemDto.Notes,
                SubstitutionNote = itemDto.SubstitutionNote,
                ComputedGrams = grams,
                Calories = cal,
                Protein = pro,
                Carbs = carb,
                Fat = fat,
                Fiber = fib,
                Sugar = sug,
                Sodium = sod
            });
        }

        recipe.Steps.Clear();
        if (request.Dto.Steps != null)
        {
            int stepNum = 1;
            foreach (var step in request.Dto.Steps.OrderBy(s => s.StepNumber))
            {
                recipe.Steps.Add(new RecipeStepItem
                {
                    StepNumber = stepNum++,
                    Instruction = step.Instruction.Trim(),
                    TimerMinutes = step.TimerMinutes,
                    ChefTip = step.ChefTip
                });
            }
        }

        recipe.RecalculateTotals();
        await _uow.Recipes.UpdateAsync(recipe.Id, recipe, ct);
        return Result.Success();
    }
}

public class DeleteRecipeCommandHandler : ICommandHandler<DeleteRecipeCommand, Result>
{
    private readonly INoSqlUnitOfWork _uow;
    public DeleteRecipeCommandHandler(INoSqlUnitOfWork uow) => _uow = uow;

    public async Task<Result> HandleAsync(DeleteRecipeCommand request, CancellationToken ct = default)
    {
        var recipe = await _uow.Recipes.GetByIdAsync(request.Id, ct);
        if (recipe == null) return Result.Failure("Recipe not found.");
        if (recipe.UserId != request.UserId) return Result.Failure("Unauthorized.");

        await _uow.Recipes.RemoveAsync(request.Id, ct);
        return Result.Success();
    }
}

public class ToggleRecipeFavoriteCommandHandler : ICommandHandler<ToggleRecipeFavoriteCommand, Result<bool>>
{
    private readonly INoSqlUnitOfWork _uow;
    public ToggleRecipeFavoriteCommandHandler(INoSqlUnitOfWork uow) => _uow = uow;

    public async Task<Result<bool>> HandleAsync(ToggleRecipeFavoriteCommand request, CancellationToken ct = default)
    {
        var recipe = await _uow.Recipes.GetByIdAsync(request.Id, ct);
        if (recipe == null) return Result.Failure<bool>("Recipe not found.");

        recipe.IsFavorite = !recipe.IsFavorite;
        recipe.UpdatedAt = DateTime.UtcNow;
        await _uow.Recipes.UpdateAsync(recipe.Id, recipe, ct);
        return Result.Success(recipe.IsFavorite);
    }
}

public class LogMealItemCommandHandler : ICommandHandler<LogMealItemCommand, Result<Guid>>
{
    private readonly INoSqlUnitOfWork _uow;
    public LogMealItemCommandHandler(INoSqlUnitOfWork uow) => _uow = uow;

    public async Task<Result<Guid>> HandleAsync(LogMealItemCommand request, CancellationToken ct = default)
    {
        var startOfDay = request.Dto.Date.Date;
        var endOfDay = startOfDay.AddDays(1);

        var existingLogs = await _uow.MealLogs.FindAsync(x =>
            x.UserId == request.UserId &&
            x.MealType == request.Dto.MealType &&
            x.Date >= startOfDay && x.Date < endOfDay);

        var mealLog = existingLogs.FirstOrDefault();
        if (mealLog == null)
        {
            mealLog = new MealLogDocument
            {
                UserId = request.UserId,
                Date = startOfDay,
                MealType = request.Dto.MealType,
                LoggedAt = DateTime.UtcNow
            };
        }

        var item = new MealLogItem
        {
            IsRecipe = request.Dto.IsRecipe,
            ReferenceId = request.Dto.ReferenceId,
            Quantity = request.Dto.Quantity,
            Unit = request.Dto.Unit
        };

        if (request.Dto.IsRecipe)
        {
            var recipe = await _uow.Recipes.GetByIdAsync(request.Dto.ReferenceId, ct);
            if (recipe == null) return Result.Failure<Guid>("Recipe not found.");

            item.Name = recipe.Name;
            double servings = request.Dto.Quantity;
            item.EquivalentGramsOrServings = servings;
            item.Calories = Math.Round(recipe.PerServingCalories * servings, 1);
            item.Protein = Math.Round(recipe.PerServingProtein * servings, 1);
            item.Carbs = Math.Round(recipe.PerServingCarbs * servings, 1);
            item.Fat = Math.Round(recipe.PerServingFat * servings, 1);
            item.Fiber = Math.Round(recipe.PerServingFiber * servings, 1);
            item.Sugar = Math.Round((recipe.TotalSugar / recipe.Servings) * servings, 1);
            item.Sodium = Math.Round((recipe.TotalSodium / recipe.Servings) * servings, 1);
        }
        else
        {
            var ingredient = await _uow.Ingredients.GetByIdAsync(request.Dto.ReferenceId, ct);
            if (ingredient == null) return Result.Failure<Guid>("Ingredient not found.");

            item.Name = ingredient.Name;
            double grams = ingredient.CalculateGrams(request.Dto.Quantity, request.Dto.Unit);
            item.EquivalentGramsOrServings = grams;
            var (cal, pro, carb, fat, fib, sug, sod) = ingredient.CalculateNutritionFor(request.Dto.Quantity, request.Dto.Unit);
            item.Calories = cal;
            item.Protein = pro;
            item.Carbs = carb;
            item.Fat = fat;
            item.Fiber = fib;
            item.Sugar = sug;
            item.Sodium = sod;
        }

        mealLog.Items.Add(item);
        mealLog.RecalculateTotals();

        if (existingLogs.Any())
        {
            await _uow.MealLogs.UpdateAsync(mealLog.Id, mealLog, ct);
        }
        else
        {
            await _uow.MealLogs.AddAsync(mealLog, ct);
        }

        return Result.Success(item.ItemId);
    }
}

public class RemoveMealItemCommandHandler : ICommandHandler<RemoveMealItemCommand, Result>
{
    private readonly INoSqlUnitOfWork _uow;
    public RemoveMealItemCommandHandler(INoSqlUnitOfWork uow) => _uow = uow;

    public async Task<Result> HandleAsync(RemoveMealItemCommand request, CancellationToken ct = default)
    {
        var mealLog = await _uow.MealLogs.GetByIdAsync(request.MealLogId, ct);
        if (mealLog == null || mealLog.UserId != request.UserId) return Result.Failure("Meal log entry not found.");

        mealLog.Items.RemoveAll(i => i.ItemId == request.ItemId);
        mealLog.RecalculateTotals();

        if (mealLog.Items.Count == 0 && mealLog.WaterMl == 0)
        {
            await _uow.MealLogs.RemoveAsync(mealLog.Id, ct);
        }
        else
        {
            await _uow.MealLogs.UpdateAsync(mealLog.Id, mealLog, ct);
        }

        return Result.Success();
    }
}

public class LogWaterCommandHandler : ICommandHandler<LogWaterCommand, Result>
{
    private readonly INoSqlUnitOfWork _uow;
    public LogWaterCommandHandler(INoSqlUnitOfWork uow) => _uow = uow;

    public async Task<Result> HandleAsync(LogWaterCommand request, CancellationToken ct = default)
    {
        var startOfDay = request.Dto.Date.Date;
        var endOfDay = startOfDay.AddDays(1);

        var existingLogs = await _uow.MealLogs.FindAsync(x =>
            x.UserId == request.UserId &&
            x.Date >= startOfDay && x.Date < endOfDay);

        var mealLog = existingLogs.FirstOrDefault();
        if (mealLog == null)
        {
            mealLog = new MealLogDocument
            {
                UserId = request.UserId,
                Date = startOfDay,
                MealType = MealType.Snack,
                WaterMl = request.Dto.WaterMl,
                LoggedAt = DateTime.UtcNow
            };
            await _uow.MealLogs.AddAsync(mealLog, ct);
        }
        else
        {
            mealLog.WaterMl += request.Dto.WaterMl;
            await _uow.MealLogs.UpdateAsync(mealLog.Id, mealLog, ct);
        }

        return Result.Success();
    }
}

public class UpdateNutritionTargetCommandHandler : ICommandHandler<UpdateNutritionTargetCommand, Result>
{
    private readonly INoSqlUnitOfWork _uow;
    public UpdateNutritionTargetCommandHandler(INoSqlUnitOfWork uow) => _uow = uow;

    public async Task<Result> HandleAsync(UpdateNutritionTargetCommand request, CancellationToken ct = default)
    {
        var targetDoc = (await _uow.NutritionTargets.FindAsync(x => x.UserId == request.UserId)).FirstOrDefault();
        if (targetDoc == null)
        {
            targetDoc = DailyNutritionTargetDocument.CreateDefault(request.UserId);
            await _uow.NutritionTargets.AddAsync(targetDoc, ct);
        }

        targetDoc.CalorieTarget = request.Dto.CalorieTarget;
        targetDoc.ProteinGramsTarget = request.Dto.ProteinGramsTarget;
        targetDoc.CarbsGramsTarget = request.Dto.CarbsGramsTarget;
        targetDoc.FatGramsTarget = request.Dto.FatGramsTarget;
        targetDoc.WaterMlTarget = request.Dto.WaterMlTarget > 0 ? request.Dto.WaterMlTarget : 3000;
        targetDoc.GoalType = request.Dto.GoalType;
        targetDoc.TargetProteinPerKg = request.Dto.TargetProteinPerKg;
        targetDoc.EnableCarbCycling = request.Dto.EnableCarbCycling;
        targetDoc.TrainingDayCalories = request.Dto.TrainingDayCalories;
        targetDoc.TrainingDayCarbs = request.Dto.TrainingDayCarbs;
        targetDoc.RestDayCalories = request.Dto.RestDayCalories;
        targetDoc.RestDayCarbs = request.Dto.RestDayCarbs;
        targetDoc.UpdatedAt = DateTime.UtcNow;

        await _uow.NutritionTargets.UpdateAsync(targetDoc.Id, targetDoc, ct);
        return Result.Success();
    }
}

public class UpsertPantryItemCommandHandler : ICommandHandler<UpsertPantryItemCommand, Result>
{
    private readonly INoSqlUnitOfWork _uow;
    public UpsertPantryItemCommandHandler(INoSqlUnitOfWork uow) => _uow = uow;

    public async Task<Result> HandleAsync(UpsertPantryItemCommand request, CancellationToken ct = default)
    {
        var targetDoc = (await _uow.NutritionTargets.FindAsync(x => x.UserId == request.UserId)).FirstOrDefault();
        if (targetDoc == null)
        {
            targetDoc = DailyNutritionTargetDocument.CreateDefault(request.UserId);
            await _uow.NutritionTargets.AddAsync(targetDoc, ct);
        }

        if (request.Dto.Id.HasValue && request.Dto.Id.Value != Guid.Empty)
        {
            var item = targetDoc.Pantry.FirstOrDefault(p => p.Id == request.Dto.Id.Value);
            if (item != null)
            {
                item.Name = request.Dto.Name.Trim();
                item.Quantity = request.Dto.Quantity;
                item.Unit = request.Dto.Unit;
                item.ExpiryDate = request.Dto.ExpiryDate;
                item.Location = request.Dto.Location;
            }
        }
        else
        {
            targetDoc.Pantry.Add(new PantryItem
            {
                IngredientId = request.Dto.IngredientId,
                Name = request.Dto.Name.Trim(),
                Quantity = request.Dto.Quantity,
                Unit = request.Dto.Unit,
                ExpiryDate = request.Dto.ExpiryDate,
                Location = request.Dto.Location
            });
        }

        targetDoc.UpdatedAt = DateTime.UtcNow;
        await _uow.NutritionTargets.UpdateAsync(targetDoc.Id, targetDoc, ct);
        return Result.Success();
    }
}

public class DeletePantryItemCommandHandler : ICommandHandler<DeletePantryItemCommand, Result>
{
    private readonly INoSqlUnitOfWork _uow;
    public DeletePantryItemCommandHandler(INoSqlUnitOfWork uow) => _uow = uow;

    public async Task<Result> HandleAsync(DeletePantryItemCommand request, CancellationToken ct = default)
    {
        var targetDoc = (await _uow.NutritionTargets.FindAsync(x => x.UserId == request.UserId)).FirstOrDefault();
        if (targetDoc == null) return Result.Failure("User targets not found.");

        targetDoc.Pantry.RemoveAll(p => p.Id == request.ItemId);
        targetDoc.UpdatedAt = DateTime.UtcNow;
        await _uow.NutritionTargets.UpdateAsync(targetDoc.Id, targetDoc, ct);
        return Result.Success();
    }
}

public class UpsertShoppingListItemCommandHandler : ICommandHandler<UpsertShoppingListItemCommand, Result>
{
    private readonly INoSqlUnitOfWork _uow;
    public UpsertShoppingListItemCommandHandler(INoSqlUnitOfWork uow) => _uow = uow;

    public async Task<Result> HandleAsync(UpsertShoppingListItemCommand request, CancellationToken ct = default)
    {
        var targetDoc = (await _uow.NutritionTargets.FindAsync(x => x.UserId == request.UserId)).FirstOrDefault();
        if (targetDoc == null)
        {
            targetDoc = DailyNutritionTargetDocument.CreateDefault(request.UserId);
            await _uow.NutritionTargets.AddAsync(targetDoc, ct);
        }

        if (request.Dto.Id.HasValue && request.Dto.Id.Value != Guid.Empty)
        {
            var item = targetDoc.ShoppingList.FirstOrDefault(s => s.Id == request.Dto.Id.Value);
            if (item != null)
            {
                item.Name = request.Dto.Name.Trim();
                item.Quantity = request.Dto.Quantity;
                item.Unit = request.Dto.Unit;
                item.IsPurchased = request.Dto.IsPurchased;
                item.Category = request.Dto.Category;
                item.Notes = request.Dto.Notes;
            }
        }
        else
        {
            targetDoc.ShoppingList.Add(new ShoppingListItem
            {
                IngredientId = request.Dto.IngredientId,
                Name = request.Dto.Name.Trim(),
                Quantity = request.Dto.Quantity,
                Unit = request.Dto.Unit,
                IsPurchased = request.Dto.IsPurchased,
                Category = request.Dto.Category,
                Notes = request.Dto.Notes
            });
        }

        targetDoc.UpdatedAt = DateTime.UtcNow;
        await _uow.NutritionTargets.UpdateAsync(targetDoc.Id, targetDoc, ct);
        return Result.Success();
    }
}

public class ToggleShoppingItemPurchasedCommandHandler : ICommandHandler<ToggleShoppingItemPurchasedCommand, Result>
{
    private readonly INoSqlUnitOfWork _uow;
    public ToggleShoppingItemPurchasedCommandHandler(INoSqlUnitOfWork uow) => _uow = uow;

    public async Task<Result> HandleAsync(ToggleShoppingItemPurchasedCommand request, CancellationToken ct = default)
    {
        var targetDoc = (await _uow.NutritionTargets.FindAsync(x => x.UserId == request.UserId)).FirstOrDefault();
        if (targetDoc == null) return Result.Failure("Targets not found.");

        var item = targetDoc.ShoppingList.FirstOrDefault(s => s.Id == request.ItemId);
        if (item != null)
        {
            item.IsPurchased = !item.IsPurchased;
            targetDoc.UpdatedAt = DateTime.UtcNow;
            await _uow.NutritionTargets.UpdateAsync(targetDoc.Id, targetDoc, ct);
        }

        return Result.Success();
    }
}

public class DeleteShoppingListItemCommandHandler : ICommandHandler<DeleteShoppingListItemCommand, Result>
{
    private readonly INoSqlUnitOfWork _uow;
    public DeleteShoppingListItemCommandHandler(INoSqlUnitOfWork uow) => _uow = uow;

    public async Task<Result> HandleAsync(DeleteShoppingListItemCommand request, CancellationToken ct = default)
    {
        var targetDoc = (await _uow.NutritionTargets.FindAsync(x => x.UserId == request.UserId)).FirstOrDefault();
        if (targetDoc == null) return Result.Failure("Targets not found.");

        targetDoc.ShoppingList.RemoveAll(s => s.Id == request.ItemId);
        targetDoc.UpdatedAt = DateTime.UtcNow;
        await _uow.NutritionTargets.UpdateAsync(targetDoc.Id, targetDoc, ct);
        return Result.Success();
    }
}

public class AddRecipeToShoppingListCommandHandler : ICommandHandler<AddRecipeToShoppingListCommand, Result>
{
    private readonly INoSqlUnitOfWork _uow;
    public AddRecipeToShoppingListCommandHandler(INoSqlUnitOfWork uow) => _uow = uow;

    public async Task<Result> HandleAsync(AddRecipeToShoppingListCommand request, CancellationToken ct = default)
    {
        var recipe = await _uow.Recipes.GetByIdAsync(request.RecipeId, ct);
        if (recipe == null) return Result.Failure("Recipe not found.");

        var targetDoc = (await _uow.NutritionTargets.FindAsync(x => x.UserId == request.UserId)).FirstOrDefault();
        if (targetDoc == null)
        {
            targetDoc = DailyNutritionTargetDocument.CreateDefault(request.UserId);
            await _uow.NutritionTargets.AddAsync(targetDoc, ct);
        }

        foreach (var ingredient in recipe.Ingredients)
        {
            targetDoc.ShoppingList.Add(new ShoppingListItem
            {
                IngredientId = ingredient.IngredientId,
                Name = ingredient.IngredientName,
                Quantity = ingredient.Quantity,
                Unit = ingredient.Unit,
                IsPurchased = false,
                Category = recipe.Category,
                Notes = $"For recipe: {recipe.Name}"
            });
        }

        targetDoc.UpdatedAt = DateTime.UtcNow;
        await _uow.NutritionTargets.UpdateAsync(targetDoc.Id, targetDoc, ct);
        return Result.Success();
    }
}

public class UpsertWeeklyMealPlanSlotCommandHandler : ICommandHandler<UpsertWeeklyMealPlanSlotCommand, Result>
{
    private readonly INoSqlUnitOfWork _uow;
    public UpsertWeeklyMealPlanSlotCommandHandler(INoSqlUnitOfWork uow) => _uow = uow;

    public async Task<Result> HandleAsync(UpsertWeeklyMealPlanSlotCommand request, CancellationToken ct = default)
    {
        var targetDoc = (await _uow.NutritionTargets.FindAsync(x => x.UserId == request.UserId)).FirstOrDefault();
        if (targetDoc == null)
        {
            targetDoc = DailyNutritionTargetDocument.CreateDefault(request.UserId);
            await _uow.NutritionTargets.AddAsync(targetDoc, ct);
        }

        if (request.Dto.Id.HasValue && request.Dto.Id.Value != Guid.Empty)
        {
            var slot = targetDoc.WeeklyPlan.FirstOrDefault(w => w.Id == request.Dto.Id.Value);
            if (slot != null)
            {
                slot.DayOfWeek = request.Dto.DayOfWeek;
                slot.MealType = request.Dto.MealType;
                slot.RecipeId = request.Dto.RecipeId;
                slot.RecipeName = request.Dto.RecipeName;
                slot.Servings = request.Dto.Servings;
                slot.Notes = request.Dto.Notes;
            }
        }
        else
        {
            targetDoc.WeeklyPlan.Add(new WeeklyMealPlanSlot
            {
                DayOfWeek = request.Dto.DayOfWeek,
                MealType = request.Dto.MealType,
                RecipeId = request.Dto.RecipeId,
                RecipeName = request.Dto.RecipeName,
                Servings = request.Dto.Servings,
                Notes = request.Dto.Notes
            });
        }

        targetDoc.UpdatedAt = DateTime.UtcNow;
        await _uow.NutritionTargets.UpdateAsync(targetDoc.Id, targetDoc, ct);
        return Result.Success();
    }
}

public class DeleteWeeklyMealPlanSlotCommandHandler : ICommandHandler<DeleteWeeklyMealPlanSlotCommand, Result>
{
    private readonly INoSqlUnitOfWork _uow;
    public DeleteWeeklyMealPlanSlotCommandHandler(INoSqlUnitOfWork uow) => _uow = uow;

    public async Task<Result> HandleAsync(DeleteWeeklyMealPlanSlotCommand request, CancellationToken ct = default)
    {
        var targetDoc = (await _uow.NutritionTargets.FindAsync(x => x.UserId == request.UserId)).FirstOrDefault();
        if (targetDoc == null) return Result.Failure("Targets not found.");

        targetDoc.WeeklyPlan.RemoveAll(w => w.Id == request.SlotId);
        targetDoc.UpdatedAt = DateTime.UtcNow;
        await _uow.NutritionTargets.UpdateAsync(targetDoc.Id, targetDoc, ct);
        return Result.Success();
    }
}

public class SeedDefaultIngredientsCommandHandler : ICommandHandler<SeedDefaultIngredientsCommand, Result>
{
    private readonly INoSqlUnitOfWork _uow;
    public SeedDefaultIngredientsCommandHandler(INoSqlUnitOfWork uow) => _uow = uow;

    public async Task<Result> HandleAsync(SeedDefaultIngredientsCommand request, CancellationToken ct = default)
    {
        var count = await _uow.Ingredients.GetAllAsync();
        if (count.Any()) return Result.Success();

        var seeds = new List<IngredientDocument>
        {
            IngredientDocument.Create("Chicken Breast (Raw)", FoodCategory.MeatAndPoultry, 165, 31, 0, 3.6, 0, 0, 74, 150, "g", 1.0, null, null, null, new() { "g", "piece" }, new() { DietaryTag.Halal, DietaryTag.HighProtein }, null, 0.9m),
            IngredientDocument.Create("Chicken Breast (Cooked/Grilled)", FoodCategory.MeatAndPoultry, 195, 36, 0, 4.8, 0, 0, 85, 120, "g", 1.0, null, null, null, new() { "g" }, new() { DietaryTag.Halal, DietaryTag.HighProtein }, null, 1.2m),
            IngredientDocument.Create("White Jasmine Rice (Raw)", FoodCategory.GrainsAndCereals, 365, 7.1, 80, 0.7, 1.3, 0.1, 5, 80, "g", 1.0, null, null, 185, new() { "g", "cup" }, new() { DietaryTag.Vegan, DietaryTag.Halal, DietaryTag.GlutenFree }, null, 0.3m),
            IngredientDocument.Create("White Rice (Cooked/Boiled)", FoodCategory.GrainsAndCereals, 130, 2.7, 28, 0.3, 0.4, 0.05, 1, 150, "g", 1.0, null, null, 158, new() { "g", "cup" }, new() { DietaryTag.Vegan, DietaryTag.Halal, DietaryTag.GlutenFree }, null, 0.2m),
            IngredientDocument.Create("Eggs (Large)", FoodCategory.DairyAndEggs, 143, 12.6, 0.7, 9.5, 0, 0.4, 142, 50, "piece", 1.0, 50, null, null, new() { "piece", "g" }, new() { DietaryTag.Vegetarian, DietaryTag.Halal, DietaryTag.HighProtein }, "Contains Egg", 0.4m),
            IngredientDocument.Create("Egg Whites", FoodCategory.DairyAndEggs, 52, 10.9, 0.7, 0.2, 0, 0.7, 166, 100, "g", 1.03, 33, 15, null, new() { "g", "ml" }, new() { DietaryTag.Vegetarian, DietaryTag.Halal, DietaryTag.HighProtein }, "Contains Egg", 0.5m),
            IngredientDocument.Create("Extra Virgin Olive Oil", FoodCategory.OilsAndFats, 884, 0, 0, 100, 0, 0, 2, 14, "tbsp", 0.92, null, 14, 216, new() { "tbsp", "ml", "g" }, new() { DietaryTag.Vegan, DietaryTag.Halal, DietaryTag.Keto }, null, 1.5m),
            IngredientDocument.Create("Rolled Oats", FoodCategory.GrainsAndCereals, 389, 16.9, 66.3, 6.9, 10.6, 0, 2, 60, "g", 1.0, null, null, 81, new() { "g", "cup" }, new() { DietaryTag.Vegan, DietaryTag.Halal, DietaryTag.GlutenFree }, null, 0.4m),
            IngredientDocument.Create("Greek Yogurt 0% Fat", FoodCategory.DairyAndEggs, 59, 10.3, 3.6, 0.4, 0, 3.2, 36, 170, "g", 1.05, null, 16, 245, new() { "g", "cup", "tbsp" }, new() { DietaryTag.Vegetarian, DietaryTag.Halal, DietaryTag.HighProtein }, "Contains Milk/Dairy", 0.6m),
            IngredientDocument.Create("Whey Protein Isolate", FoodCategory.Supplements, 380, 85, 4, 1.5, 1, 1, 150, 30, "g", 1.0, null, null, null, new() { "g", "piece" }, new() { DietaryTag.Vegetarian, DietaryTag.Halal, DietaryTag.HighProtein }, "Contains Milk/Dairy", 3.0m),
            IngredientDocument.Create("Atlantic Salmon Fillet (Raw)", FoodCategory.FishAndSeafood, 208, 20.4, 0, 13.4, 0, 0, 59, 150, "g", 1.0, null, null, null, new() { "g", "piece" }, new() { DietaryTag.Halal, DietaryTag.HighProtein, DietaryTag.Keto }, "Contains Fish", 2.2m),
            IngredientDocument.Create("Broccoli (Fresh)", FoodCategory.Vegetables, 34, 2.8, 6.6, 0.4, 2.6, 1.7, 33, 100, "g", 1.0, null, null, 91, new() { "g", "cup" }, new() { DietaryTag.Vegan, DietaryTag.Halal, DietaryTag.LowCarb }, null, 0.3m),
            IngredientDocument.Create("Banana (Medium)", FoodCategory.Fruits, 89, 1.1, 22.8, 0.3, 2.6, 12.2, 1, 118, "piece", 1.0, 118, null, null, new() { "piece", "g" }, new() { DietaryTag.Vegan, DietaryTag.Halal }, null, 0.2m),
            IngredientDocument.Create("Almonds (Raw)", FoodCategory.NutsAndSeeds, 579, 21.2, 21.6, 49.9, 12.5, 4.4, 1, 30, "g", 1.0, null, null, 143, new() { "g", "cup" }, new() { DietaryTag.Vegan, DietaryTag.Halal, DietaryTag.Keto }, "Contains Tree Nuts", 1.8m),
            IngredientDocument.Create("Sweet Potato (Raw)", FoodCategory.Vegetables, 86, 1.6, 20.1, 0.1, 3.0, 4.2, 55, 150, "g", 1.0, 150, null, null, new() { "g", "piece" }, new() { DietaryTag.Vegan, DietaryTag.Halal, DietaryTag.GlutenFree }, null, 0.3m)
        };

        foreach (var seed in seeds)
        {
            await _uow.Ingredients.AddAsync(seed, ct);
        }

        return Result.Success();
    }
}

// ======================== UNIT INTERFACE ========================

public interface IFoodCommandUnit
{
    CreateIngredientCommandHandler CreateIngredient { get; }
    UpdateIngredientCommandHandler UpdateIngredient { get; }
    DeleteIngredientCommandHandler DeleteIngredient { get; }

    CreateRecipeCommandHandler CreateRecipe { get; }
    UpdateRecipeCommandHandler UpdateRecipe { get; }
    DeleteRecipeCommandHandler DeleteRecipe { get; }
    ToggleRecipeFavoriteCommandHandler ToggleRecipeFavorite { get; }

    LogMealItemCommandHandler LogMealItem { get; }
    RemoveMealItemCommandHandler RemoveMealItem { get; }
    LogWaterCommandHandler LogWater { get; }

    UpdateNutritionTargetCommandHandler UpdateNutritionTarget { get; }
    UpsertPantryItemCommandHandler UpsertPantryItem { get; }
    DeletePantryItemCommandHandler DeletePantryItem { get; }

    UpsertShoppingListItemCommandHandler UpsertShoppingListItem { get; }
    ToggleShoppingItemPurchasedCommandHandler ToggleShoppingItemPurchased { get; }
    DeleteShoppingListItemCommandHandler DeleteShoppingListItem { get; }
    AddRecipeToShoppingListCommandHandler AddRecipeToShoppingList { get; }

    UpsertWeeklyMealPlanSlotCommandHandler UpsertWeeklyMealPlanSlot { get; }
    DeleteWeeklyMealPlanSlotCommandHandler DeleteWeeklyMealPlanSlot { get; }
    SeedDefaultIngredientsCommandHandler SeedDefaultIngredients { get; }
}

public class FoodCommandUnit : IFoodCommandUnit
{
    public CreateIngredientCommandHandler CreateIngredient { get; }
    public UpdateIngredientCommandHandler UpdateIngredient { get; }
    public DeleteIngredientCommandHandler DeleteIngredient { get; }

    public CreateRecipeCommandHandler CreateRecipe { get; }
    public UpdateRecipeCommandHandler UpdateRecipe { get; }
    public DeleteRecipeCommandHandler DeleteRecipe { get; }
    public ToggleRecipeFavoriteCommandHandler ToggleRecipeFavorite { get; }

    public LogMealItemCommandHandler LogMealItem { get; }
    public RemoveMealItemCommandHandler RemoveMealItem { get; }
    public LogWaterCommandHandler LogWater { get; }

    public UpdateNutritionTargetCommandHandler UpdateNutritionTarget { get; }
    public UpsertPantryItemCommandHandler UpsertPantryItem { get; }
    public DeletePantryItemCommandHandler DeletePantryItem { get; }

    public UpsertShoppingListItemCommandHandler UpsertShoppingListItem { get; }
    public ToggleShoppingItemPurchasedCommandHandler ToggleShoppingItemPurchased { get; }
    public DeleteShoppingListItemCommandHandler DeleteShoppingListItem { get; }
    public AddRecipeToShoppingListCommandHandler AddRecipeToShoppingList { get; }

    public UpsertWeeklyMealPlanSlotCommandHandler UpsertWeeklyMealPlanSlot { get; }
    public DeleteWeeklyMealPlanSlotCommandHandler DeleteWeeklyMealPlanSlot { get; }
    public SeedDefaultIngredientsCommandHandler SeedDefaultIngredients { get; }

    public FoodCommandUnit(
        CreateIngredientCommandHandler createIngredient,
        UpdateIngredientCommandHandler updateIngredient,
        DeleteIngredientCommandHandler deleteIngredient,
        CreateRecipeCommandHandler createRecipe,
        UpdateRecipeCommandHandler updateRecipe,
        DeleteRecipeCommandHandler deleteRecipe,
        ToggleRecipeFavoriteCommandHandler toggleRecipeFavorite,
        LogMealItemCommandHandler logMealItem,
        RemoveMealItemCommandHandler removeMealItem,
        LogWaterCommandHandler logWater,
        UpdateNutritionTargetCommandHandler updateNutritionTarget,
        UpsertPantryItemCommandHandler upsertPantryItem,
        DeletePantryItemCommandHandler deletePantryItem,
        UpsertShoppingListItemCommandHandler upsertShoppingListItem,
        ToggleShoppingItemPurchasedCommandHandler toggleShoppingItemPurchased,
        DeleteShoppingListItemCommandHandler deleteShoppingListItem,
        AddRecipeToShoppingListCommandHandler addRecipeToShoppingList,
        UpsertWeeklyMealPlanSlotCommandHandler upsertWeeklyMealPlanSlot,
        DeleteWeeklyMealPlanSlotCommandHandler deleteWeeklyMealPlanSlot,
        SeedDefaultIngredientsCommandHandler seedDefaultIngredients)
    {
        CreateIngredient = createIngredient;
        UpdateIngredient = updateIngredient;
        DeleteIngredient = deleteIngredient;
        CreateRecipe = createRecipe;
        UpdateRecipe = updateRecipe;
        DeleteRecipe = deleteRecipe;
        ToggleRecipeFavorite = toggleRecipeFavorite;
        LogMealItem = logMealItem;
        RemoveMealItem = removeMealItem;
        LogWater = logWater;
        UpdateNutritionTarget = updateNutritionTarget;
        UpsertPantryItem = upsertPantryItem;
        DeletePantryItem = deletePantryItem;
        UpsertShoppingListItem = upsertShoppingListItem;
        ToggleShoppingItemPurchased = toggleShoppingItemPurchased;
        DeleteShoppingListItem = deleteShoppingListItem;
        AddRecipeToShoppingList = addRecipeToShoppingList;
        UpsertWeeklyMealPlanSlot = upsertWeeklyMealPlanSlot;
        DeleteWeeklyMealPlanSlot = deleteWeeklyMealPlanSlot;
        SeedDefaultIngredients = seedDefaultIngredients;
    }
}
