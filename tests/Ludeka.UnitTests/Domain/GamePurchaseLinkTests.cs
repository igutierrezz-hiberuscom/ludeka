using System;
using System.Collections.Generic;
using Ludeka.Core.Entities;
using Ludeka.Core.Enums;
using Ludeka.Core.ValueObjects;
using Xunit;

namespace Ludeka.UnitTests.Domain;

public class GamePurchaseLinkTests
{
    private static Game CreateSampleGame(IEnumerable<GamePurchaseLink>? purchaseLinks = null)
    {
        return new Game(
            bggId: 266192,
            originalTitle: "Wingspan",
            spanishTitle: "Wingspan",
            designer: "Elizabeth Hargrave",
            publisher: "Maldito Games",
            yearPublished: 2019,
            coverImageUrl: "https://example.com/wingspan.jpg",
            thumbnailUrl: "https://example.com/wingspan-thumb.jpg",
            description: "Juego de aves.",
            bggRating: 8.1,
            bggRank: 25,
            ludistRating: 8.5,
            confrontation: ConfrontationType.Competitive,
            style: GameStyle.Eurogame,
            isOfficialSolo: true,
            age: new AgeRating(10, 10),
            language: LanguageDependence.Low,
            footprint: TableFootprint.StandardTable,
            duration: new GameDuration(40, 70, 25),
            purchaseLinks: purchaseLinks
        );
    }

    [Fact]
    public void GamePurchaseLink_ShouldInstantiateProperly_WithValidValues()
    {
        // Act
        var link = new GamePurchaseLink(
            storeName: "Zacatrus",
            affiliateUrl: "https://zacatrus.es/wingspan.html?ref=ludeka",
            price: 49.95m,
            currency: "€",
            inStock: true,
            badge: "Envío 24h",
            affiliateTag: "Direct"
        );

        // Assert
        Assert.Equal("Zacatrus", link.StoreName);
        Assert.Equal("https://zacatrus.es/wingspan.html?ref=ludeka", link.AffiliateUrl);
        Assert.Equal(49.95m, link.Price);
        Assert.Equal("€", link.Currency);
        Assert.True(link.InStock);
        Assert.Equal("Envío 24h", link.Badge);
        Assert.Equal("Direct", link.AffiliateTag);
        Assert.True(link.FormattedPrice == "49,95 €" || link.FormattedPrice == "49.95 €");
    }

    [Theory]
    [InlineData("", "https://example.com")]
    [InlineData("   ", "https://example.com")]
    [InlineData(null, "https://example.com")]
    public void GamePurchaseLink_ShouldThrowException_WhenStoreNameIsInvalid(string? invalidName, string url)
    {
        Assert.Throws<ArgumentException>(() => new GamePurchaseLink(invalidName!, url));
    }

    [Theory]
    [InlineData("Zacatrus", "")]
    [InlineData("Zacatrus", "   ")]
    [InlineData("Zacatrus", null)]
    public void GamePurchaseLink_ShouldThrowException_WhenAffiliateUrlIsInvalid(string store, string? invalidUrl)
    {
        Assert.Throws<ArgumentException>(() => new GamePurchaseLink(store, invalidUrl!));
    }

    [Fact]
    public void FormattedPrice_ShouldReturnConsultar_WhenPriceIsNull()
    {
        var link = new GamePurchaseLink("Cuarto de Juegos", "https://cuartodejuegos.es/item");
        Assert.Null(link.Price);
        Assert.Equal("Consultar", link.FormattedPrice);
    }

    [Fact]
    public void Game_ShouldInitializeWithEmptyPurchaseLinks_WhenOmittedInConstructor()
    {
        var game = CreateSampleGame();
        Assert.NotNull(game.PurchaseLinks);
        Assert.Empty(game.PurchaseLinks);
    }

    [Fact]
    public void Game_ShouldAddAndManagePurchaseLinksCorrectly()
    {
        var game = CreateSampleGame();

        var link1 = new GamePurchaseLink("Zacatrus", "https://zacatrus.es/wingspan?ref=ludeka", 49.95m, badge: "Envío 24h");
        var link2 = new GamePurchaseLink("Amazon", "https://amazon.es/dp/B07MZT?tag=ludeka-21", 52.00m);

        // Act - Add
        game.AddPurchaseLink(link1);
        Assert.Single(game.PurchaseLinks);
        Assert.Equal("Zacatrus", game.PurchaseLinks[0].StoreName);

        // Act - Update batch
        game.UpdatePurchaseLinks(new[] { link1, link2 });
        Assert.Equal(2, game.PurchaseLinks.Count);
        Assert.Equal("Amazon", game.PurchaseLinks[1].StoreName);

        // Act - Clear
        game.ClearPurchaseLinks();
        Assert.Empty(game.PurchaseLinks);
    }
}
