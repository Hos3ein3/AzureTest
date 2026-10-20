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

// Stock Management Commands
public record CreateStockItemCommand(Guid UserId, CreateStockItemDto Dto) : ICommand<Result<Guid>>;
public record AdjustStockCommand(Guid UserId, AdjustStockDto Dto) : ICommand<Result>;
public record TransferStockCommand(Guid UserId, TransferStockDto Dto) : ICommand<Result>;
public record LogWasteCommand(Guid UserId, LogWasteDto Dto) : ICommand<Result>;
public record UpdateStapleStatusCommand(Guid UserId, UpdateStapleStatusDto Dto) : ICommand<Result>;
public record DeleteStockItemCommand(Guid UserId, Guid StockItemId) : ICommand<Result>;

// Shopping List Commands
public record CreateShoppingItemCommand(Guid UserId, CreateShoppingItemDto Dto, string? UserName = null) : ICommand<Result<Guid>>;
public record UpdateShoppingItemCommand(Guid UserId, UpdateShoppingItemDto Dto, string? UserName = null) : ICommand<Result>;
public record SetShoppingItemStatusCommand(Guid UserId, SetShoppingItemStatusDto Dto, string? UserName = null) : ICommand<Result>;
public record DeleteShoppingItemCommand(Guid UserId, Guid Id, string? UserName = null) : ICommand<Result>;
public record AutoGenerateShoppingListCommand(Guid UserId, AutoGenerateShoppingListDto Dto) : ICommand<Result<int>>;
public record FinishShoppingCommand(Guid UserId, FinishShoppingDto Dto) : ICommand<Result<int>>;
public record AddMissingRecipeIngredientsToShoppingListCommand(Guid UserId, Guid RecipeId, double Servings) : ICommand<Result<int>>;

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

        // Auto-deduct from stock if requested
        if (request.Dto.DeductFromStock)
        {
            if (request.Dto.IsRecipe)
            {
                var recipe = await _uow.Recipes.GetByIdAsync(request.Dto.ReferenceId, ct);
                if (recipe != null)
                {
                    double scale = request.Dto.Quantity / Math.Max(1.0, recipe.Servings);
                    foreach (var ing in recipe.Ingredients)
                    {
                        var ingDoc = await _uow.Ingredients.GetByIdAsync(ing.IngredientId, ct);
                        double qty = ing.Quantity * scale;
                        await DeductIngredientFromStockAsync(request.UserId, mealLog.Id, item.ItemId, ing.IngredientId, ing.IngredientName, qty, ing.Unit, ingDoc, ct);
                    }
                }
            }
            else
            {
                var ingDoc = await _uow.Ingredients.GetByIdAsync(request.Dto.ReferenceId, ct);
                await DeductIngredientFromStockAsync(request.UserId, mealLog.Id, item.ItemId, request.Dto.ReferenceId, item.Name, request.Dto.Quantity, request.Dto.Unit, ingDoc, ct);
            }
        }

        return Result.Success(item.ItemId);
    }

    private async Task DeductIngredientFromStockAsync(
        Guid userId,
        Guid mealLogId,
        Guid mealItemId,
        Guid ingredientId,
        string ingredientName,
        double quantity,
        string unit,
        IngredientDocument? ingredientDoc,
        CancellationToken ct)
    {
        double normalizedGrams = ingredientDoc != null ? ingredientDoc.CalculateGrams(quantity, unit) : quantity;

        // FEFO (First-Expired, First-Out)
        var lots = (await _uow.PantryStockItems.FindAsync(x => x.UserId == userId && x.IngredientId == ingredientId && !x.IsDepleted))
            .OrderBy(x => x.ExpiryDate.HasValue ? x.ExpiryDate.Value : DateTime.MaxValue)
            .ThenBy(x => x.CreatedAt)
            .ToList();

        double remainingToDeduct = quantity;
        double remainingGramsToDeduct = normalizedGrams;
        string correlationId = $"MealLog:{mealLogId}:{mealItemId}";

        foreach (var lot in lots)
        {
            if (remainingToDeduct <= 0) break;

            if (lot.CurrentQuantity >= remainingToDeduct)
            {
                double deductedQty = remainingToDeduct;
                double deductedGrams = remainingGramsToDeduct;

                lot.CurrentQuantity = Math.Round(lot.CurrentQuantity - deductedQty, 2);
                lot.CurrentNormalizedGrams = Math.Max(0, Math.Round(lot.CurrentNormalizedGrams - deductedGrams, 2));
                if (lot.CurrentQuantity <= 0 && !lot.IsStaple) lot.IsDepleted = true;
                lot.UpdatedAt = DateTime.UtcNow;

                await _uow.PantryStockItems.UpdateAsync(lot.Id, lot, ct);

                var movement = StockMovementDocument.Create(
                    userId,
                    lot.Id,
                    ingredientId,
                    ingredientName,
                    StockMovementType.MealLogConsumption,
                    -deductedQty,
                    unit,
                    -deductedGrams,
                    correlationId: correlationId,
                    location: lot.Location,
                    notes: "Consumed in meal log item"
                );
                await _uow.StockMovements.AddAsync(movement, ct);

                remainingToDeduct = 0;
                remainingGramsToDeduct = 0;
                break;
            }
            else
            {
                double deductedQty = lot.CurrentQuantity;
                double deductedGrams = lot.CurrentNormalizedGrams;

                lot.CurrentQuantity = 0;
                lot.CurrentNormalizedGrams = 0;
                if (!lot.IsStaple) lot.IsDepleted = true;
                lot.UpdatedAt = DateTime.UtcNow;

                await _uow.PantryStockItems.UpdateAsync(lot.Id, lot, ct);

                var movement = StockMovementDocument.Create(
                    userId,
                    lot.Id,
                    ingredientId,
                    ingredientName,
                    StockMovementType.MealLogConsumption,
                    -deductedQty,
                    unit,
                    -deductedGrams,
                    correlationId: correlationId,
                    location: lot.Location,
                    notes: "Consumed in meal log item (batch depleted)"
                );
                await _uow.StockMovements.AddAsync(movement, ct);

                remainingToDeduct -= deductedQty;
                remainingGramsToDeduct -= deductedGrams;
            }
        }

        // If consumed beyond recorded stock, create negative unstocked balance with flag NeedsRestockReconciliation
        if (remainingToDeduct > 0)
        {
            var existingLot = lots.LastOrDefault() ?? (await _uow.PantryStockItems.FindAsync(x => x.UserId == userId && x.IngredientId == ingredientId)).FirstOrDefault();
            if (existingLot != null)
            {
                existingLot.CurrentQuantity = Math.Round(existingLot.CurrentQuantity - remainingToDeduct, 2);
                existingLot.CurrentNormalizedGrams = Math.Round(existingLot.CurrentNormalizedGrams - remainingGramsToDeduct, 2);
                existingLot.NeedsRestockReconciliation = true;
                existingLot.IsDepleted = false;
                existingLot.UpdatedAt = DateTime.UtcNow;
                await _uow.PantryStockItems.UpdateAsync(existingLot.Id, existingLot, ct);

                var movement = StockMovementDocument.Create(
                    userId,
                    existingLot.Id,
                    ingredientId,
                    ingredientName,
                    StockMovementType.MealLogConsumption,
                    -remainingToDeduct,
                    unit,
                    -remainingGramsToDeduct,
                    correlationId: correlationId,
                    location: existingLot.Location,
                    notes: "Unstocked meal consumption (negative inventory reconciliation)"
                );
                await _uow.StockMovements.AddAsync(movement, ct);
            }
            else
            {
                var unstockedLot = PantryStockItemDocument.Create(
                    userId,
                    ingredientId,
                    ingredientName,
                    StockLocation.Pantry,
                    -remainingToDeduct,
                    unit,
                    -remainingGramsToDeduct
                );
                unstockedLot.NeedsRestockReconciliation = true;
                unstockedLot.IsDepleted = false;
                await _uow.PantryStockItems.AddAsync(unstockedLot, ct);

                var movement = StockMovementDocument.Create(
                    userId,
                    unstockedLot.Id,
                    ingredientId,
                    ingredientName,
                    StockMovementType.MealLogConsumption,
                    -remainingToDeduct,
                    unit,
                    -remainingGramsToDeduct,
                    correlationId: correlationId,
                    location: unstockedLot.Location,
                    notes: "Unstocked meal consumption (auto-created reconciliation lot)"
                );
                await _uow.StockMovements.AddAsync(movement, ct);
            }
        }
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

        // Revert associated stock movements
        string correlationId = $"MealLog:{request.MealLogId}:{request.ItemId}";
        var relatedMovements = await _uow.StockMovements.FindAsync(x => x.CorrelationId == correlationId);
        foreach (var movement in relatedMovements)
        {
            if (movement.QuantityDelta < 0)
            {
                double restoreQty = Math.Abs(movement.QuantityDelta);
                double restoreGrams = Math.Abs(movement.NormalizedGramsDelta);

                if (movement.StockItemId.HasValue)
                {
                    var lot = await _uow.PantryStockItems.GetByIdAsync(movement.StockItemId.Value, ct);
                    if (lot != null)
                    {
                        lot.CurrentQuantity = Math.Round(lot.CurrentQuantity + restoreQty, 2);
                        lot.CurrentNormalizedGrams = Math.Round(lot.CurrentNormalizedGrams + restoreGrams, 2);
                        if (lot.CurrentQuantity > 0) lot.IsDepleted = false;
                        if (lot.CurrentQuantity >= 0) lot.NeedsRestockReconciliation = false;
                        lot.UpdatedAt = DateTime.UtcNow;
                        await _uow.PantryStockItems.UpdateAsync(lot.Id, lot, ct);
                    }
                }

                var reversal = StockMovementDocument.Create(
                    request.UserId,
                    movement.StockItemId,
                    movement.IngredientId,
                    movement.IngredientName,
                    StockMovementType.RestockReturn,
                    restoreQty,
                    movement.Unit,
                    restoreGrams,
                    correlationId: $"Reversal:{request.MealLogId}:{request.ItemId}",
                    location: movement.Location,
                    notes: $"Reversal for removed meal log item {request.ItemId}"
                );
                await _uow.StockMovements.AddAsync(reversal, ct);
            }
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

// ======================== STOCK & SHOPPING COMMAND HANDLERS ========================

public class CreateStockItemCommandHandler : ICommandHandler<CreateStockItemCommand, Result<Guid>>
{
    private readonly INoSqlUnitOfWork _uow;
    public CreateStockItemCommandHandler(INoSqlUnitOfWork uow) => _uow = uow;

    public async Task<Result<Guid>> HandleAsync(CreateStockItemCommand request, CancellationToken ct = default)
    {
        var ing = await _uow.Ingredients.GetByIdAsync(request.Dto.IngredientId, ct);
        if (ing == null) return Result.Failure<Guid>("Ingredient not found.");

        double normalizedGrams = ing.CalculateGrams(request.Dto.Quantity, request.Dto.Unit);

        var doc = PantryStockItemDocument.Create(
            request.UserId,
            ing.Id,
            ing.Name,
            request.Dto.Location,
            request.Dto.Quantity,
            request.Dto.Unit,
            normalizedGrams,
            request.Dto.ExpiryDate,
            request.Dto.UnitPrice,
            request.Dto.MinimumParLevel,
            request.Dto.IsStaple,
            request.Dto.BatchCode
        );

        if (request.Dto.IsStaple)
        {
            doc.StapleStatus = request.Dto.StapleStatus;
        }

        await _uow.PantryStockItems.AddAsync(doc, ct);

        // Record Initial / Purchase Ledger Movement
        var movement = StockMovementDocument.Create(
            request.UserId,
            doc.Id,
            ing.Id,
            ing.Name,
            StockMovementType.Purchase,
            doc.CurrentQuantity,
            doc.Unit,
            doc.CurrentNormalizedGrams,
            cost: request.Dto.UnitPrice * (decimal)request.Dto.Quantity,
            location: doc.Location,
            notes: string.IsNullOrWhiteSpace(request.Dto.Notes) ? "Initial stock entry / purchase" : request.Dto.Notes
        );
        await _uow.StockMovements.AddAsync(movement, ct);

        return Result.Success(doc.Id);
    }
}

public class AdjustStockCommandHandler : ICommandHandler<AdjustStockCommand, Result>
{
    private readonly INoSqlUnitOfWork _uow;
    public AdjustStockCommandHandler(INoSqlUnitOfWork uow) => _uow = uow;

    public async Task<Result> HandleAsync(AdjustStockCommand request, CancellationToken ct = default)
    {
        var lot = await _uow.PantryStockItems.GetByIdAsync(request.Dto.StockItemId, ct);
        if (lot == null || lot.UserId != request.UserId) return Result.Failure("Stock item not found.");

        double oldQty = lot.CurrentQuantity;
        double newQty = Math.Round(request.Dto.NewQuantity, 2);
        double delta = Math.Round(newQty - oldQty, 2);

        var ing = await _uow.Ingredients.GetByIdAsync(lot.IngredientId, ct);
        double deltaGrams = ing != null ? ing.CalculateGrams(delta, lot.Unit) : delta;

        lot.CurrentQuantity = newQty;
        lot.CurrentNormalizedGrams = ing != null ? ing.CalculateGrams(newQty, lot.Unit) : newQty;
        lot.IsDepleted = newQty <= 0 && !lot.IsStaple;
        if (newQty >= 0) lot.NeedsRestockReconciliation = false;
        lot.UpdatedAt = DateTime.UtcNow;

        await _uow.PantryStockItems.UpdateAsync(lot.Id, lot, ct);

        var movement = StockMovementDocument.Create(
            request.UserId,
            lot.Id,
            lot.IngredientId,
            lot.IngredientName,
            StockMovementType.ManualCorrection,
            delta,
            lot.Unit,
            deltaGrams,
            location: lot.Location,
            notes: string.IsNullOrWhiteSpace(request.Dto.Reason) ? "Manual stock adjustment" : request.Dto.Reason
        );
        await _uow.StockMovements.AddAsync(movement, ct);

        return Result.Success();
    }
}

public class TransferStockCommandHandler : ICommandHandler<TransferStockCommand, Result>
{
    private readonly INoSqlUnitOfWork _uow;
    public TransferStockCommandHandler(INoSqlUnitOfWork uow) => _uow = uow;

    public async Task<Result> HandleAsync(TransferStockCommand request, CancellationToken ct = default)
    {
        var lot = await _uow.PantryStockItems.GetByIdAsync(request.Dto.StockItemId, ct);
        if (lot == null || lot.UserId != request.UserId) return Result.Failure("Stock item not found.");

        var oldLoc = lot.Location;
        lot.Location = request.Dto.NewLocation;
        lot.UpdatedAt = DateTime.UtcNow;

        await _uow.PantryStockItems.UpdateAsync(lot.Id, lot, ct);

        var movement = StockMovementDocument.Create(
            request.UserId,
            lot.Id,
            lot.IngredientId,
            lot.IngredientName,
            StockMovementType.Transfer,
            0,
            lot.Unit,
            0,
            location: lot.Location,
            notes: $"Transferred from {oldLoc} to {request.Dto.NewLocation}"
        );
        await _uow.StockMovements.AddAsync(movement, ct);

        return Result.Success();
    }
}

public class LogWasteCommandHandler : ICommandHandler<LogWasteCommand, Result>
{
    private readonly INoSqlUnitOfWork _uow;
    public LogWasteCommandHandler(INoSqlUnitOfWork uow) => _uow = uow;

    public async Task<Result> HandleAsync(LogWasteCommand request, CancellationToken ct = default)
    {
        var lot = await _uow.PantryStockItems.GetByIdAsync(request.Dto.StockItemId, ct);
        if (lot == null || lot.UserId != request.UserId) return Result.Failure("Stock item not found.");

        double wasteQty = Math.Min(lot.CurrentQuantity, Math.Round(request.Dto.Quantity, 2));
        var ing = await _uow.Ingredients.GetByIdAsync(lot.IngredientId, ct);
        double wasteGrams = ing != null ? ing.CalculateGrams(wasteQty, lot.Unit) : wasteQty;

        lot.CurrentQuantity = Math.Round(lot.CurrentQuantity - wasteQty, 2);
        lot.CurrentNormalizedGrams = Math.Max(0, Math.Round(lot.CurrentNormalizedGrams - wasteGrams, 2));
        if (lot.CurrentQuantity <= 0 && !lot.IsStaple) lot.IsDepleted = true;
        lot.UpdatedAt = DateTime.UtcNow;

        await _uow.PantryStockItems.UpdateAsync(lot.Id, lot, ct);

        decimal? cost = request.Dto.EstimatedCost;
        if (!cost.HasValue && lot.UnitPrice.HasValue)
        {
            cost = Math.Round(lot.UnitPrice.Value * (decimal)wasteQty, 2);
        }

        var movement = StockMovementDocument.Create(
            request.UserId,
            lot.Id,
            lot.IngredientId,
            lot.IngredientName,
            StockMovementType.Waste,
            -wasteQty,
            lot.Unit,
            -wasteGrams,
            wasteReason: request.Dto.Reason,
            cost: cost,
            location: lot.Location,
            notes: string.IsNullOrWhiteSpace(request.Dto.Notes) ? $"Food waste logged ({request.Dto.Reason})" : request.Dto.Notes
        );
        await _uow.StockMovements.AddAsync(movement, ct);

        return Result.Success();
    }
}

public class UpdateStapleStatusCommandHandler : ICommandHandler<UpdateStapleStatusCommand, Result>
{
    private readonly INoSqlUnitOfWork _uow;
    public UpdateStapleStatusCommandHandler(INoSqlUnitOfWork uow) => _uow = uow;

    public async Task<Result> HandleAsync(UpdateStapleStatusCommand request, CancellationToken ct = default)
    {
        var lot = await _uow.PantryStockItems.GetByIdAsync(request.Dto.StockItemId, ct);
        if (lot == null || lot.UserId != request.UserId) return Result.Failure("Stock item not found.");

        var oldStatus = lot.StapleStatus;
        lot.IsStaple = true;
        lot.StapleStatus = request.Dto.Status;
        lot.IsDepleted = request.Dto.Status == StapleStatus.OutOfStock;
        lot.UpdatedAt = DateTime.UtcNow;

        await _uow.PantryStockItems.UpdateAsync(lot.Id, lot, ct);

        var movement = StockMovementDocument.Create(
            request.UserId,
            lot.Id,
            lot.IngredientId,
            lot.IngredientName,
            StockMovementType.StapleAdjustment,
            0,
            lot.Unit,
            0,
            location: lot.Location,
            notes: $"Staple status changed from {oldStatus} to {request.Dto.Status}"
        );
        await _uow.StockMovements.AddAsync(movement, ct);

        return Result.Success();
    }
}

public class DeleteStockItemCommandHandler : ICommandHandler<DeleteStockItemCommand, Result>
{
    private readonly INoSqlUnitOfWork _uow;
    public DeleteStockItemCommandHandler(INoSqlUnitOfWork uow) => _uow = uow;

    public async Task<Result> HandleAsync(DeleteStockItemCommand request, CancellationToken ct = default)
    {
        var lot = await _uow.PantryStockItems.GetByIdAsync(request.StockItemId, ct);
        if (lot == null || lot.UserId != request.UserId) return Result.Failure("Stock item not found.");

        await _uow.PantryStockItems.RemoveAsync(lot.Id, ct);
        return Result.Success();
    }
}

public class CreateShoppingItemCommandHandler : ICommandHandler<CreateShoppingItemCommand, Result<Guid>>
{
    private readonly INoSqlUnitOfWork _uow;
    public CreateShoppingItemCommandHandler(INoSqlUnitOfWork uow) => _uow = uow;

    public async Task<Result<Guid>> HandleAsync(CreateShoppingItemCommand request, CancellationToken ct = default)
    {
        double normalizedGrams = 0;
        string name = request.Dto.Name.Trim();

        if (request.Dto.IngredientId.HasValue)
        {
            var ing = await _uow.Ingredients.GetByIdAsync(request.Dto.IngredientId.Value, ct);
            if (ing != null)
            {
                name = ing.Name;
                normalizedGrams = ing.CalculateGrams(request.Dto.Quantity, request.Dto.Unit);
            }
        }

        var doc = ShoppingListItemDocument.Create(
            request.UserId,
            name,
            request.Dto.Quantity,
            request.Dto.Unit,
            request.Dto.StoreSection,
            ShoppingItemSource.Manual,
            request.Dto.IngredientId,
            request.Dto.TargetLocation,
            request.Dto.EstimatedCost,
            request.Dto.Notes,
            normalizedGrams,
            request.Dto.IsShared,
            request.UserName
        );

        await _uow.ShoppingListItems.AddAsync(doc, ct);
        return Result.Success(doc.Id);
    }
}

public class UpdateShoppingItemCommandHandler : ICommandHandler<UpdateShoppingItemCommand, Result>
{
    private readonly INoSqlUnitOfWork _uow;
    public UpdateShoppingItemCommandHandler(INoSqlUnitOfWork uow) => _uow = uow;

    public async Task<Result> HandleAsync(UpdateShoppingItemCommand request, CancellationToken ct = default)
    {
        var item = await _uow.ShoppingListItems.GetByIdAsync(request.Dto.Id, ct);
        if (item == null) return Result.Failure("Shopping item not found.");
        if (!item.IsShared && item.UserId != request.UserId) return Result.Failure("Not authorized to modify this item.");

        item.Name = request.Dto.Name.Trim();
        item.Quantity = Math.Max(0.01, request.Dto.Quantity);
        item.Unit = string.IsNullOrWhiteSpace(request.Dto.Unit) ? "piece" : request.Dto.Unit.Trim();
        item.StoreSection = request.Dto.StoreSection;
        item.TargetLocation = request.Dto.TargetLocation;
        item.EstimatedCost = request.Dto.EstimatedCost;
        item.Notes = request.Dto.Notes?.Trim();
        if (!string.IsNullOrWhiteSpace(request.UserName)) item.LastModifiedByUserName = request.UserName;
        item.UpdatedAt = DateTime.UtcNow;

        await _uow.ShoppingListItems.UpdateAsync(item.Id, item, ct);
        return Result.Success();
    }
}

public class SetShoppingItemStatusCommandHandler : ICommandHandler<SetShoppingItemStatusCommand, Result>
{
    private readonly INoSqlUnitOfWork _uow;
    public SetShoppingItemStatusCommandHandler(INoSqlUnitOfWork uow) => _uow = uow;

    public async Task<Result> HandleAsync(SetShoppingItemStatusCommand request, CancellationToken ct = default)
    {
        var item = await _uow.ShoppingListItems.GetByIdAsync(request.Dto.Id, ct);
        if (item == null) return Result.Failure("Shopping item not found.");
        if (!item.IsShared && item.UserId != request.UserId) return Result.Failure("Not authorized to modify this item.");

        item.Status = request.Dto.Status;
        if (!string.IsNullOrWhiteSpace(request.UserName)) item.LastModifiedByUserName = request.UserName;
        item.UpdatedAt = DateTime.UtcNow;

        await _uow.ShoppingListItems.UpdateAsync(item.Id, item, ct);
        return Result.Success();
    }
}

public class DeleteShoppingItemCommandHandler : ICommandHandler<DeleteShoppingItemCommand, Result>
{
    private readonly INoSqlUnitOfWork _uow;
    public DeleteShoppingItemCommandHandler(INoSqlUnitOfWork uow) => _uow = uow;

    public async Task<Result> HandleAsync(DeleteShoppingItemCommand request, CancellationToken ct = default)
    {
        var item = await _uow.ShoppingListItems.GetByIdAsync(request.Id, ct);
        if (item == null) return Result.Failure("Shopping item not found.");
        if (!item.IsShared && item.UserId != request.UserId) return Result.Failure("Not authorized to delete this item.");

        await _uow.ShoppingListItems.RemoveAsync(item.Id, ct);
        return Result.Success();
    }
}

public class AutoGenerateShoppingListCommandHandler : ICommandHandler<AutoGenerateShoppingListCommand, Result<int>>
{
    private readonly INoSqlUnitOfWork _uow;
    public AutoGenerateShoppingListCommandHandler(INoSqlUnitOfWork uow) => _uow = uow;

    public async Task<Result<int>> HandleAsync(AutoGenerateShoppingListCommand request, CancellationToken ct = default)
    {
        int generatedCount = 0;
        var existingList = (await _uow.ShoppingListItems.FindAsync(x => x.UserId == request.UserId && x.Status != ShoppingItemStatus.Bought)).ToList();
        var activeStock = await _uow.PantryStockItems.FindAsync(x => x.UserId == request.UserId && !x.IsDepleted);

        // 1. Evaluate Low Stock and Out of Stock Staples
        if (request.Dto.IncludeLowStock)
        {
            var lowStockItems = activeStock.Where(x => x.IsBelowParLevel()).ToList();
            // Also check items with par levels that might be completely depleted
            var allStock = await _uow.PantryStockItems.FindAsync(x => x.UserId == request.UserId);
            var depletedParItems = allStock.Where(x => x.IsDepleted && x.MinimumParLevel.HasValue && x.MinimumParLevel.Value > 0).ToList();

            var candidates = lowStockItems.Concat(depletedParItems).DistinctBy(x => x.Id).ToList();

            foreach (var stockItem in candidates)
            {
                double neededQty = 1.0;
                if (stockItem.MinimumParLevel.HasValue && stockItem.MinimumParLevel.Value > stockItem.CurrentQuantity)
                {
                    neededQty = Math.Round(stockItem.MinimumParLevel.Value - Math.Max(0, stockItem.CurrentQuantity), 2);
                }

                // Check if already on shopping list
                var existing = existingList.FirstOrDefault(x =>
                    (x.IngredientId.HasValue && x.IngredientId == stockItem.IngredientId) ||
                    x.Name.Equals(stockItem.IngredientName, StringComparison.OrdinalIgnoreCase));

                if (existing != null)
                {
                    existing.Quantity = Math.Round(existing.Quantity + neededQty, 2);
                    existing.UpdatedAt = DateTime.UtcNow;
                    await _uow.ShoppingListItems.UpdateAsync(existing.Id, existing, ct);
                    generatedCount++;
                }
                else
                {
                    var newItem = ShoppingListItemDocument.Create(
                        request.UserId,
                        stockItem.IngredientName,
                        neededQty,
                        stockItem.Unit,
                        StoreSection.PantryAndDryGoods,
                        ShoppingItemSource.LowStockParLevel,
                        stockItem.IngredientId,
                        stockItem.Location,
                        stockItem.UnitPrice.HasValue ? Math.Round(stockItem.UnitPrice.Value * (decimal)neededQty, 2) : null,
                        "Auto-generated from low stock par level"
                    );
                    await _uow.ShoppingListItems.AddAsync(newItem, ct);
                    existingList.Add(newItem);
                    generatedCount++;
                }
            }
        }

        // 2. Evaluate Meal Plan Requirements
        if (request.Dto.IncludeMealPlan)
        {
            var targetDoc = (await _uow.NutritionTargets.FindAsync(x => x.UserId == request.UserId)).FirstOrDefault();
            if (targetDoc?.WeeklyPlan != null && targetDoc.WeeklyPlan.Any())
            {
                // Aggregate required ingredients from weekly plan
                var planNeeds = new Dictionary<Guid, (string Name, double NeededQty, string Unit)>();

                foreach (var slot in targetDoc.WeeklyPlan)
                {
                    if (!slot.RecipeId.HasValue) continue;
                    var recipe = await _uow.Recipes.GetByIdAsync(slot.RecipeId.Value, ct);
                    if (recipe == null) continue;

                    double scale = (double)slot.Servings / Math.Max(1.0, recipe.Servings);
                    foreach (var ing in recipe.Ingredients)
                    {
                        if (planNeeds.ContainsKey(ing.IngredientId))
                        {
                            var current = planNeeds[ing.IngredientId];
                            planNeeds[ing.IngredientId] = (current.Name, current.NeededQty + (ing.Quantity * scale), current.Unit);
                        }
                        else
                        {
                            planNeeds[ing.IngredientId] = (ing.IngredientName, ing.Quantity * scale, ing.Unit);
                        }
                    }
                }

                // Compare with available stock
                foreach (var kvp in planNeeds)
                {
                    var ingId = kvp.Key;
                    var (ingName, neededTotal, unit) = kvp.Value;

                    double totalStockAvailable = activeStock
                        .Where(s => s.IngredientId == ingId)
                        .Sum(s => s.CurrentQuantity);

                    if (totalStockAvailable < neededTotal)
                    {
                        double deficit = Math.Round(neededTotal - totalStockAvailable, 2);

                        var existing = existingList.FirstOrDefault(x =>
                            (x.IngredientId.HasValue && x.IngredientId == ingId) ||
                            x.Name.Equals(ingName, StringComparison.OrdinalIgnoreCase));

                        if (existing != null)
                        {
                            existing.Quantity = Math.Round(existing.Quantity + deficit, 2);
                            existing.UpdatedAt = DateTime.UtcNow;
                            await _uow.ShoppingListItems.UpdateAsync(existing.Id, existing, ct);
                            generatedCount++;
                        }
                        else
                        {
                            var newItem = ShoppingListItemDocument.Create(
                                request.UserId,
                                ingName,
                                deficit,
                                unit,
                                StoreSection.Produce,
                                ShoppingItemSource.MealPlan,
                                ingId,
                                StockLocation.Fridge,
                                null,
                                "Auto-generated from weekly meal plan"
                            );
                            await _uow.ShoppingListItems.AddAsync(newItem, ct);
                            existingList.Add(newItem);
                            generatedCount++;
                        }
                    }
                }
            }
        }

        return Result.Success(generatedCount);
    }
}

public class FinishShoppingCommandHandler : ICommandHandler<FinishShoppingCommand, Result<int>>
{
    private readonly INoSqlUnitOfWork _uow;
    public FinishShoppingCommandHandler(INoSqlUnitOfWork uow) => _uow = uow;

    public async Task<Result<int>> HandleAsync(FinishShoppingCommand request, CancellationToken ct = default)
    {
        var allItems = await _uow.ShoppingListItems.FindAsync(x => x.UserId == request.UserId || x.IsShared == true);
        var targetItems = allItems.Where(x =>
            x.Status == ShoppingItemStatus.Bought ||
            (request.Dto.IncludeInCartItems && x.Status == ShoppingItemStatus.InCart)
        ).ToList();

        if (!targetItems.Any()) return Result.Success(0);

        string batchCode = string.IsNullOrWhiteSpace(request.Dto.CustomBatchCode)
            ? $"SHOP-{DateTime.UtcNow:yyyyMMdd-HHmm}"
            : request.Dto.CustomBatchCode.Trim();

        int processedCount = 0;

        foreach (var item in targetItems)
        {
            // Find existing lot or create new lot
            PantryStockItemDocument? targetLot = null;

            if (item.IngredientId.HasValue)
            {
                var existingLots = await _uow.PantryStockItems.FindAsync(x =>
                    x.UserId == request.UserId &&
                    x.IngredientId == item.IngredientId.Value &&
                    x.Location == item.TargetLocation &&
                    !x.IsDepleted);

                targetLot = existingLots.FirstOrDefault();
            }

            if (targetLot != null)
            {
                targetLot.CurrentQuantity = Math.Round(targetLot.CurrentQuantity + item.Quantity, 2);
                targetLot.IsDepleted = false;
                targetLot.NeedsRestockReconciliation = false;
                targetLot.UpdatedAt = DateTime.UtcNow;

                await _uow.PantryStockItems.UpdateAsync(targetLot.Id, targetLot, ct);

                var movement = StockMovementDocument.Create(
                    request.UserId,
                    targetLot.Id,
                    targetLot.IngredientId,
                    targetLot.IngredientName,
                    StockMovementType.Purchase,
                    item.Quantity,
                    item.Unit,
                    item.NormalizedGrams,
                    correlationId: $"ShoppingFinish:{batchCode}",
                    cost: item.EstimatedCost,
                    location: targetLot.Location,
                    notes: $"Purchased via Shopping List ({batchCode})"
                );
                await _uow.StockMovements.AddAsync(movement, ct);
            }
            else
            {
                // Create new stock lot
                Guid ingId = item.IngredientId ?? Guid.CreateVersion7();
                var newLot = PantryStockItemDocument.Create(
                    request.UserId,
                    ingId,
                    item.Name,
                    item.TargetLocation,
                    item.Quantity,
                    item.Unit,
                    item.NormalizedGrams,
                    expiryDate: null,
                    unitPrice: item.EstimatedCost.HasValue && item.Quantity > 0 ? item.EstimatedCost.Value / (decimal)item.Quantity : null,
                    batchCode: batchCode
                );

                await _uow.PantryStockItems.AddAsync(newLot, ct);

                var movement = StockMovementDocument.Create(
                    request.UserId,
                    newLot.Id,
                    ingId,
                    item.Name,
                    StockMovementType.Purchase,
                    item.Quantity,
                    item.Unit,
                    item.NormalizedGrams,
                    correlationId: $"ShoppingFinish:{batchCode}",
                    cost: item.EstimatedCost,
                    location: newLot.Location,
                    notes: $"Purchased via Shopping List ({batchCode})"
                );
                await _uow.StockMovements.AddAsync(movement, ct);
            }

            // Remove the bought item from shopping list
            await _uow.ShoppingListItems.RemoveAsync(item.Id, ct);
            processedCount++;
        }

        return Result.Success(processedCount);
    }
}

public class AddMissingRecipeIngredientsToShoppingListCommandHandler : ICommandHandler<AddMissingRecipeIngredientsToShoppingListCommand, Result<int>>
{
    private readonly INoSqlUnitOfWork _uow;
    public AddMissingRecipeIngredientsToShoppingListCommandHandler(INoSqlUnitOfWork uow) => _uow = uow;

    public async Task<Result<int>> HandleAsync(AddMissingRecipeIngredientsToShoppingListCommand request, CancellationToken ct = default)
    {
        var recipe = await _uow.Recipes.GetByIdAsync(request.RecipeId, ct);
        if (recipe == null) return Result.Failure<int>("Recipe not found.");

        double servings = Math.Max(1.0, request.Servings);
        double scale = servings / Math.Max(1.0, recipe.Servings);

        var activeStock = await _uow.PantryStockItems.FindAsync(x => x.UserId == request.UserId && !x.IsDepleted);
        var existingShopping = (await _uow.ShoppingListItems.FindAsync(x => x.UserId == request.UserId && x.Status != ShoppingItemStatus.Bought)).ToList();
        int addedCount = 0;

        foreach (var ing in recipe.Ingredients)
        {
            double neededQty = ing.Quantity * scale;
            double available = activeStock.Where(s => s.IngredientId == ing.IngredientId).Sum(s => s.CurrentQuantity);

            if (available < neededQty)
            {
                double deficit = Math.Round(neededQty - available, 2);

                var existing = existingShopping.FirstOrDefault(x =>
                    (x.IngredientId.HasValue && x.IngredientId == ing.IngredientId) ||
                    x.Name.Equals(ing.IngredientName, StringComparison.OrdinalIgnoreCase));

                if (existing != null)
                {
                    existing.Quantity = Math.Round(existing.Quantity + deficit, 2);
                    existing.UpdatedAt = DateTime.UtcNow;
                    await _uow.ShoppingListItems.UpdateAsync(existing.Id, existing, ct);
                    addedCount++;
                }
                else
                {
                    var newItem = ShoppingListItemDocument.Create(
                        request.UserId,
                        ing.IngredientName,
                        deficit,
                        ing.Unit,
                        StoreSection.Produce,
                        ShoppingItemSource.RecipeMissing,
                        ing.IngredientId,
                        StockLocation.Pantry,
                        null,
                        $"Missing for {recipe.Name} ({servings} servings)"
                    );
                    await _uow.ShoppingListItems.AddAsync(newItem, ct);
                    existingShopping.Add(newItem);
                    addedCount++;
                }
            }
        }

        return Result.Success(addedCount);
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

    // Stock Management
    CreateStockItemCommandHandler CreateStockItem { get; }
    AdjustStockCommandHandler AdjustStock { get; }
    TransferStockCommandHandler TransferStock { get; }
    LogWasteCommandHandler LogWaste { get; }
    UpdateStapleStatusCommandHandler UpdateStapleStatus { get; }
    DeleteStockItemCommandHandler DeleteStockItem { get; }

    // Shopping List Management
    CreateShoppingItemCommandHandler CreateShoppingItem { get; }
    UpdateShoppingItemCommandHandler UpdateShoppingItem { get; }
    SetShoppingItemStatusCommandHandler SetShoppingItemStatus { get; }
    DeleteShoppingItemCommandHandler DeleteShoppingItem { get; }
    AutoGenerateShoppingListCommandHandler AutoGenerateShoppingList { get; }
    FinishShoppingCommandHandler FinishShopping { get; }
    AddMissingRecipeIngredientsToShoppingListCommandHandler AddMissingRecipeIngredientsToShoppingList { get; }
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

    // Stock Management
    public CreateStockItemCommandHandler CreateStockItem { get; }
    public AdjustStockCommandHandler AdjustStock { get; }
    public TransferStockCommandHandler TransferStock { get; }
    public LogWasteCommandHandler LogWaste { get; }
    public UpdateStapleStatusCommandHandler UpdateStapleStatus { get; }
    public DeleteStockItemCommandHandler DeleteStockItem { get; }

    // Shopping List Management
    public CreateShoppingItemCommandHandler CreateShoppingItem { get; }
    public UpdateShoppingItemCommandHandler UpdateShoppingItem { get; }
    public SetShoppingItemStatusCommandHandler SetShoppingItemStatus { get; }
    public DeleteShoppingItemCommandHandler DeleteShoppingItem { get; }
    public AutoGenerateShoppingListCommandHandler AutoGenerateShoppingList { get; }
    public FinishShoppingCommandHandler FinishShopping { get; }
    public AddMissingRecipeIngredientsToShoppingListCommandHandler AddMissingRecipeIngredientsToShoppingList { get; }

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
        SeedDefaultIngredientsCommandHandler seedDefaultIngredients,
        CreateStockItemCommandHandler createStockItem,
        AdjustStockCommandHandler adjustStock,
        TransferStockCommandHandler transferStock,
        LogWasteCommandHandler logWaste,
        UpdateStapleStatusCommandHandler updateStapleStatus,
        DeleteStockItemCommandHandler deleteStockItem,
        CreateShoppingItemCommandHandler createShoppingItem,
        UpdateShoppingItemCommandHandler updateShoppingItem,
        SetShoppingItemStatusCommandHandler setShoppingItemStatus,
        DeleteShoppingItemCommandHandler deleteShoppingItem,
        AutoGenerateShoppingListCommandHandler autoGenerateShoppingList,
        FinishShoppingCommandHandler finishShopping,
        AddMissingRecipeIngredientsToShoppingListCommandHandler addMissingRecipeIngredientsToShoppingList)
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

        CreateStockItem = createStockItem;
        AdjustStock = adjustStock;
        TransferStock = transferStock;
        LogWaste = logWaste;
        UpdateStapleStatus = updateStapleStatus;
        DeleteStockItem = deleteStockItem;

        CreateShoppingItem = createShoppingItem;
        UpdateShoppingItem = updateShoppingItem;
        SetShoppingItemStatus = setShoppingItemStatus;
        DeleteShoppingItem = deleteShoppingItem;
        AutoGenerateShoppingList = autoGenerateShoppingList;
        FinishShopping = finishShopping;
        AddMissingRecipeIngredientsToShoppingList = addMissingRecipeIngredientsToShoppingList;
    }
}
