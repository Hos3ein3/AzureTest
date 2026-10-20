using System;
using System.Collections.Generic;
using AzureTest.Contracts.DTOs.Common;
using AzureTest.Domain.Documents.Food.Enums;

namespace AzureTest.Contracts.DTOs.Food;

public class MealLogFilter : PagedFilterRequest
{
    public Guid UserId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public MealType? MealType { get; set; }

    public MealLogFilter()
    {
        SortBy = "Date";
        SortDescending = true;
        PageSize = 30;
    }
}

public record MealLogItemDto(
    Guid ItemId,
    bool IsRecipe,
    Guid ReferenceId,
    string Name,
    double Quantity,
    string Unit,
    double EquivalentGramsOrServings,
    double Calories,
    double Protein,
    double Carbs,
    double Fat,
    double Fiber,
    double Sugar,
    double Sodium
);

public record MealLogDto(
    Guid Id,
    Guid UserId,
    DateTime Date,
    MealType MealType,
    List<MealLogItemDto> Items,
    double TotalCalories,
    double TotalProtein,
    double TotalCarbs,
    double TotalFat,
    double TotalFiber,
    double TotalSugar,
    double TotalSodium,
    int WaterMl,
    string? Notes,
    DateTime LoggedAt
);

public class LogMealItemDto
{
    public DateTime Date { get; set; } = DateTime.UtcNow.Date;
    public MealType MealType { get; set; } = MealType.Breakfast;
    public bool IsRecipe { get; set; }
    public Guid ReferenceId { get; set; }
    public double Quantity { get; set; }
    public string Unit { get; set; } = "g";
    public string? Notes { get; set; }
    public bool DeductFromStock { get; set; } = true;
}

public class LogWaterDto
{
    public DateTime Date { get; set; } = DateTime.UtcNow.Date;
    public int WaterMl { get; set; }
}

public record DailyNutritionSummaryDto(
    DateTime Date,
    double TotalCalories,
    double TotalProtein,
    double TotalCarbs,
    double TotalFat,
    double TotalFiber,
    double TotalSugar,
    double TotalSodium,
    int TotalWaterMl,
    int TargetCalories,
    int TargetProtein,
    int TargetCarbs,
    int TargetFat,
    int TargetWaterMl,
    List<MealLogDto> Meals
);
