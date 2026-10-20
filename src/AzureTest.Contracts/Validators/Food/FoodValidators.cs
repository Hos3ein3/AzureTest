using AzureTest.Contracts.DTOs.Food;
using AzureTest.Domain.Documents.Food;
using FluentValidation;

namespace AzureTest.Contracts.Validators.Food;

public class CreateIngredientDtoValidator : AbstractValidator<CreateIngredientDto>
{
    public CreateIngredientDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Ingredient name is required.")
            .Length(IngredientDocument.Rules.NameMinLength, IngredientDocument.Rules.NameMaxLength);

        RuleFor(x => x.Calories)
            .InclusiveBetween(IngredientDocument.Rules.MinNutritionValue, IngredientDocument.Rules.MaxCaloriesPer100g);

        RuleFor(x => x.ProteinGrams)
            .InclusiveBetween(IngredientDocument.Rules.MinNutritionValue, IngredientDocument.Rules.MaxMacroGramsPer100g);

        RuleFor(x => x.CarbsGrams)
            .InclusiveBetween(IngredientDocument.Rules.MinNutritionValue, IngredientDocument.Rules.MaxMacroGramsPer100g);

        RuleFor(x => x.FatGrams)
            .InclusiveBetween(IngredientDocument.Rules.MinNutritionValue, IngredientDocument.Rules.MaxMacroGramsPer100g);

        RuleFor(x => x.DefaultServingSizeGrams)
            .GreaterThan(0).WithMessage("Default serving size must be greater than 0g.");

        RuleFor(x => x.DensityGramsPerMl)
            .InclusiveBetween(IngredientDocument.Rules.MinDensity, IngredientDocument.Rules.MaxDensity);
    }
}

public class UpdateIngredientDtoValidator : AbstractValidator<UpdateIngredientDto>
{
    public UpdateIngredientDtoValidator()
    {
        Include(new CreateIngredientDtoValidator());
        RuleFor(x => x.Id).NotEmpty().WithMessage("Ingredient ID is required.");
    }
}

public class CreateRecipeDtoValidator : AbstractValidator<CreateRecipeDto>
{
    public CreateRecipeDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Recipe name is required.")
            .Length(RecipeDocument.Rules.TitleMinLength, RecipeDocument.Rules.TitleMaxLength);

        RuleFor(x => x.Servings)
            .InclusiveBetween(RecipeDocument.Rules.MinServings, RecipeDocument.Rules.MaxServings);

        RuleFor(x => x.PrepTimeMinutes)
            .InclusiveBetween(RecipeDocument.Rules.MinPrepTimeMinutes, RecipeDocument.Rules.MaxPrepTimeMinutes);

        RuleFor(x => x.CookTimeMinutes)
            .InclusiveBetween(RecipeDocument.Rules.MinCookTimeMinutes, RecipeDocument.Rules.MaxCookTimeMinutes);

        RuleFor(x => x.CookingYieldFactor)
            .InclusiveBetween(RecipeDocument.Rules.MinYieldFactor, RecipeDocument.Rules.MaxYieldFactor);

        RuleFor(x => x.Ingredients)
            .NotEmpty().WithMessage("A recipe must have at least one ingredient.");
    }
}

public class UpdateRecipeDtoValidator : AbstractValidator<UpdateRecipeDto>
{
    public UpdateRecipeDtoValidator()
    {
        Include(new CreateRecipeDtoValidator());
        RuleFor(x => x.Id).NotEmpty().WithMessage("Recipe ID is required.");
    }
}

public class LogMealItemDtoValidator : AbstractValidator<LogMealItemDto>
{
    public LogMealItemDtoValidator()
    {
        RuleFor(x => x.ReferenceId).NotEmpty().WithMessage("Selected food or recipe is required.");
        RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("Quantity must be greater than zero.");
        RuleFor(x => x.Unit).NotEmpty().WithMessage("Unit is required.");
    }
}

public class LogWaterDtoValidator : AbstractValidator<LogWaterDto>
{
    public LogWaterDtoValidator()
    {
        RuleFor(x => x.WaterMl)
            .InclusiveBetween(1, MealLogDocument.Rules.MaxWaterMl)
            .WithMessage("Water volume must be between 1 and 10,000 ml.");
    }
}

public class UpdateNutritionTargetDtoValidator : AbstractValidator<UpdateNutritionTargetDto>
{
    public UpdateNutritionTargetDtoValidator()
    {
        RuleFor(x => x.CalorieTarget)
            .InclusiveBetween(DailyNutritionTargetDocument.Rules.MinCalories, DailyNutritionTargetDocument.Rules.MaxCalories);

        RuleFor(x => x.ProteinGramsTarget)
            .InclusiveBetween(DailyNutritionTargetDocument.Rules.MinMacroGrams, DailyNutritionTargetDocument.Rules.MaxMacroGrams);

        RuleFor(x => x.CarbsGramsTarget)
            .InclusiveBetween(DailyNutritionTargetDocument.Rules.MinMacroGrams, DailyNutritionTargetDocument.Rules.MaxMacroGrams);

        RuleFor(x => x.FatGramsTarget)
            .InclusiveBetween(DailyNutritionTargetDocument.Rules.MinMacroGrams, DailyNutritionTargetDocument.Rules.MaxMacroGrams);

        RuleFor(x => x.WaterMlTarget)
            .InclusiveBetween(DailyNutritionTargetDocument.Rules.MinWaterMl, DailyNutritionTargetDocument.Rules.MaxWaterMl);
    }
}
