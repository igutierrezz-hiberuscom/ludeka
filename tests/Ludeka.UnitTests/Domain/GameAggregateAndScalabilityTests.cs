using System.Collections.Generic;
using Ludeka.Core.Entities;
using Ludeka.Core.Enums;
using Ludeka.Core.ValueObjects;
using Xunit;

namespace Ludeka.UnitTests.Domain;

public class GameAggregateAndScalabilityTests
{
    [Fact]
    public void Game_CalculateIdealPlayerCountText_ShouldReturnSingleCount_WhenOnlyOneMustPlay()
    {
        // Arrange: 7 Wonders Duel (exclusivo 2 jugadores)
        var game = CreateSampleGame("7 Wonders: Duel", "7 Wonders: Duel",
            new List<ScalabilityEntry>
            {
                new(1, "1J", ScalabilityStatus.NotRecommended, BestVotes: 0, RecommendedVotes: 2, NotRecommendedVotes: 50),
                new(2, "2J", ScalabilityStatus.MustPlay, BestVotes: 450, RecommendedVotes: 10, NotRecommendedVotes: 2)
            });

        // Act & Assert
        Assert.Equal("Ideal: 2 jugadores", game.IdealPlayerCountText);
    }

    [Fact]
    public void Game_CalculateIdealPlayerCountText_ShouldReturnRange_WhenConsecutiveMustPlay()
    {
        // Arrange: Wingspan (ideal a 2 y 3 jugadores)
        var game = CreateSampleGame("Wingspan", "Wingspan",
            new List<ScalabilityEntry>
            {
                new(1, "1J", ScalabilityStatus.Recommended, BestVotes: 15, RecommendedVotes: 60, NotRecommendedVotes: 10),
                new(2, "2J", ScalabilityStatus.MustPlay, BestVotes: 180, RecommendedVotes: 30, NotRecommendedVotes: 5),
                new(3, "3J", ScalabilityStatus.MustPlay, BestVotes: 160, RecommendedVotes: 35, NotRecommendedVotes: 4),
                new(4, "4J", ScalabilityStatus.Recommended, BestVotes: 40, RecommendedVotes: 110, NotRecommendedVotes: 25),
                new(5, "5J", ScalabilityStatus.NotRecommended, BestVotes: 5, RecommendedVotes: 20, NotRecommendedVotes: 90)
            });

        // Act & Assert
        Assert.Equal("Ideal: 2-3 jugadores", game.IdealPlayerCountText);
    }

    [Theory]
    [InlineData(100, 20, 5, ScalabilityStatus.MustPlay)]
    [InlineData(20, 80, 10, ScalabilityStatus.Recommended)]
    [InlineData(5, 10, 80, ScalabilityStatus.NotRecommended)]
    [InlineData(0, 0, 0, ScalabilityStatus.NotRecommended)]
    public void CalculateStatus_FromVotes_ShouldDetermineCorrectTrafficLightStatus(
        int best, int recommended, int notRecommended, ScalabilityStatus expectedStatus)
    {
        // Act
        var status = ScalabilityCalculator.DetermineStatus(best, recommended, notRecommended);

        // Assert
        Assert.Equal(expectedStatus, status);
    }

    [Fact]
    public void Game_ShouldInstantiateWithCorrectAttributesAndInvariants()
    {
        // Arrange
        var game = CreateSampleGame("Terraforming Mars", "Terraforming Mars", null);

        // Assert
        Assert.Equal("terraforming-mars", game.Slug);
        Assert.Equal(ConfrontationType.Competitive, game.Confrontation);
        Assert.Equal(GameStyle.Eurogame, game.Style);
        Assert.True(game.Age.IsAccessibleEarlier);
    }

    private static Game CreateSampleGame(string originalTitle, string spanishTitle, List<ScalabilityEntry>? scalability)
    {
        return new Game(
            bggId: 266192,
            originalTitle: originalTitle,
            spanishTitle: spanishTitle,
            designer: "Elizabeth Hargrave",
            publisher: "Maldito Games",
            yearPublished: 2019,
            coverImageUrl: "https://cf.geekdo-images.com/sample.jpg",
            thumbnailUrl: "https://cf.geekdo-images.com/sample_t.jpg",
            description: "Juego de aves competitivo con motor de cartas.",
            bggRating: 8.1,
            bggRank: 25,
            ludistRating: 8.5,
            confrontation: ConfrontationType.Competitive,
            style: GameStyle.Eurogame,
            isOfficialSolo: true,
            age: new AgeRating(BoxAge: 14, CommunityAge: 10),
            language: LanguageDependence.Low,
            footprint: TableFootprint.StandardTable,
            duration: new GameDuration(40, 70, 25),
            scalability: scalability
        );
    }
}
