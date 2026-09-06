using System;
using Ludeca.Core.Entities;
using Ludeca.Core.Enums;
using Ludeca.Core.ValueObjects;
using Xunit;

namespace Ludeca.UnitTests.Domain;

public class ValueObjectsAndSlugTests
{
    [Theory]
    [InlineData("Wingspan", "wingspan")]
    [InlineData("Los Castillos de Borgoña", "los-castillos-de-borgona")]
    [InlineData("¡Aventureros al Tren! Europa", "aventureros-al-tren-europa")]
    [InlineData("7 Wonders: Duel", "7-wonders-duel")]
    [InlineData("Catán: El Juego", "catan-el-juego")]
    [InlineData("Terraforming Mars (Expansión)", "terraforming-mars-expansion")]
    public void GenerateSlug_ShouldNormalizeAndRemoveDiacriticsAndPunctuation(string input, string expectedSlug)
    {
        // Act
        string slug = Game.GenerateSlug(input);

        // Assert
        Assert.Equal(expectedSlug, slug);
    }

    [Fact]
    public void AgeRating_IsAccessibleEarlier_ShouldBeTrue_WhenCommunityAgeIsLowerThanBoxAge()
    {
        // Arrange
        var rating = new AgeRating(BoxAge: 14, CommunityAge: 8);

        // Act & Assert
        Assert.True(rating.IsAccessibleEarlier);
    }

    [Fact]
    public void AgeRating_IsAccessibleEarlier_ShouldBeFalse_WhenCommunityAgeIsEqualOrHigherThanBoxAge()
    {
        // Arrange
        var equalRating = new AgeRating(BoxAge: 10, CommunityAge: 10);
        var higherRating = new AgeRating(BoxAge: 8, CommunityAge: 10);

        // Act & Assert
        Assert.False(equalRating.IsAccessibleEarlier);
        Assert.False(higherRating.IsAccessibleEarlier);
    }

    [Fact]
    public void GameDuration_ShouldCalculateEstimatedTextCorrectly()
    {
        // Arrange
        var duration = new GameDuration(MinMinutes: 40, MaxMinutes: 70, EstimatedPerPlayerMinutes: 30);

        // Act
        string text = duration.FormatForPlayerCount(2);

        // Assert
        Assert.Contains("~60 min a 2 jugadores", text);
    }

    [Fact]
    public void SleeveItem_CalculatePacksNeeded_ShouldRoundUpToNearestPackSize()
    {
        // Arrange: 110 cartas, paquetes estándar de 50 fundas
        var sleeve = new SleeveItem("Estándar", 63.5, 88.0, 110, "https://tienda.com/fundas");

        // Act
        int packs = sleeve.CalculatePacksNeeded(packSize: 50);

        // Assert
        Assert.Equal(3, packs); // 110 / 50 = 2.2 -> 3 paquetes
    }

    [Fact]
    public void ScalabilityEntry_ShouldStoreVotesAndStatusCorrectly()
    {
        // Arrange
        var entry = new ScalabilityEntry(
            PlayerCount: 2,
            DisplayCount: "2J",
            Status: ScalabilityStatus.MustPlay,
            BestVotes: 120,
            RecommendedVotes: 15,
            NotRecommendedVotes: 3
        );

        // Assert
        Assert.Equal(2, entry.PlayerCount);
        Assert.Equal("2J", entry.DisplayCount);
        Assert.Equal(ScalabilityStatus.MustPlay, entry.Status);
        Assert.Equal(138, entry.TotalVotes);
    }
}
