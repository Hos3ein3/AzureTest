using System;
using System.Collections.Generic;
using AzureTest.Contracts.DTOs.Common;
using AzureTest.Domain.Documents.Food.Enums;

namespace AzureTest.Contracts.DTOs.Food;

public class RecipeFilter : PagedFilterRequest
{
    public string? Category { get; set; }
    public RecipeDifficulty? Difficulty { get; set; }
    public DietaryTag? DietaryTag { get; set; }
    public bool? FavoritesOnly { get; set; }
    public Guid? UserId { get; set; }

    public RecipeFilter()
    {
        SortBy = "CreatedAt";
        SortDescending = true;
        PageSize = 12;
    }
}

public record RecipeIngredientDto(
    Guid IngredientId,
    string IngredientName,
    double Quantity,
    string Unit,
    string? Notes,
    string? SubstitutionNote,
    double ComputedGrams,
    double Calories,
    double Protein,
    double Carbs,
    double Fat,
    double Fiber,
    double Sugar,
    double Sodium
);

public record RecipeStepDto(
    int StepNumber,
    string Instruction,
    int? TimerMinutes,
    string? ChefTip
);

public record RecipeDto(
    Guid Id,
    Guid UserId,
    string Name,
    string? Description,
    string Category,
    int Servings,
    int PrepTimeMinutes,
    int CookTimeMinutes,
    int TotalTimeMinutes,
    RecipeDifficulty Difficulty,
    List<string> Tags,
    List<DietaryTag> DietaryTags,
    List<RecipeIngredientDto> Ingredients,
    List<RecipeStepDto> Steps,
    double CookingYieldFactor,
    double RawWeightGrams,
    double CookedWeightGrams,
    double TotalCalories,
    double TotalProtein,
    double TotalCarbs,
    double TotalFat,
    double TotalFiber,
    double TotalSugar,
    double TotalSodium,
    double PerServingCalories,
    double PerServingProtein,
    double PerServingCarbs,
    double PerServingFat,
    double PerServingFiber,
    double PerServingCookedWeightGrams,
    double Per100gCookedCalories,
    double Per100gCookedProtein,
    double Per100gCookedCarbs,
    double Per100gCookedFat,
    bool IsFavorite,
    string? ImageUrl,
    decimal? EstimatedCost,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public class CreateRecipeIngredientDto
{
    public Guid IngredientId { get; set; }
    public double Quantity { get; set; }
    public string Unit { get; set; } = "g";
    public string? Notes { get; set; }
    public string? SubstitutionNote { get; set; }
}

public class CreateRecipeStepDto
{
    public int StepNumber { get; set; }
    public string Instruction { get; set; } = string.Empty;
    public int? TimerMinutes { get; set; }
    public string? ChefTip { get; set; }
}

public class CreateRecipeDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = "Main Dish";
    public int Servings { get; set; } = 1;
    public int PrepTimeMinutes { get; set; }
    public int CookTimeMinutes { get; set; }
    public RecipeDifficulty Difficulty { get; set; } = RecipeDifficulty.Easy;
    public List<string> Tags { get; set; } = new();
    public List<DietaryTag> DietaryTags { get; set; } = new();
    public double CookingYieldFactor { get; set; } = 1.0;
    public string? ImageUrl { get; set; }
    public decimal? EstimatedCost { get; set; }
    public List<CreateRecipeIngredientDto> Ingredients { get; set; } = new();
    public List<CreateRecipeStepDto> Steps { get; set; } = new();
}

public class UpdateRecipeDto : CreateRecipeDto
{
    public Guid Id { get; set; }
}
