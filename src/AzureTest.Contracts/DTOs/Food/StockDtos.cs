using System;
using System.Collections.Generic;
using AzureTest.Contracts.DTOs.Common;
using AzureTest.Domain.Documents.Food.Enums;

namespace AzureTest.Contracts.DTOs.Food;

public record PantryStockItemDto(
    Guid Id,
    Guid UserId,
    Guid IngredientId,
    string IngredientName,
    StockLocation Location,
    double CurrentQuantity,
    string Unit,
    double CurrentNormalizedGrams,
    DateTime? ExpiryDate,
    string? BatchCode,
    DateTime PurchaseDate,
    decimal? UnitPrice,
    double? MinimumParLevel,
    bool IsStaple,
    StapleStatus StapleStatus,
    bool IsDepleted,
    bool NeedsRestockReconciliation,
    bool IsExpired,
    bool IsExpiringSoon,
    bool IsBelowParLevel,
    int? DaysUntilExpiry,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public class StockFilter : PagedFilterRequest
{
    public Guid UserId { get; set; }
    public StockLocation? Location { get; set; }
    public string? SearchTerm { get; set; }
    public bool IncludeDepleted { get; set; } = false;
    public bool OnlyLowStock { get; set; } = false;
    public bool OnlyExpiringSoon { get; set; } = false;
    public bool OnlyStaples { get; set; } = false;

    public StockFilter()
    {
        SortBy = "IngredientName";
        SortDescending = false;
        PageSize = 50;
    }
}

public record StockMovementDto(
    Guid Id,
    Guid UserId,
    Guid? StockItemId,
    Guid IngredientId,
    string IngredientName,
    StockMovementType MovementType,
    double QuantityDelta,
    string Unit,
    double NormalizedGramsDelta,
    DateTime Timestamp,
    string? CorrelationId,
    WasteReason? WasteReason,
    decimal? Cost,
    StockLocation? Location,
    string? Notes
);

public class StockMovementFilter : PagedFilterRequest
{
    public Guid UserId { get; set; }
    public Guid? IngredientId { get; set; }
    public StockMovementType? MovementType { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }

    public StockMovementFilter()
    {
        SortBy = "Timestamp";
        SortDescending = true;
        PageSize = 50;
    }
}

public class CreateStockItemDto
{
    public Guid IngredientId { get; set; }
    public StockLocation Location { get; set; } = StockLocation.Pantry;
    public double Quantity { get; set; } = 1.0;
    public string Unit { get; set; } = "g";
    public DateTime? ExpiryDate { get; set; }
    public decimal? UnitPrice { get; set; }
    public double? MinimumParLevel { get; set; }
    public bool IsStaple { get; set; }
    public StapleStatus StapleStatus { get; set; } = StapleStatus.InStock;
    public string? BatchCode { get; set; }
    public string? Notes { get; set; }
}

public class AdjustStockDto
{
    public Guid StockItemId { get; set; }
    public double NewQuantity { get; set; }
    public string? Reason { get; set; }
}

public class TransferStockDto
{
    public Guid StockItemId { get; set; }
    public StockLocation NewLocation { get; set; }
}

public class LogWasteDto
{
    public Guid StockItemId { get; set; }
    public double Quantity { get; set; }
    public WasteReason Reason { get; set; } = WasteReason.SpoiledExpired;
    public decimal? EstimatedCost { get; set; }
    public string? Notes { get; set; }
}

public class UpdateStapleStatusDto
{
    public Guid StockItemId { get; set; }
    public StapleStatus Status { get; set; } = StapleStatus.InStock;
}

public record StockInventoryOverviewDto(
    int TotalItems,
    int LowStockCount,
    int ExpiringSoonCount,
    int ExpiredCount,
    decimal TotalEstimatedValue,
    List<PantryStockItemDto> Items,
    List<StockMovementDto> RecentMovements
);
