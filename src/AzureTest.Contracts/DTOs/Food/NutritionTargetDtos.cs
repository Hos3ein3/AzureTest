using System;
using System.Collections.Generic;
using AzureTest.Domain.Documents.Food.Enums;

namespace AzureTest.Contracts.DTOs.Food;

public record PantryItemDto(
    Guid Id,
    Guid? IngredientId,
    string Name,
    double Quantity,
    string Unit,
    DateTime? ExpiryDate,
    string Location
);

public class UpsertPantryItemDto
{
    public Guid? Id { get; set; }
    public Guid? IngredientId { get; set; }
    public string Name { get; set; } = string.Empty;
    public double Quantity { get; set; }
    public string Unit { get; set; } = "g";
    public DateTime? ExpiryDate { get; set; }
    public string Location { get; set; } = "Pantry";
}

public record ShoppingListItemDto(
    Guid Id,
    Guid? IngredientId,
    string Name,
    double Quantity,
    string Unit,
    bool IsPurchased,
    string Category,
    string? Notes
);

public class UpsertShoppingListItemDto
{
    public Guid? Id { get; set; }
    public Guid? IngredientId { get; set; }
    public string Name { get; set; } = string.Empty;
    public double Quantity { get; set; }
    public string Unit { get; set; } = "g";
    public bool IsPurchased { get; set; }
    public string Category { get; set; } = "Produce";
    public string? Notes { get; set; }
}

public record WeeklyMealPlanSlotDto(
    Guid Id,
    DayOfWeek DayOfWeek,
    MealType MealType,
    Guid? RecipeId,
    string? RecipeName,
    int Servings,
    string? Notes
);

public class UpsertWeeklyMealPlanSlotDto
{
    public Guid? Id { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public MealType MealType { get; set; }
    public Guid? RecipeId { get; set; }
    public string? RecipeName { get; set; }
    public int Servings { get; set; } = 1;
    public string? Notes { get; set; }
}

public record NutritionTargetDto(
    Guid Id,
    Guid UserId,
    int CalorieTarget,
    int ProteinGramsTarget,
    int CarbsGramsTarget,
    int FatGramsTarget,
    int WaterMlTarget,
    FitnessGoalType GoalType,
    double TargetProteinPerKg,
    bool EnableCarbCycling,
    int TrainingDayCalories,
    int TrainingDayCarbs,
    int RestDayCalories,
    int RestDayCarbs,
    List<PantryItemDto> Pantry,
    List<ShoppingListItemDto> ShoppingList,
    List<WeeklyMealPlanSlotDto> WeeklyPlan,
    DateTime UpdatedAt
);

public class UpdateNutritionTargetDto
{
    public int CalorieTarget { get; set; }
    public int ProteinGramsTarget { get; set; }
    public int CarbsGramsTarget { get; set; }
    public int FatGramsTarget { get; set; }
    public int WaterMlTarget { get; set; } = 3000;
    public FitnessGoalType GoalType { get; set; } = FitnessGoalType.Maintain;
    public double TargetProteinPerKg { get; set; } = 2.0;
    public bool EnableCarbCycling { get; set; }
    public int TrainingDayCalories { get; set; }
    public int TrainingDayCarbs { get; set; }
    public int RestDayCalories { get; set; }
    public int RestDayCarbs { get; set; }
}

public record AthleteProfileDto(
    double? CurrentWeightKg,
    double? CurrentHeightCm,
    double? CurrentBmi,
    string? CurrentBmiCategory,
    int CalorieTarget,
    int ProteinGramsTarget,
    int CarbsGramsTarget,
    int FatGramsTarget,
    double ProteinPerKgActual,
    FitnessGoalType GoalType,
    bool EnableCarbCycling,
    int TrainingDayCalories,
    int TrainingDayCarbs,
    int RestDayCalories,
    int RestDayCarbs,
    List<DailyNutritionSummaryDto> RecentDays
);

public record ChefScalingResultDto(
    Guid RecipeId,
    string RecipeName,
    int OriginalServings,
    double TargetServings,
    double ScaleFactor,
    double ScaledRawWeightGrams,
    double ScaledCookedWeightGrams,
    double ScaledCalories,
    double ScaledProtein,
    double ScaledCarbs,
    double ScaledFat,
    List<RecipeIngredientDto> ScaledIngredients
);
