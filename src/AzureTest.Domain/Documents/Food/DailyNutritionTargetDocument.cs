using System;
using System.Collections.Generic;
using AzureTest.Domain.Documents.Food.Enums;

namespace AzureTest.Domain.Documents.Food;

public class PantryItem
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
    public Guid? IngredientId { get; set; }
    public string Name { get; set; } = string.Empty;
    public double Quantity { get; set; }
    public string Unit { get; set; } = "g";
    public DateTime? ExpiryDate { get; set; }
    public string Location { get; set; } = "Pantry"; // "Fridge", "Pantry", "Freezer"
}

public class ShoppingListItem
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
    public Guid? IngredientId { get; set; }
    public string Name { get; set; } = string.Empty;
    public double Quantity { get; set; }
    public string Unit { get; set; } = "g";
    public bool IsPurchased { get; set; }
    public string Category { get; set; } = "Produce";
    public string? Notes { get; set; }
}

public class WeeklyMealPlanSlot
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
    public DayOfWeek DayOfWeek { get; set; }
    public MealType MealType { get; set; }
    public Guid? RecipeId { get; set; }
    public string? RecipeName { get; set; }
    public int Servings { get; set; } = 1;
    public string? Notes { get; set; }
}

public class DailyNutritionTargetDocument
{
    public static class Rules
    {
        public const int MinCalories = 500;
        public const int MaxCalories = 10000;
        public const int MinMacroGrams = 0;
        public const int MaxMacroGrams = 1500;
        public const double MinProteinPerKg = 0.5;
        public const double MaxProteinPerKg = 4.5;
        public const int MinWaterMl = 500;
        public const int MaxWaterMl = 15000;
    }

    public Guid Id { get; init; } = Guid.CreateVersion7();
    public Guid UserId { get; set; }

    // Core Daily Targets
    public int CalorieTarget { get; set; } = 2200;
    public int ProteinGramsTarget { get; set; } = 160;
    public int CarbsGramsTarget { get; set; } = 220;
    public int FatGramsTarget { get; set; } = 65;
    public int WaterMlTarget { get; set; } = 3000;

    // Athlete & Cycling Settings
    public FitnessGoalType GoalType { get; set; } = FitnessGoalType.Maintain;
    public double TargetProteinPerKg { get; set; } = 2.0;
    public bool EnableCarbCycling { get; set; } = false;
    public int TrainingDayCalories { get; set; } = 2400;
    public int TrainingDayCarbs { get; set; } = 270;
    public int RestDayCalories { get; set; } = 2000;
    public int RestDayCarbs { get; set; } = 170;

    // Pantry, Shopping List, Meal Plan
    public List<PantryItem> Pantry { get; set; } = new();
    public List<ShoppingListItem> ShoppingList { get; set; } = new();
    public List<WeeklyMealPlanSlot> WeeklyPlan { get; set; } = new();

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public static DailyNutritionTargetDocument CreateDefault(Guid userId)
    {
        return new DailyNutritionTargetDocument
        {
            UserId = userId,
            CalorieTarget = 2200,
            ProteinGramsTarget = 150,
            CarbsGramsTarget = 225,
            FatGramsTarget = 65,
            WaterMlTarget = 3000,
            GoalType = FitnessGoalType.Maintain,
            TargetProteinPerKg = 2.0,
            TrainingDayCalories = 2400,
            TrainingDayCarbs = 270,
            RestDayCalories = 2000,
            RestDayCarbs = 170
        };
    }
}
