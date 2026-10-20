using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Food;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Contracts.DTOs.Common;
using AzureTest.Contracts.DTOs.Food;
using AzureTest.Domain.Documents.Food.Enums;
using AzureTest.MVC.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.MVC.Controllers;

[Authorize]
[DisplayName("Food & Cooking Management")]
[Description("Manage ingredients, recipes, meal logging, chef tools, and athlete nutrition.")]
public class FoodController : Controller
{
    private readonly IFoodQueryUnit _queryUnit;
    private readonly IFoodCommandUnit _commandUnit;
    private readonly ICurrentUser _currentUser;

    public FoodController(
        IFoodQueryUnit queryUnit,
        IFoodCommandUnit commandUnit,
        ICurrentUser currentUser)
    {
        _queryUnit = queryUnit;
        _commandUnit = commandUnit;
        _currentUser = currentUser;
    }

    // ======================== DASHBOARD / DAILY DIARY ========================

    [HttpGet]
    [DisplayName("Daily Nutrition Diary")]
    [Description("View daily calorie and macro intake, meal logs, and hydration tracking.")]
    public async Task<IActionResult> Index([FromQuery] DateTime? date, CancellationToken ct = default)
    {
        // Auto seed default standard ingredients if database is empty
        await _commandUnit.SeedDefaultIngredients.HandleAsync(new SeedDefaultIngredientsCommand(), ct);

        var targetDate = date?.Date ?? DateTime.UtcNow.Date;
        var summaryResult = await _queryUnit.GetDailyNutritionSummary.HandleAsync(new GetDailyNutritionSummaryQuery(_currentUser.Id, targetDate), ct);
        var targetResult = await _queryUnit.GetNutritionTarget.HandleAsync(new GetNutritionTargetQuery(_currentUser.Id), ct);

        // Fetch quick ingredients for quick add modal
        var allIngredientsResult = await _queryUnit.GetAllIngredients.HandleAsync(new GetAllIngredientsQuery(new IngredientFilter { PageSize = -1 }), ct);
        var allRecipesResult = await _queryUnit.GetAllRecipes.HandleAsync(new GetAllRecipesQuery(new RecipeFilter { PageSize = -1, UserId = _currentUser.Id }), ct);

        ViewBag.TargetDate = targetDate;
        ViewBag.Targets = targetResult.IsSuccess ? targetResult.Value : null;
        ViewBag.IngredientsList = allIngredientsResult.IsSuccess ? allIngredientsResult.Value.Items : new List<IngredientDto>();
        ViewBag.RecipesList = allRecipesResult.IsSuccess ? allRecipesResult.Value.Items : new List<RecipeDto>();

        return View(summaryResult.Value);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Log Meal Item")]
    [Description("Log an ingredient or recipe to a meal slot.")]
    public async Task<IActionResult> LogItem([FromForm] LogMealItemDto dto, CancellationToken ct = default)
    {
        var result = await _commandUnit.LogMealItem.HandleAsync(new LogMealItemCommand(_currentUser.Id, dto), ct);
        if (result.IsFailure)
        {
            TempData["ErrorMessage"] = result.Message ?? "Failed to log item.";
        }
        else
        {
            TempData["SuccessMessage"] = "Food item logged successfully.";
        }

        return RedirectToAction(nameof(Index), new { date = dto.Date.ToString("yyyy-MM-dd") });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Remove Meal Item")]
    [Description("Remove an item from a meal log.")]
    public async Task<IActionResult> RemoveLogItem(Guid mealLogId, Guid itemId, DateTime date, CancellationToken ct = default)
    {
        var result = await _commandUnit.RemoveMealItem.HandleAsync(new RemoveMealItemCommand(_currentUser.Id, mealLogId, itemId), ct);
        if (result.IsFailure)
        {
            TempData["ErrorMessage"] = result.Message ?? "Failed to remove item.";
        }

        return RedirectToAction(nameof(Index), new { date = date.ToString("yyyy-MM-dd") });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Log Water")]
    [Description("Log daily water and hydration intake.")]
    public async Task<IActionResult> LogWater([FromForm] LogWaterDto dto, CancellationToken ct = default)
    {
        var result = await _commandUnit.LogWater.HandleAsync(new LogWaterCommand(_currentUser.Id, dto), ct);
        if (result.IsFailure)
        {
            TempData["ErrorMessage"] = result.Message ?? "Failed to log water.";
        }

        return RedirectToAction(nameof(Index), new { date = dto.Date.ToString("yyyy-MM-dd") });
    }

    // ======================== RECIPES ========================

    [HttpGet]
    [DisplayName("Recipes Catalog")]
    [Description("Browse and search cooking recipes with dietary filters.")]
    public async Task<IActionResult> Recipes([FromQuery] RecipeFilter? filter, CancellationToken ct = default)
    {
        filter ??= new RecipeFilter();
        var result = await _queryUnit.GetAllRecipes.HandleAsync(new GetAllRecipesQuery(filter), ct);

        ViewBag.Filter = filter;
        return View(result.Value);
    }

    [HttpGet]
    [DisplayName("Recipe Details")]
    [Description("View recipe instructions, timers, scalable servings, and raw vs cooked yield.")]
    public async Task<IActionResult> RecipeDetails(Guid id, [FromQuery] double? servings, CancellationToken ct = default)
    {
        var recipeResult = await _queryUnit.GetRecipeById.HandleAsync(new GetRecipeByIdQuery(id), ct);
        if (recipeResult.IsFailure) return NotFound();

        var recipe = recipeResult.Value;
        double targetServings = (servings.HasValue && servings.Value > 0) ? servings.Value : recipe.Servings;

        var scaleResult = await _queryUnit.GetChefScaling.HandleAsync(new GetChefScalingQuery(id, targetServings), ct);
        ViewBag.Scaling = scaleResult.IsSuccess ? scaleResult.Value : null;
        ViewBag.SelectedServings = targetServings;

        return View(recipe);
    }

    [HttpGet]
    [DisplayName("Create Recipe Page")]
    [Description("Interface to design and calculate a new cooking recipe.")]
    public async Task<IActionResult> CreateRecipe(CancellationToken ct = default)
    {
        var ingredients = await _queryUnit.GetAllIngredients.HandleAsync(new GetAllIngredientsQuery(new IngredientFilter { PageSize = -1 }), ct);
        ViewBag.AvailableIngredients = ingredients.IsSuccess ? ingredients.Value.Items : new List<IngredientDto>();

        return View(new CreateRecipeDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Save New Recipe")]
    [Description("Save a new recipe with ingredients and cooking steps.")]
    public async Task<IActionResult> CreateRecipe([FromForm] CreateRecipeDto dto, CancellationToken ct = default)
    {
        var result = await _commandUnit.CreateRecipe.HandleAsync(new CreateRecipeCommand(_currentUser.Id, dto), ct);
        if (result.IsSuccess)
        {
            TempData["SuccessMessage"] = "Recipe created successfully.";
            return RedirectToAction(nameof(RecipeDetails), new { id = result.Value });
        }

        TempData["ErrorMessage"] = result.Message ?? "Failed to create recipe.";
        var ingredients = await _queryUnit.GetAllIngredients.HandleAsync(new GetAllIngredientsQuery(new IngredientFilter { PageSize = -1 }), ct);
        ViewBag.AvailableIngredients = ingredients.IsSuccess ? ingredients.Value.Items : new List<IngredientDto>();

        return View(dto);
    }

    [HttpGet]
    [DisplayName("Edit Recipe Page")]
    [Description("Modify an existing recipe's ingredients, steps, and yield factor.")]
    public async Task<IActionResult> EditRecipe(Guid id, CancellationToken ct = default)
    {
        var recipeResult = await _queryUnit.GetRecipeById.HandleAsync(new GetRecipeByIdQuery(id), ct);
        if (recipeResult.IsFailure) return NotFound();

        var recipe = recipeResult.Value;
        var dto = new UpdateRecipeDto
        {
            Id = recipe.Id,
            Name = recipe.Name,
            Description = recipe.Description,
            Category = recipe.Category,
            Servings = recipe.Servings,
            PrepTimeMinutes = recipe.PrepTimeMinutes,
            CookTimeMinutes = recipe.CookTimeMinutes,
            Difficulty = recipe.Difficulty,
            Tags = recipe.Tags,
            DietaryTags = recipe.DietaryTags,
            CookingYieldFactor = recipe.CookingYieldFactor,
            ImageUrl = recipe.ImageUrl,
            EstimatedCost = recipe.EstimatedCost,
            Ingredients = recipe.Ingredients.Select(i => new CreateRecipeIngredientDto
            {
                IngredientId = i.IngredientId,
                Quantity = i.Quantity,
                Unit = i.Unit,
                Notes = i.Notes,
                SubstitutionNote = i.SubstitutionNote
            }).ToList(),
            Steps = recipe.Steps.Select(s => new CreateRecipeStepDto
            {
                StepNumber = s.StepNumber,
                Instruction = s.Instruction,
                TimerMinutes = s.TimerMinutes,
                ChefTip = s.ChefTip
            }).ToList()
        };

        var ingredients = await _queryUnit.GetAllIngredients.HandleAsync(new GetAllIngredientsQuery(new IngredientFilter { PageSize = -1 }), ct);
        ViewBag.AvailableIngredients = ingredients.IsSuccess ? ingredients.Value.Items : new List<IngredientDto>();

        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Update Recipe")]
    [Description("Save changes to an existing recipe.")]
    public async Task<IActionResult> EditRecipe([FromForm] UpdateRecipeDto dto, CancellationToken ct = default)
    {
        var result = await _commandUnit.UpdateRecipe.HandleAsync(new UpdateRecipeCommand(_currentUser.Id, dto), ct);
        if (result.IsSuccess)
        {
            TempData["SuccessMessage"] = "Recipe updated successfully.";
            return RedirectToAction(nameof(RecipeDetails), new { id = dto.Id });
        }

        TempData["ErrorMessage"] = result.Message ?? "Failed to update recipe.";
        var ingredients = await _queryUnit.GetAllIngredients.HandleAsync(new GetAllIngredientsQuery(new IngredientFilter { PageSize = -1 }), ct);
        ViewBag.AvailableIngredients = ingredients.IsSuccess ? ingredients.Value.Items : new List<IngredientDto>();

        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Delete Recipe")]
    [Description("Remove a recipe permanently.")]
    public async Task<IActionResult> DeleteRecipe(Guid id, CancellationToken ct = default)
    {
        var result = await _commandUnit.DeleteRecipe.HandleAsync(new DeleteRecipeCommand(_currentUser.Id, id), ct);
        if (result.IsFailure)
        {
            TempData["ErrorMessage"] = result.Message ?? "Failed to delete recipe.";
        }
        else
        {
            TempData["SuccessMessage"] = "Recipe deleted successfully.";
        }

        return RedirectToAction(nameof(Recipes));
    }

    [HttpPost]
    [DisplayName("Toggle Favorite Recipe")]
    [Description("Mark or unmark a recipe as favorite.")]
    public async Task<IActionResult> ToggleFavorite(Guid id, CancellationToken ct = default)
    {
        var result = await _commandUnit.ToggleRecipeFavorite.HandleAsync(new ToggleRecipeFavoriteCommand(_currentUser.Id, id), ct);
        return Json(new { success = result.IsSuccess, isFavorite = result.Value });
    }

    // ======================== INGREDIENTS ========================

    [HttpGet]
    [DisplayName("Ingredients Catalog")]
    [Description("View and filter the ingredients database with standard 100g nutrition facts.")]
    public async Task<IActionResult> Ingredients([FromQuery] IngredientFilter? filter, CancellationToken ct = default)
    {
        filter ??= new IngredientFilter();
        var result = await _queryUnit.GetAllIngredients.HandleAsync(new GetAllIngredientsQuery(filter), ct);

        ViewBag.Filter = filter;
        return View(result.Value);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Create Ingredient")]
    [Description("Add a new raw ingredient with nutritional specifications.")]
    public async Task<IActionResult> CreateIngredient([FromForm] CreateIngredientDto dto, CancellationToken ct = default)
    {
        var result = await _commandUnit.CreateIngredient.HandleAsync(new CreateIngredientCommand(_currentUser.Id, dto), ct);
        if (result.IsSuccess)
        {
            TempData["SuccessMessage"] = $"Ingredient '{dto.Name}' added successfully.";
        }
        else
        {
            TempData["ErrorMessage"] = result.Message ?? "Failed to add ingredient.";
        }

        return RedirectToAction(nameof(Ingredients));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Update Ingredient")]
    [Description("Modify nutritional data or conversion parameters of an ingredient.")]
    public async Task<IActionResult> UpdateIngredient([FromForm] UpdateIngredientDto dto, CancellationToken ct = default)
    {
        var result = await _commandUnit.UpdateIngredient.HandleAsync(new UpdateIngredientCommand(_currentUser.Id, dto), ct);
        if (result.IsSuccess)
        {
            TempData["SuccessMessage"] = $"Ingredient '{dto.Name}' updated successfully.";
        }
        else
        {
            TempData["ErrorMessage"] = result.Message ?? "Failed to update ingredient.";
        }

        return RedirectToAction(nameof(Ingredients));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Delete Ingredient")]
    [Description("Delete an ingredient from the library.")]
    public async Task<IActionResult> DeleteIngredient(Guid id, CancellationToken ct = default)
    {
        var result = await _commandUnit.DeleteIngredient.HandleAsync(new DeleteIngredientCommand(_currentUser.Id, id), ct);
        if (result.IsFailure)
        {
            TempData["ErrorMessage"] = result.Message ?? "Failed to delete ingredient.";
        }
        else
        {
            TempData["SuccessMessage"] = "Ingredient deleted successfully.";
        }

        return RedirectToAction(nameof(Ingredients));
    }

    // ======================== PERSPECTIVE: CHEF ========================

    [HttpGet]
    [DisplayName("Chef Workspace")]
    [Description("Pantry inventory, leftover recipe matcher, unit conversion, and smart shopping list.")]
    public async Task<IActionResult> Chef(CancellationToken ct = default)
    {
        var targetResult = await _queryUnit.GetNutritionTarget.HandleAsync(new GetNutritionTargetQuery(_currentUser.Id), ct);
        var pantryMatches = await _queryUnit.GetPantryMatchRecipes.HandleAsync(new GetPantryMatchRecipesQuery(_currentUser.Id), ct);
        var ingredients = await _queryUnit.GetAllIngredients.HandleAsync(new GetAllIngredientsQuery(new IngredientFilter { PageSize = -1 }), ct);

        ViewBag.PantryMatches = pantryMatches.IsSuccess ? pantryMatches.Value : new List<RecipeDto>();
        ViewBag.AvailableIngredients = ingredients.IsSuccess ? ingredients.Value.Items : new List<IngredientDto>();

        return View(targetResult.Value);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Save Pantry Item")]
    [Description("Add or update an item in the home pantry / fridge inventory.")]
    public async Task<IActionResult> UpsertPantryItem([FromForm] UpsertPantryItemDto dto, CancellationToken ct = default)
    {
        var result = await _commandUnit.UpsertPantryItem.HandleAsync(new UpsertPantryItemCommand(_currentUser.Id, dto), ct);
        if (result.IsFailure) TempData["ErrorMessage"] = result.Message ?? "Failed to save pantry item.";
        return RedirectToAction(nameof(Chef));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Delete Pantry Item")]
    [Description("Remove an item from pantry.")]
    public async Task<IActionResult> DeletePantryItem(Guid itemId, CancellationToken ct = default)
    {
        await _commandUnit.DeletePantryItem.HandleAsync(new DeletePantryItemCommand(_currentUser.Id, itemId), ct);
        return RedirectToAction(nameof(Chef));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Save Shopping Item")]
    [Description("Add or update an item on the shopping list.")]
    public async Task<IActionResult> UpsertShoppingItem([FromForm] UpsertShoppingListItemDto dto, CancellationToken ct = default)
    {
        var result = await _commandUnit.UpsertShoppingListItem.HandleAsync(new UpsertShoppingListItemCommand(_currentUser.Id, dto), ct);
        if (result.IsFailure) TempData["ErrorMessage"] = result.Message ?? "Failed to save shopping item.";
        return RedirectToAction(nameof(Chef));
    }

    [HttpPost]
    [DisplayName("Toggle Shopping Item")]
    [Description("Check or uncheck an item on the shopping list.")]
    public async Task<IActionResult> ToggleShoppingItem(Guid itemId, CancellationToken ct = default)
    {
        var result = await _commandUnit.ToggleShoppingItemPurchased.HandleAsync(new ToggleShoppingItemPurchasedCommand(_currentUser.Id, itemId), ct);
        return Json(new { success = result.IsSuccess });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Delete Shopping Item")]
    [Description("Delete an item from shopping list.")]
    public async Task<IActionResult> DeleteShoppingItem(Guid itemId, CancellationToken ct = default)
    {
        await _commandUnit.DeleteShoppingListItem.HandleAsync(new DeleteShoppingListItemCommand(_currentUser.Id, itemId), ct);
        return RedirectToAction(nameof(Chef));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Add Recipe to Shopping List")]
    [Description("Automatically add missing ingredients of a recipe to shopping list.")]
    public async Task<IActionResult> AddRecipeToShoppingList(Guid recipeId, string? returnUrl = null, CancellationToken ct = default)
    {
        var result = await _commandUnit.AddRecipeToShoppingList.HandleAsync(new AddRecipeToShoppingListCommand(_currentUser.Id, recipeId), ct);
        if (result.IsSuccess)
        {
            TempData["SuccessMessage"] = "Ingredients added to shopping list.";
        }
        else
        {
            TempData["ErrorMessage"] = result.Message ?? "Failed to add ingredients.";
        }

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction(nameof(Chef));
    }

    // ======================== PERSPECTIVE: ATHLETE ========================

    [HttpGet]
    [DisplayName("Athlete Nutrition Hub")]
    [Description("Cut/Bulk/Maintain targets, protein/kg tracking synced with weight logs, and carb cycling.")]
    public async Task<IActionResult> Athlete(CancellationToken ct = default)
    {
        var result = await _queryUnit.GetAthleteProfile.HandleAsync(new GetAthleteProfileQuery(_currentUser.Id), ct);
        return View(result.Value);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Update Nutrition Targets")]
    [Description("Configure daily calories, macro ratios, goal type, and carb cycling.")]
    public async Task<IActionResult> UpdateTargets([FromForm] UpdateNutritionTargetDto dto, string? returnUrl = null, CancellationToken ct = default)
    {
        var result = await _commandUnit.UpdateNutritionTarget.HandleAsync(new UpdateNutritionTargetCommand(_currentUser.Id, dto), ct);
        if (result.IsSuccess)
        {
            TempData["SuccessMessage"] = "Nutrition targets updated successfully.";
        }
        else
        {
            TempData["ErrorMessage"] = result.Message ?? "Failed to update targets.";
        }

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction(nameof(Athlete));
    }

    // ======================== PERSPECTIVE: NORMAL PERSON (PLANNER) ========================

    [HttpGet]
    [DisplayName("Weekly Meal Planner")]
    [Description("Organize weekly meals from Monday to Sunday.")]
    public async Task<IActionResult> Planner(CancellationToken ct = default)
    {
        var targetResult = await _queryUnit.GetNutritionTarget.HandleAsync(new GetNutritionTargetQuery(_currentUser.Id), ct);
        var recipesResult = await _queryUnit.GetAllRecipes.HandleAsync(new GetAllRecipesQuery(new RecipeFilter { PageSize = -1, UserId = _currentUser.Id }), ct);

        ViewBag.Recipes = recipesResult.IsSuccess ? recipesResult.Value.Items : new List<RecipeDto>();
        return View(targetResult.Value);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Save Meal Plan Slot")]
    [Description("Assign a recipe to a day and meal time in the weekly planner.")]
    public async Task<IActionResult> UpsertPlanSlot([FromForm] UpsertWeeklyMealPlanSlotDto dto, CancellationToken ct = default)
    {
        var result = await _commandUnit.UpsertWeeklyMealPlanSlot.HandleAsync(new UpsertWeeklyMealPlanSlotCommand(_currentUser.Id, dto), ct);
        if (result.IsFailure) TempData["ErrorMessage"] = result.Message ?? "Failed to save slot.";
        return RedirectToAction(nameof(Planner));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Delete Meal Plan Slot")]
    [Description("Clear an assigned recipe slot.")]
    public async Task<IActionResult> DeletePlanSlot(Guid slotId, CancellationToken ct = default)
    {
        await _commandUnit.DeleteWeeklyMealPlanSlot.HandleAsync(new DeleteWeeklyMealPlanSlotCommand(_currentUser.Id, slotId), ct);
        return RedirectToAction(nameof(Planner));
    }
}
