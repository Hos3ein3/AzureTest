using System;
using System.Collections.Generic;
using System.Linq;
using AzureTest.Domain.Documents.Food;
using AzureTest.Domain.Documents.Food.Enums;
using FluentAssertions;
using Xunit;

namespace AzureTest.UnitTest.Food;

public class StockAndShoppingDomainTests
{
    [Fact]
    public void PantryStockItem_ExpiryChecks_ShouldCorrectlyIdentifyExpiredAndExpiringSoon()
    {
        var now = DateTime.UtcNow;
        var expiredItem = PantryStockItemDocument.Create(
            Guid.NewGuid(), Guid.NewGuid(), "Yogurt", StockLocation.Fridge, 200, "g", 200,
            expiryDate: now.AddDays(-1)
        );

        var expiringSoonItem = PantryStockItemDocument.Create(
            Guid.NewGuid(), Guid.NewGuid(), "Fresh Milk", StockLocation.Fridge, 1000, "ml", 1030,
            expiryDate: now.AddDays(2)
        );

        var freshItem = PantryStockItemDocument.Create(
            Guid.NewGuid(), Guid.NewGuid(), "Canned Beans", StockLocation.Pantry, 400, "g", 400,
            expiryDate: now.AddDays(90)
        );

        expiredItem.IsExpired(now).Should().BeTrue();
        expiringSoonItem.IsExpired(now).Should().BeFalse();
        expiringSoonItem.IsExpiringSoon(3, now).Should().BeTrue();
        freshItem.IsExpiringSoon(3, now).Should().BeFalse();
    }

    [Fact]
    public void PantryStockItem_ParLevelCheck_ShouldIdentifyLowStockCorrectly()
    {
        var stock = PantryStockItemDocument.Create(
            Guid.NewGuid(), Guid.NewGuid(), "Eggs", StockLocation.Fridge, 3, "piece", 150,
            minimumParLevel: 6
        );

        stock.IsBelowParLevel().Should().BeTrue();

        stock.CurrentQuantity = 12;
        stock.IsBelowParLevel().Should().BeFalse();
    }

    [Fact]
    public void KitchenStaple_WithoutMeasurement_ShouldEvaluateStatusProperly()
    {
        var salt = PantryStockItemDocument.Create(
            Guid.NewGuid(), Guid.NewGuid(), "Kosher Salt", StockLocation.SpiceRack, 1, "piece", 0,
            isStaple: true
        );
        salt.StapleStatus = StapleStatus.Low;

        salt.IsBelowParLevel().Should().BeTrue();

        salt.StapleStatus = StapleStatus.InStock;
        salt.IsBelowParLevel().Should().BeFalse();
    }

    [Fact]
    public void FEFO_Sorting_ShouldPrioritizeOldestExpiringBatchesFirst()
    {
        var now = DateTime.UtcNow;
        var ingId = Guid.NewGuid();

        var batch1 = PantryStockItemDocument.Create(Guid.NewGuid(), ingId, "Greek Yogurt", StockLocation.Fridge, 1, "piece", 170, expiryDate: now.AddDays(10));
        var batch2 = PantryStockItemDocument.Create(Guid.NewGuid(), ingId, "Greek Yogurt", StockLocation.Fridge, 1, "piece", 170, expiryDate: now.AddDays(2));
        var batch3 = PantryStockItemDocument.Create(Guid.NewGuid(), ingId, "Greek Yogurt", StockLocation.Fridge, 1, "piece", 170, expiryDate: now.AddDays(5));

        var lots = new List<PantryStockItemDocument> { batch1, batch2, batch3 };

        var sortedFefo = lots.OrderBy(x => x.ExpiryDate ?? DateTime.MaxValue).ToList();

        sortedFefo[0].ExpiryDate.Should().Be(batch2.ExpiryDate);
        sortedFefo[1].ExpiryDate.Should().Be(batch3.ExpiryDate);
        sortedFefo[2].ExpiryDate.Should().Be(batch1.ExpiryDate);
    }

    [Fact]
    public void StockMovement_Reversal_PreservesLedgerAuditTrail()
    {
        var userId = Guid.NewGuid();
        var stockId = Guid.NewGuid();
        var ingId = Guid.NewGuid();
        string correlationId = "MealLog:101:202";

        // Initial consumption movement (-200g)
        var consumption = StockMovementDocument.Create(
            userId, stockId, ingId, "Chicken Breast", StockMovementType.MealLogConsumption,
            quantityDelta: -200, unit: "g", normalizedGramsDelta: -200,
            correlationId: correlationId
        );

        // Compensating reversal movement (+200g)
        var reversal = StockMovementDocument.Create(
            userId, stockId, ingId, "Chicken Breast", StockMovementType.RestockReturn,
            quantityDelta: Math.Abs(consumption.QuantityDelta), unit: "g",
            normalizedGramsDelta: Math.Abs(consumption.NormalizedGramsDelta),
            correlationId: $"Reversal:{correlationId}"
        );

        consumption.QuantityDelta.Should().Be(-200);
        reversal.QuantityDelta.Should().Be(200);
        (consumption.QuantityDelta + reversal.QuantityDelta).Should().Be(0);
    }

    [Fact]
    public void PartialPackages_TrackingInBaseUnits_ShouldReflectRemainingQuantity()
    {
        // Buy 1 Liter (1000 ml) of Whole Milk
        var milk = PantryStockItemDocument.Create(
            Guid.NewGuid(), Guid.NewGuid(), "Whole Milk", StockLocation.Fridge,
            quantity: 1000, unit: "ml", normalizedGrams: 1030
        );

        // Pour 200 ml into coffee
        double used = 200;
        milk.CurrentQuantity -= used;
        milk.CurrentNormalizedGrams -= (used * 1.03);

        milk.CurrentQuantity.Should().Be(800);
        milk.CurrentNormalizedGrams.Should().Be(824);
        milk.IsDepleted.Should().BeFalse();
    }

    [Fact]
    public void MultiUserShoppingList_RealTimeSync_AttributesAndStatusShouldPersistCorrectly()
    {
        var aliceId = Guid.NewGuid();
        var bobId = Guid.NewGuid();

        // Alice creates a shared shopping list item
        var item = ShoppingListItemDocument.Create(
            userId: aliceId,
            name: "Greek Yogurt",
            quantity: 2,
            unit: "container",
            storeSection: StoreSection.DairyAndEggs,
            source: ShoppingItemSource.Manual,
            isShared: true,
            createdByUserName: "Alice"
        );

        item.IsShared.Should().BeTrue();
        item.CreatedByUserName.Should().Be("Alice");
        item.Status.Should().Be(ShoppingItemStatus.ToBuy);

        // Bob (at the store) checks it off into cart
        item.Status = ShoppingItemStatus.InCart;
        item.LastModifiedByUserName = "Bob";
        item.UpdatedAt = DateTime.UtcNow;

        item.Status.Should().Be(ShoppingItemStatus.InCart);
        item.LastModifiedByUserName.Should().Be("Bob");
    }
}
