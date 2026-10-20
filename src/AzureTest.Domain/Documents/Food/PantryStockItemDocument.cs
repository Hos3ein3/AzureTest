using System;
using AzureTest.Domain.Documents.Food.Enums;

namespace AzureTest.Domain.Documents.Food;

public class PantryStockItemDocument
{
    public static class Rules
    {
        public const int ItemNameMinLength = 2;
        public const int ItemNameMaxLength = 160;
        public const double MinQuantity = -100000.0; // Allow negative for unstocked consumption
        public const double MaxQuantity = 1000000.0;
        public const decimal MinPrice = 0.0m;
        public const decimal MaxPrice = 100000.0m;
    }

    public Guid Id { get; init; } = Guid.CreateVersion7();
    public Guid UserId { get; set; }
    public Guid IngredientId { get; set; }
    public string IngredientName { get; set; } = string.Empty;
    public StockLocation Location { get; set; } = StockLocation.Pantry;
    
    // Inventory balance
    public double CurrentQuantity { get; set; }
    public string Unit { get; set; } = "g";
    public double CurrentNormalizedGrams { get; set; }
    
    // Expiry & Batch info
    public DateTime? ExpiryDate { get; set; }
    public string? BatchCode { get; set; }
    public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;
    public decimal? UnitPrice { get; set; }
    
    // Reorder threshold & Kitchen staples
    public double? MinimumParLevel { get; set; }
    public bool IsStaple { get; set; }
    public StapleStatus StapleStatus { get; set; } = StapleStatus.InStock;
    
    // Status flags
    public bool IsDepleted { get; set; }
    public bool NeedsRestockReconciliation { get; set; }
    
    // Concurrency version
    public long Version { get; set; } = 1;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public bool IsExpired(DateTime? asOfUtc = null)
    {
        if (!ExpiryDate.HasValue) return false;
        var checkTime = asOfUtc ?? DateTime.UtcNow;
        return ExpiryDate.Value.Date < checkTime.Date;
    }

    public bool IsExpiringSoon(int warningDays = 3, DateTime? asOfUtc = null)
    {
        if (!ExpiryDate.HasValue) return false;
        var checkTime = asOfUtc ?? DateTime.UtcNow;
        var diff = (ExpiryDate.Value.Date - checkTime.Date).TotalDays;
        return diff >= 0 && diff <= warningDays;
    }

    public bool IsBelowParLevel()
    {
        if (IsStaple)
        {
            return StapleStatus == StapleStatus.Low || StapleStatus == StapleStatus.OutOfStock;
        }

        if (!MinimumParLevel.HasValue || MinimumParLevel.Value <= 0) return false;
        return CurrentQuantity <= MinimumParLevel.Value;
    }

    public static PantryStockItemDocument Create(
        Guid userId,
        Guid ingredientId,
        string ingredientName,
        StockLocation location,
        double quantity,
        string unit,
        double normalizedGrams,
        DateTime? expiryDate = null,
        decimal? unitPrice = null,
        double? minimumParLevel = null,
        bool isStaple = false,
        string? batchCode = null)
    {
        return new PantryStockItemDocument
        {
            UserId = userId,
            IngredientId = ingredientId,
            IngredientName = ingredientName.Trim(),
            Location = location,
            CurrentQuantity = Math.Round(quantity, 2),
            Unit = string.IsNullOrWhiteSpace(unit) ? "g" : unit.Trim(),
            CurrentNormalizedGrams = Math.Round(normalizedGrams, 2),
            ExpiryDate = expiryDate,
            UnitPrice = unitPrice,
            MinimumParLevel = minimumParLevel,
            IsStaple = isStaple,
            StapleStatus = isStaple ? (quantity <= 0 ? StapleStatus.OutOfStock : StapleStatus.InStock) : StapleStatus.InStock,
            BatchCode = batchCode,
            IsDepleted = quantity <= 0 && !isStaple,
            NeedsRestockReconciliation = quantity < 0,
            Version = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
}
