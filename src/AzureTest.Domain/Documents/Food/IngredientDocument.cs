using System;
using System.Collections.Generic;
using AzureTest.Domain.Documents.Food.Enums;

namespace AzureTest.Domain.Documents.Food;

public class IngredientDocument
{
    public static class Rules
    {
        public const int NameMinLength = 2;
        public const int NameMaxLength = 120;
        public const int DescriptionMaxLength = 500;
        public const double MinNutritionValue = 0.0;
        public const double MaxCaloriesPer100g = 900.0;
        public const double MaxMacroGramsPer100g = 100.0;
        public const double MaxSodiumMgPer100g = 100000.0;
        public const double MinDensity = 0.1;
        public const double MaxDensity = 10.0;
    }

    public Guid Id { get; init; } = Guid.CreateVersion7();
    public string Name { get; set; } = string.Empty;
    public FoodCategory Category { get; set; } = FoodCategory.Other;
    public string? Description { get; set; }

    // Nutrition per 100g
    public double Calories { get; set; }
    public double ProteinGrams { get; set; }
    public double CarbsGrams { get; set; }
    public double FatGrams { get; set; }
    public double FiberGrams { get; set; }
    public double SugarGrams { get; set; }
    public double SodiumMg { get; set; }

    // Unit Conversion & Portions
    public double DefaultServingSizeGrams { get; set; } = 100.0;
    public string DefaultUnit { get; set; } = "g";
    public double DensityGramsPerMl { get; set; } = 1.0;
    public double? GramsPerPiece { get; set; }
    public double? GramsPerTbsp { get; set; }
    public double? GramsPerCup { get; set; }
    public List<string> AllowedUnits { get; set; } = new() { "g", "ml" };

    // Diets & Allergens
    public List<DietaryTag> DietaryTags { get; set; } = new();
    public string? AllergenInfo { get; set; }
    public decimal? ApproximateCostPer100g { get; set; }

    public Guid? CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public double CalculateGrams(double quantity, string unit)
    {
        if (quantity <= 0) return 0;
        var cleanUnit = (unit ?? "g").Trim().ToLowerInvariant();

        return cleanUnit switch
        {
            "g" or "gram" or "grams" => quantity,
            "kg" or "kilogram" or "kilograms" => quantity * 1000.0,
            "ml" or "milliliter" or "milliliters" => quantity * (DensityGramsPerMl > 0 ? DensityGramsPerMl : 1.0),
            "l" or "liter" or "liters" => quantity * 1000.0 * (DensityGramsPerMl > 0 ? DensityGramsPerMl : 1.0),
            "piece" or "pc" or "item" => quantity * (GramsPerPiece ?? DefaultServingSizeGrams),
            "tbsp" or "tablespoon" => quantity * (GramsPerTbsp ?? (15.0 * (DensityGramsPerMl > 0 ? DensityGramsPerMl : 1.0))),
            "tsp" or "teaspoon" => quantity * ((GramsPerTbsp ?? (15.0 * (DensityGramsPerMl > 0 ? DensityGramsPerMl : 1.0))) / 3.0),
            "cup" or "cups" => quantity * (GramsPerCup ?? (240.0 * (DensityGramsPerMl > 0 ? DensityGramsPerMl : 1.0))),
            "oz" or "ounce" => quantity * 28.3495,
            "lb" or "lbs" or "pound" => quantity * 453.592,
            _ => quantity
        };
    }

    public (double Calories, double Protein, double Carbs, double Fat, double Fiber, double Sugar, double Sodium) CalculateNutritionFor(double quantity, string unit)
    {
        double grams = CalculateGrams(quantity, unit);
        double factor = grams / 100.0;

        return (
            Math.Round(Calories * factor, 1),
            Math.Round(ProteinGrams * factor, 1),
            Math.Round(CarbsGrams * factor, 1),
            Math.Round(FatGrams * factor, 1),
            Math.Round(FiberGrams * factor, 1),
            Math.Round(SugarGrams * factor, 1),
            Math.Round(SodiumMg * factor, 1)
        );
    }

    public static IngredientDocument Create(
        string name,
        FoodCategory category,
        double calories,
        double protein,
        double carbs,
        double fat,
        double fiber = 0,
        double sugar = 0,
        double sodium = 0,
        double defaultServingSizeGrams = 100,
        string defaultUnit = "g",
        double densityGramsPerMl = 1.0,
        double? gramsPerPiece = null,
        double? gramsPerTbsp = null,
        double? gramsPerCup = null,
        List<string>? allowedUnits = null,
        List<DietaryTag>? dietaryTags = null,
        string? allergenInfo = null,
        decimal? costPer100g = null,
        Guid? createdByUserId = null)
    {
        return new IngredientDocument
        {
            Name = name.Trim(),
            Category = category,
            Calories = Math.Round(calories, 1),
            ProteinGrams = Math.Round(protein, 1),
            CarbsGrams = Math.Round(carbs, 1),
            FatGrams = Math.Round(fat, 1),
            FiberGrams = Math.Round(fiber, 1),
            SugarGrams = Math.Round(sugar, 1),
            SodiumMg = Math.Round(sodium, 1),
            DefaultServingSizeGrams = defaultServingSizeGrams > 0 ? defaultServingSizeGrams : 100,
            DefaultUnit = string.IsNullOrWhiteSpace(defaultUnit) ? "g" : defaultUnit.Trim(),
            DensityGramsPerMl = densityGramsPerMl > 0 ? densityGramsPerMl : 1.0,
            GramsPerPiece = gramsPerPiece,
            GramsPerTbsp = gramsPerTbsp,
            GramsPerCup = gramsPerCup,
            AllowedUnits = allowedUnits ?? new List<string> { "g", "ml" },
            DietaryTags = dietaryTags ?? new List<DietaryTag>(),
            AllergenInfo = allergenInfo?.Trim(),
            ApproximateCostPer100g = costPer100g,
            CreatedByUserId = createdByUserId,
            CreatedAt = DateTime.UtcNow
        };
    }
}
