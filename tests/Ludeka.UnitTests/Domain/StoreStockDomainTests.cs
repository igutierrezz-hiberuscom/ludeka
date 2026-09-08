using System;
using Ludeka.Core.Enums;
using Ludeka.Core.ValueObjects;
using Xunit;

namespace Ludeka.UnitTests.Domain;

public class StoreStockDomainTests
{
    [Fact]
    public void StoreStockInfo_Defaults_ShouldBeUnknownAndNotChecking()
    {
        var info = new StoreStockInfo();

        Assert.Equal(StockStatus.Unknown, info.Status);
        Assert.True(info.IsUnknown);
        Assert.False(info.IsInStock);
        Assert.False(info.IsLowStock);
        Assert.False(info.IsOutOfStock);
        Assert.False(info.IsChecking);
        Assert.Null(info.AvailableQuantity);
        Assert.Null(info.CurrentPrice);
        Assert.Null(info.LastCheckedUtc);
        Assert.Equal("Sin comprobar", info.GetRelativeTimeText());
    }

    [Fact]
    public void StoreStockInfo_Checking_ShouldSetIsCheckingTrue()
    {
        var info = StoreStockInfo.Checking();

        Assert.True(info.IsChecking);
        Assert.Equal(StockStatus.Unknown, info.Status);
    }

    [Fact]
    public void StoreStockInfo_InStock_ShouldSetPropertiesProperly()
    {
        var info = StoreStockInfo.InStock(quantity: 5, price: 39.95m, note: "Unidades en almacén central");

        Assert.Equal(StockStatus.InStock, info.Status);
        Assert.True(info.IsInStock);
        Assert.False(info.IsLowStock);
        Assert.False(info.IsOutOfStock);
        Assert.False(info.IsUnknown);
        Assert.Equal(5, info.AvailableQuantity);
        Assert.Equal(39.95m, info.CurrentPrice);
        Assert.Equal("Unidades en almacén central", info.StatusNote);
        Assert.NotNull(info.LastCheckedUtc);
    }

    [Fact]
    public void StoreStockInfo_LowStock_ShouldSetPropertiesProperly()
    {
        var info = StoreStockInfo.LowStock(quantity: 1, price: 45.00m, note: "Última unidad disponible");

        Assert.Equal(StockStatus.LowStock, info.Status);
        Assert.True(info.IsLowStock);
        Assert.False(info.IsInStock);
        Assert.False(info.IsOutOfStock);
        Assert.Equal(1, info.AvailableQuantity);
    }

    [Fact]
    public void StoreStockInfo_OutOfStock_ShouldSetPropertiesProperly()
    {
        var info = StoreStockInfo.OutOfStock(note: "Agotado en distribuidor");

        Assert.Equal(StockStatus.OutOfStock, info.Status);
        Assert.True(info.IsOutOfStock);
        Assert.False(info.IsInStock);
        Assert.Equal("Agotado en distribuidor", info.StatusNote);
    }

    [Fact]
    public void StoreStockInfo_GetRelativeTimeText_ShouldFormatCorrectly()
    {
        var baseTime = new DateTime(2026, 9, 8, 10, 0, 0, DateTimeKind.Utc);

        var justNow = new StoreStockInfo
        {
            Status = StockStatus.InStock,
            LastCheckedUtc = baseTime.AddSeconds(-30)
        };
        Assert.Equal("Comprobado hace instantes", justNow.GetRelativeTimeText(baseTime));

        var fiveMinutesAgo = new StoreStockInfo
        {
            Status = StockStatus.InStock,
            LastCheckedUtc = baseTime.AddMinutes(-5)
        };
        Assert.Equal("Comprobado hace 5 min", fiveMinutesAgo.GetRelativeTimeText(baseTime));

        var twoHoursAgo = new StoreStockInfo
        {
            Status = StockStatus.InStock,
            LastCheckedUtc = baseTime.AddHours(-2)
        };
        Assert.Equal("Comprobado hace 2 h", twoHoursAgo.GetRelativeTimeText(baseTime));

        var yesterday = new StoreStockInfo
        {
            Status = StockStatus.InStock,
            LastCheckedUtc = baseTime.AddDays(-1)
        };
        Assert.Equal($"Comprobado el {yesterday.LastCheckedUtc.Value:dd/MM}", yesterday.GetRelativeTimeText(baseTime));
    }

    [Fact]
    public void GamePurchaseLink_InitialStockStatus_ShouldReflectInStockBoolean()
    {
        var linkInStock = new GamePurchaseLink(
            storeName: "Zacatrus",
            affiliateUrl: "https://zacatrus.es/ark-nova.html",
            price: 59.95m,
            inStock: true
        );

        var linkOutOfStock = new GamePurchaseLink(
            storeName: "Cuarto de Juegos",
            affiliateUrl: "https://cuartodejuegos.com/ark-nova.html",
            price: 59.95m,
            inStock: false
        );

        Assert.Equal(StockStatus.InStock, linkInStock.InitialStockStatus);
        Assert.Equal(StockStatus.OutOfStock, linkOutOfStock.InitialStockStatus);
    }
}
