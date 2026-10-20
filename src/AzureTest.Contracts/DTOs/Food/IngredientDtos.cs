using System;
using System.Collections.Generic;
using AzureTest.Contracts.DTOs.Common;
using AzureTest.Domain.Documents.Food.Enums;

namespace AzureTest.Contracts.DTOs.Food;

public class IngredientFilter : PagedFilterRequest
{
    public FoodCategory? Category { get; set; }
    public DietaryTag? DietaryTag { get; set; }
    public Guid? UserId { get; set; }

    public IngredientFilter()
    {
        SortBy = "Name";
        SortDescending = false;
        PageSize = 20;
    }
}

public record IngredientDto(
    Guid Id,
    string Name,
    FoodCategory Category,
    string? Description,
    double Calories,
    double ProteinGrams,
    double CarbsGrams,
    double FatGrams,
    double FiberGrams,
    double SugarGrams,
    double SodiumMg,
    double DefaultServingSizeGrams,
    string DefaultUnit,
    double DensityGramsPerMl,
    double? GramsPerPiece,
    double? GramsPerTbsp,
    double? GramsPerCup,
    List<string> AllowedUnits,
    List<DietaryTag> DietaryTags,
    string? AllergenInfo,
    decimal? ApproximateCostPer100g,
    Guid? CreatedByUserId,
    DateTime CreatedAt
);

public class CreateIngredientDto
{
    public string Name { get; set; } = string.Empty;
    public FoodCategory Category { get; set; } = FoodCategory.Other;
    public string? Description { get; set; }
    public double Calories { get; set; }
    public double ProteinGrams { get; set; }
    public double CarbsGrams { get; set; }
    public double FatGrams { get; set; }
    public double FiberGrams { get; set; }
    public double SugarGrams { get; set; }
    public double SodiumMg { get; set; }
    public double DefaultServingSizeGrams { get; set; } = 100.0;
    public string DefaultUnit { get; set; } = "g";
    public double DensityGramsPerMl { get; set; } = 1.0;
    public double? GramsPerPiece { get; set; }
    public double? GramsPerTbsp { get; set; }
    public double? GramsPerCup { get; set; }
    public List<string> AllowedUnits { get; set; } = new() { "g", "ml" };
    public List<DietaryTag> DietaryTags { get; set; } = new();
    public string? AllergenInfo { get; set; }
    public decimal? ApproximateCostPer100g { get; set; }
}

public class UpdateIngredientDto : CreateIngredientDto
{
    public Guid Id { get; set; }
}
