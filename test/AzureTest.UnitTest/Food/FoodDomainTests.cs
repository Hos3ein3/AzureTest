using System;
using System.Collections.Generic;
using AzureTest.Contracts.DTOs.Food;
using AzureTest.Contracts.Validators.Food;
using AzureTest.Domain.Documents.Food;
using AzureTest.Domain.Documents.Food.Enums;
using FluentAssertions;
using Xunit;

namespace AzureTest.UnitTest.Food;

public class FoodDomainTests
{
    [Fact]
    public void Ingredient_CalculateGrams_WithDensity_ShouldConvertAccurately()
    {
        // Arrange: Olive oil with density 0.92 g/ml, 1 tbsp = 14g
        var oil = IngredientDocument.Create(
            "Olive Oil",
            FoodCategory.OilsAndFats,
            884, 0, 0, 100,
            densityGramsPerMl: 0.92,
            gramsPerTbsp: 14.0
        );

        // Act & Assert
        oil.CalculateGrams(100, "g").Should().Be(100.0);
        oil.CalculateGrams(100, "ml").Should().BeApproximately(92.0, 0.01);
        oil.CalculateGrams(2, "tbsp").Should().Be(28.0);
    }

    [Fact]
    public void Ingredient_CalculateNutritionFor_ShouldScaleProportionately()
    {
        // Arrange: Chicken Breast 165 kcal, 31g protein per 100g
        var chicken = IngredientDocument.Create(
            "Chicken Breast",
            FoodCategory.MeatAndPoultry,
            165, 31, 0, 3.6
        );

        // Act: 200g of chicken
        var (cal, pro, carb, fat, _, _, _) = chicken.CalculateNutritionFor(200, "g");

        // Assert
        cal.Should().Be(330.0);
        pro.Should().Be(62.0);
        carb.Should().Be(0.0);
        fat.Should().Be(7.2);
    }

    [Fact]
    public void Recipe_YieldFactor_ShouldCalculateRawVsCookedWeight_Accurately()
    {
        // Arrange: 400g raw chicken breast with 0.75 yield factor (25% water loss during roasting)
        var recipe = new RecipeDocument
        {
            Name = "Roasted Chicken Breast",
            Servings = 2,
            CookingYieldFactor = 0.75,
            Ingredients = new List<RecipeIngredientItem>
            {
                new()
                {
                    IngredientId = Guid.NewGuid(),
                    IngredientName = "Chicken Breast",
                    Quantity = 400,
                    Unit = "g",
                    ComputedGrams = 400,
                    Calories = 660,
                    Protein = 124,
                    Carbs = 0,
                    Fat = 14.4
                }
            }
        };

        // Act
        recipe.RecalculateTotals();

        // Assert
        recipe.RawWeightGrams.Should().Be(400.0);
        recipe.CookedWeightGrams.Should().Be(300.0); // 400 * 0.75
        recipe.TotalCalories.Should().Be(660.0);
        recipe.PerServingCalories.Should().Be(330.0);
        recipe.PerServingProtein.Should().Be(62.0);
        recipe.PerServingCookedWeightGrams.Should().Be(150.0);
        // Per 100g cooked: (660 / 300) * 100 = 220 kcal/100g cooked
        recipe.Per100gCookedCalories.Should().Be(220.0);
    }

    [Fact]
    public void MealLog_RecalculateTotals_ShouldSumAllItems()
    {
        // Arrange
        var meal = new MealLogDocument
        {
            UserId = Guid.NewGuid(),
            MealType = MealType.Lunch,
            Items = new List<MealLogItem>
            {
                new() { Name = "Rice", Quantity = 150, Calories = 195, Protein = 4, Carbs = 42, Fat = 0.5 },
                new() { Name = "Salmon", Quantity = 150, Calories = 312, Protein = 30.6, Carbs = 0, Fat = 20.1 }
            }
        };

        // Act
        meal.RecalculateTotals();

        // Assert
        meal.TotalCalories.Should().Be(507.0);
        meal.TotalProtein.Should().Be(34.6);
        meal.TotalCarbs.Should().Be(42.0);
        meal.TotalFat.Should().Be(20.6);
    }

    [Fact]
    public void CreateIngredientValidator_ShouldRejectInvalidCaloriesOrNegativeMacros()
    {
        var validator = new CreateIngredientDtoValidator();

        var invalidDto = new CreateIngredientDto
        {
            Name = "X", // too short (min length 2)
            Calories = -10, // negative
            ProteinGrams = 150, // exceeds max 100g per 100g
            DefaultServingSizeGrams = 0 // invalid
        };

        var result = validator.Validate(invalidDto);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
        result.Errors.Should().Contain(e => e.PropertyName == "Calories");
        result.Errors.Should().Contain(e => e.PropertyName == "ProteinGrams");
        result.Errors.Should().Contain(e => e.PropertyName == "DefaultServingSizeGrams");
    }
}
