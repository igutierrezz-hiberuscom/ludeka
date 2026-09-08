using System;
using System.Collections.Generic;
using Ludeka.Core.Entities;
using Ludeka.Core.Enums;
using Ludeka.Core.ValueObjects;
using Xunit;

namespace Ludeka.UnitTests.Domain;

public class UserGameReviewTests
{
    [Fact]
    public void Constructor_ShouldInitializeCorrectly_WhenDataIsValid()
    {
        // Arrange
        var userId = "user-1";
        var gameId = Guid.NewGuid();
        var votes = new List<UserPlayerCountVote>
        {
            new(2, ScalabilityStatus.MustPlay),
            new(4, ScalabilityStatus.Recommended)
        };
        var family = new UserFamilyExperienceVote(true, 8, true);

        // Act
        var review = new UserGameReview(
            userId,
            gameId,
            8.5,
            "Excelente juego para parejas.",
            votes,
            family,
            PlayContextType.Friends);

        // Assert
        Assert.Equal(8.5, review.Score);
        Assert.Equal("Excelente juego para parejas.", review.MicroReview);
        Assert.Equal(2, review.PlayerCountRatings.Count);
        Assert.NotNull(review.FamilyExperience);
        Assert.Equal(8, review.FamilyExperience.SuggestedMinAge);
        Assert.Equal(PlayContextType.Friends, review.PlayContext);
    }

    [Theory]
    [InlineData(0.9)]
    [InlineData(10.1)]
    [InlineData(-1.0)]
    public void Constructor_ShouldThrowOutOfRangeException_WhenScoreIsOutOfRange(double invalidScore)
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new UserGameReview("user-1", Guid.NewGuid(), invalidScore));
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentException_WhenMicroReviewExceeds280Chars()
    {
        // Arrange
        string longReview = new string('A', 281);

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new UserGameReview("user-1", Guid.NewGuid(), 8.0, longReview));
    }

    [Fact]
    public void Update_ShouldModifyPropertiesAndSetTimestamp()
    {
        // Arrange
        var review = new UserGameReview("user-1", Guid.NewGuid(), 7.0, "Bueno");

        // Act
        review.Update(9.0, "¡Imprescindible tras más partidas!", null, null, PlayContextType.Owned);

        // Assert
        Assert.Equal(9.0, review.Score);
        Assert.Equal("¡Imprescindible tras más partidas!", review.MicroReview);
        Assert.NotNull(review.UpdatedAt);
        Assert.Equal(PlayContextType.Owned, review.PlayContext);
    }

    [Theory]
    [InlineData("8.0", 8.0)]
    [InlineData("8.5", 8.5)]
    [InlineData("8", 8.0)]
    [InlineData("8,5", 8.5)]
    [InlineData("10.0", 10.0)]
    [InlineData("1.0", 1.0)]
    [InlineData("15.0", 10.0)]
    [InlineData("0.5", 1.0)]
    public void ScoreParsing_ShouldPreventCultureMisinterpretation_AndClampAccurately(string rawInput, double expectedScore)
    {
        var normalized = rawInput.Replace(',', '.');
        Assert.True(double.TryParse(normalized, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var parsed));
        var score = Math.Clamp(Math.Round(parsed * 2.0, MidpointRounding.AwayFromZero) / 2.0, 1.0, 10.0);

        var review = new UserGameReview("user-1", Guid.NewGuid(), score);

        Assert.Equal(expectedScore, review.Score);
    }
}
