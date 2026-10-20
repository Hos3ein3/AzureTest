using System;
using System.Collections.Generic;
using System.Linq;
using AzureTest.Domain.Documents.Food.Enums;

namespace AzureTest.Domain.Documents.Food;

public class MealLogItem
{
    public Guid ItemId { get; init; } = Guid.CreateVersion7();
    public bool IsRecipe { get; set; }
    public Guid ReferenceId { get; set; } // IngredientId or RecipeId
    public string Name { get; set; } = string.Empty;
    public double Quantity { get; set; }
    public string Unit { get; set; } = "g";
    public double EquivalentGramsOrServings { get; set; }

    public double Calories { get; set; }
    public double Protein { get; set; }
    public double Carbs { get; set; }
    public double Fat { get; set; }
    public double Fiber { get; set; }
    public double Sugar { get; set; }
    public double Sodium { get; set; }
}

public class MealLogDocument
{
    public static class Rules
    {
        public const int NotesMaxLength = 1000;
        public const int MaxItemsPerMeal = 100;
        public const int MaxWaterMl = 10000;
    }

    public Guid Id { get; init; } = Guid.CreateVersion7();
    public Guid UserId { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow.Date;
    public MealType MealType { get; set; } = MealType.Breakfast;
    public List<MealLogItem> Items { get; set; } = new();

    public double TotalCalories { get; set; }
    public double TotalProtein { get; set; }
    public double TotalCarbs { get; set; }
    public double TotalFat { get; set; }
    public double TotalFiber { get; set; }
    public double TotalSugar { get; set; }
    public double TotalSodium { get; set; }

    public int WaterMl { get; set; }
    public string? Notes { get; set; }
    public DateTime LoggedAt { get; set; } = DateTime.UtcNow;

    public void RecalculateTotals()
    {
        TotalCalories = Math.Round(Items.Sum(x => x.Calories), 1);
        TotalProtein = Math.Round(Items.Sum(x => x.Protein), 1);
        TotalCarbs = Math.Round(Items.Sum(x => x.Carbs), 1);
        TotalFat = Math.Round(Items.Sum(x => x.Fat), 1);
        TotalFiber = Math.Round(Items.Sum(x => x.Fiber), 1);
        TotalSugar = Math.Round(Items.Sum(x => x.Sugar), 1);
        TotalSodium = Math.Round(Items.Sum(x => x.Sodium), 1);
    }
}
