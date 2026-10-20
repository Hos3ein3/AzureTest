using System;
using AzureTest.Domain.Documents.Food.Enums;

using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Documents.Food;

public class ShoppingListItemDocument : ITenantEntity
{
    public static class Rules
    {
        public const int NameMinLength = 1;
        public const int NameMaxLength = 160;
        public const int NotesMaxLength = 500;
        public const double MinQuantity = 0.01;
        public const double MaxQuantity = 100000.0;
        public const decimal MinCost = 0.0m;
        public const decimal MaxCost = 100000.0m;
    }

    public Guid Id { get; init; } = Guid.CreateVersion7();
    public Guid UserId { get; set; }
    public Guid TenantId { get; set; }
    public Guid? IngredientId { get; set; }
    public string Name { get; set; } = string.Empty;
    public double Quantity { get; set; } = 1.0;
    public string Unit { get; set; } = "piece";
    public double NormalizedGrams { get; set; }
    
    public ShoppingItemStatus Status { get; set; } = ShoppingItemStatus.ToBuy;
    public StoreSection StoreSection { get; set; } = StoreSection.Produce;
    public ShoppingItemSource Source { get; set; } = ShoppingItemSource.Manual;
    public StockLocation TargetLocation { get; set; } = StockLocation.Pantry;
    
    public decimal? EstimatedCost { get; set; }
    public string? Notes { get; set; }
    public DateTime? CreatedFromMealPlanDate { get; set; }
    
    public bool IsShared { get; set; } = true;
    public string? CreatedByUserName { get; set; }
    public string? LastModifiedByUserName { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public static ShoppingListItemDocument Create(
        Guid userId,
        string name,
        double quantity,
        string unit,
        StoreSection storeSection,
        ShoppingItemSource source = ShoppingItemSource.Manual,
        Guid? ingredientId = null,
        StockLocation targetLocation = StockLocation.Pantry,
        decimal? estimatedCost = null,
        string? notes = null,
        double normalizedGrams = 0,
        bool isShared = true,
        string? createdByUserName = null)
    {
        return new ShoppingListItemDocument
        {
            UserId = userId,
            IngredientId = ingredientId,
            Name = name.Trim(),
            Quantity = Math.Max(Rules.MinQuantity, Math.Round(quantity, 2)),
            Unit = string.IsNullOrWhiteSpace(unit) ? "piece" : unit.Trim(),
            NormalizedGrams = Math.Round(normalizedGrams, 2),
            StoreSection = storeSection,
            Source = source,
            TargetLocation = targetLocation,
            EstimatedCost = estimatedCost,
            Notes = notes?.Trim(),
            Status = ShoppingItemStatus.ToBuy,
            IsShared = isShared,
            CreatedByUserName = createdByUserName,
            LastModifiedByUserName = createdByUserName,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
}
