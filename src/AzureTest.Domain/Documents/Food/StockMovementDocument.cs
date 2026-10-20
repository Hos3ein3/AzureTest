using System;
using AzureTest.Domain.Documents.Food.Enums;

namespace AzureTest.Domain.Documents.Food;

public class StockMovementDocument : AzureTest.Domain.Primitives.ITenantEntity
{
    public static class Rules
    {
        public const int NotesMaxLength = 500;
        public const int CorrelationIdMaxLength = 120;
        public const double MinQuantityDelta = -1000000.0;
        public const double MaxQuantityDelta = 1000000.0;
    }

    public Guid Id { get; init; } = Guid.CreateVersion7();
    public Guid UserId { get; set; }
    public Guid TenantId { get; set; }
    public Guid? StockItemId { get; set; }
    public Guid IngredientId { get; set; }
    public string IngredientName { get; set; } = string.Empty;
    public StockMovementType MovementType { get; set; } = StockMovementType.ManualCorrection;
    
    // Quantitative change
    public double QuantityDelta { get; set; }
    public string Unit { get; set; } = "g";
    public double NormalizedGramsDelta { get; set; }
    
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    // Idempotency & Reversal Tracking
    public string? CorrelationId { get; set; }
    
    // Waste and Financial Metrics
    public WasteReason? WasteReason { get; set; }
    public decimal? Cost { get; set; }
    
    public StockLocation? Location { get; set; }
    public string? Notes { get; set; }

    public static StockMovementDocument Create(
        Guid userId,
        Guid? stockItemId,
        Guid ingredientId,
        string ingredientName,
        StockMovementType movementType,
        double quantityDelta,
        string unit,
        double normalizedGramsDelta,
        string? correlationId = null,
        WasteReason? wasteReason = null,
        decimal? cost = null,
        StockLocation? location = null,
        string? notes = null)
    {
        return new StockMovementDocument
        {
            UserId = userId,
            StockItemId = stockItemId,
            IngredientId = ingredientId,
            IngredientName = ingredientName.Trim(),
            MovementType = movementType,
            QuantityDelta = Math.Round(quantityDelta, 2),
            Unit = string.IsNullOrWhiteSpace(unit) ? "g" : unit.Trim(),
            NormalizedGramsDelta = Math.Round(normalizedGramsDelta, 2),
            Timestamp = DateTime.UtcNow,
            CorrelationId = correlationId?.Trim(),
            WasteReason = wasteReason,
            Cost = cost,
            Location = location,
            Notes = notes?.Trim()
        };
    }
}
