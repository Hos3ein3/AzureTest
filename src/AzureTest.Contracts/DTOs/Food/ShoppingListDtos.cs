using System;
using System.Collections.Generic;
using AzureTest.Contracts.DTOs.Common;
using AzureTest.Domain.Documents.Food.Enums;

namespace AzureTest.Contracts.DTOs.Food;

public record GroceryShoppingItemDto(
    Guid Id,
    Guid UserId,
    Guid? IngredientId,
    string Name,
    double Quantity,
    string Unit,
    double NormalizedGrams,
    ShoppingItemStatus Status,
    StoreSection StoreSection,
    ShoppingItemSource Source,
    StockLocation TargetLocation,
    decimal? EstimatedCost,
    string? Notes,
    DateTime CreatedAt,
    bool IsShared = true,
    string? CreatedByUserName = null,
    string? LastModifiedByUserName = null
);

public class ShoppingListFilter : PagedFilterRequest
{
    public Guid UserId { get; set; }
    public bool IncludeShared { get; set; } = true;
    public ShoppingItemStatus? Status { get; set; }
    public StoreSection? StoreSection { get; set; }
    public string? SearchTerm { get; set; }

    public ShoppingListFilter()
    {
        SortBy = "StoreSection";
        SortDescending = false;
        PageSize = 100;
    }
}

public class CreateShoppingItemDto
{
    public Guid? IngredientId { get; set; }
    public string Name { get; set; } = string.Empty;
    public double Quantity { get; set; } = 1.0;
    public string Unit { get; set; } = "piece";
    public StoreSection StoreSection { get; set; } = StoreSection.Produce;
    public StockLocation TargetLocation { get; set; } = StockLocation.Pantry;
    public decimal? EstimatedCost { get; set; }
    public string? Notes { get; set; }
    public bool IsShared { get; set; } = true;
}

public class UpdateShoppingItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public double Quantity { get; set; } = 1.0;
    public string Unit { get; set; } = "piece";
    public StoreSection StoreSection { get; set; } = StoreSection.Produce;
    public StockLocation TargetLocation { get; set; } = StockLocation.Pantry;
    public decimal? EstimatedCost { get; set; }
    public string? Notes { get; set; }
}

public class SetShoppingItemStatusDto
{
    public Guid Id { get; set; }
    public ShoppingItemStatus Status { get; set; } = ShoppingItemStatus.ToBuy;
}

public class FinishShoppingDto
{
    public bool IncludeInCartItems { get; set; } = true;
    public string? CustomBatchCode { get; set; }
}

public class AutoGenerateShoppingListDto
{
    public bool IncludeLowStock { get; set; } = true;
    public bool IncludeMealPlan { get; set; } = true;
    public int PlanDaysAhead { get; set; } = 7;
}

public record ShoppingListGroupedDto(
    int TotalItems,
    int ToBuyCount,
    int InCartCount,
    int BoughtCount,
    decimal EstimatedTotalCost,
    Dictionary<StoreSection, List<GroceryShoppingItemDto>> Sections
);
