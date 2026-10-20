using System;
using System.Collections.Generic;
using System.Linq;
using AzureTest.Domain.Documents.Food.Enums;

namespace AzureTest.Domain.Documents.Food;

public class RecipeIngredientItem
{
    public Guid IngredientId { get; set; }
    public string IngredientName { get; set; } = string.Empty;
    public double Quantity { get; set; }
    public string Unit { get; set; } = "g";
    public string? Notes { get; set; }
    public string? SubstitutionNote { get; set; }

    // Computed nutrition for this ingredient row
    public double ComputedGrams { get; set; }
    public double Calories { get; set; }
    public double Protein { get; set; }
    public double Carbs { get; set; }
    public double Fat { get; set; }
    public double Fiber { get; set; }
    public double Sugar { get; set; }
    public double Sodium { get; set; }
}

public class RecipeStepItem
{
    public int StepNumber { get; set; }
    public string Instruction { get; set; } = string.Empty;
    public int? TimerMinutes { get; set; }
    public string? ChefTip { get; set; }
}

public class RecipeDocument
{
    public static class Rules
    {
        public const int TitleMinLength = 2;
        public const int TitleMaxLength = 160;
        public const int DescriptionMaxLength = 2000;
        public const int MinServings = 1;
        public const int MaxServings = 100;
        public const int MinPrepTimeMinutes = 0;
        public const int MaxPrepTimeMinutes = 1440;
        public const int MinCookTimeMinutes = 0;
        public const int MaxCookTimeMinutes = 1440;
        public const double MinYieldFactor = 0.2;
        public const double MaxYieldFactor = 5.0;
    }

    public Guid Id { get; init; } = Guid.CreateVersion7();
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = "Main Dish";
    public int Servings { get; set; } = 1;
    public int PrepTimeMinutes { get; set; }
    public int CookTimeMinutes { get; set; }
    public int TotalTimeMinutes => PrepTimeMinutes + CookTimeMinutes;
    public RecipeDifficulty Difficulty { get; set; } = RecipeDifficulty.Easy;
    public List<string> Tags { get; set; } = new();
    public List<DietaryTag> DietaryTags { get; set; } = new();

    public List<RecipeIngredientItem> Ingredients { get; set; } = new();
    public List<RecipeStepItem> Steps { get; set; } = new();

    // Cooking Physics: Raw vs Cooked weight & oil absorption
    // e.g., 0.75 for chicken/beef (water loss during grilling/roasting)
    // e.g., 2.2 for dry rice or pasta (water absorption during boiling)
    public double CookingYieldFactor { get; set; } = 1.0;
    public double RawWeightGrams => Math.Round(Ingredients.Sum(i => i.ComputedGrams), 1);
    public double CookedWeightGrams => Math.Round(RawWeightGrams * (CookingYieldFactor > 0 ? CookingYieldFactor : 1.0), 1);

    // Totals for whole recipe
    public double TotalCalories { get; set; }
    public double TotalProtein { get; set; }
    public double TotalCarbs { get; set; }
    public double TotalFat { get; set; }
    public double TotalFiber { get; set; }
    public double TotalSugar { get; set; }
    public double TotalSodium { get; set; }

    // Per serving
    public double PerServingCalories => Servings > 0 ? Math.Round(TotalCalories / Servings, 1) : TotalCalories;
    public double PerServingProtein => Servings > 0 ? Math.Round(TotalProtein / Servings, 1) : TotalProtein;
    public double PerServingCarbs => Servings > 0 ? Math.Round(TotalCarbs / Servings, 1) : TotalCarbs;
    public double PerServingFat => Servings > 0 ? Math.Round(TotalFat / Servings, 1) : TotalFat;
    public double PerServingFiber => Servings > 0 ? Math.Round(TotalFiber / Servings, 1) : TotalFiber;
    public double PerServingCookedWeightGrams => Servings > 0 ? Math.Round(CookedWeightGrams / Servings, 1) : CookedWeightGrams;

    // Per 100g Cooked
    public double Per100gCookedCalories => CookedWeightGrams > 0 ? Math.Round((TotalCalories / CookedWeightGrams) * 100.0, 1) : 0;
    public double Per100gCookedProtein => CookedWeightGrams > 0 ? Math.Round((TotalProtein / CookedWeightGrams) * 100.0, 1) : 0;
    public double Per100gCookedCarbs => CookedWeightGrams > 0 ? Math.Round((TotalCarbs / CookedWeightGrams) * 100.0, 1) : 0;
    public double Per100gCookedFat => CookedWeightGrams > 0 ? Math.Round((TotalFat / CookedWeightGrams) * 100.0, 1) : 0;

    public bool IsFavorite { get; set; }
    public string? ImageUrl { get; set; }
    public decimal? EstimatedCost { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public void RecalculateTotals()
    {
        TotalCalories = Math.Round(Ingredients.Sum(x => x.Calories), 1);
        TotalProtein = Math.Round(Ingredients.Sum(x => x.Protein), 1);
        TotalCarbs = Math.Round(Ingredients.Sum(x => x.Carbs), 1);
        TotalFat = Math.Round(Ingredients.Sum(x => x.Fat), 1);
        TotalFiber = Math.Round(Ingredients.Sum(x => x.Fiber), 1);
        TotalSugar = Math.Round(Ingredients.Sum(x => x.Sugar), 1);
        TotalSodium = Math.Round(Ingredients.Sum(x => x.Sodium), 1);
    }
}
