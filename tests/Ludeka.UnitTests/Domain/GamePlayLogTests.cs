using System;
using Ludeka.Core.Entities;
using Xunit;

namespace Ludeka.UnitTests.Domain;

public class GamePlayLogTests
{
    [Fact]
    public void Constructor_InitializesCorrectly_WithValidParameters()
    {
        // Arrange
        string userId = "user-test";
        Guid gameId = Guid.NewGuid();
        var playDate = DateTimeOffset.UtcNow.AddDays(-1);
        string location = "Club Lúdico";
        int playerCount = 4;
        int durationMinutes = 75;
        string comment = "Gran partida competitiva con final de infarto";

        // Act
        var log = new GamePlayLog(userId, gameId, playDate, location, playerCount, durationMinutes, comment);

        // Assert
        Assert.NotEqual(Guid.Empty, log.Id);
        Assert.Equal(userId, log.UserId);
        Assert.Equal(gameId, log.GameId);
        Assert.Equal(playDate, log.PlayDate);
        Assert.Equal(location, log.Location);
        Assert.Equal(playerCount, log.PlayerCount);
        Assert.Equal(durationMinutes, log.DurationMinutes);
        Assert.Equal(comment, log.Comment);
        Assert.True(log.CreatedAt <= DateTimeOffset.UtcNow);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ThrowsArgumentException_WhenUserIdIsInvalid(string? invalidUserId)
    {
        Assert.Throws<ArgumentException>(() => new GamePlayLog(invalidUserId!, Guid.NewGuid(), DateTimeOffset.UtcNow, "Casa", 2));
    }

    [Fact]
    public void Constructor_ThrowsArgumentException_WhenGameIdIsEmpty()
    {
        Assert.Throws<ArgumentException>(() => new GamePlayLog("user-1", Guid.Empty, DateTimeOffset.UtcNow, "Casa", 2));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-5)]
    public void Constructor_ThrowsArgumentException_WhenPlayerCountIsLessThanOne(int invalidPlayerCount)
    {
        Assert.Throws<ArgumentException>(() => new GamePlayLog("user-1", Guid.NewGuid(), DateTimeOffset.UtcNow, "Casa", invalidPlayerCount));
    }

    [Fact]
    public void Constructor_DefaultsLocationToEnCasa_WhenEmptyOrWhitespace()
    {
        var log = new GamePlayLog("user-1", Guid.NewGuid(), DateTimeOffset.UtcNow, "   ", 2);
        Assert.Equal("En casa", log.Location);
    }

    [Fact]
    public void Update_ModifiesPropertiesCorrectly()
    {
        // Arrange
        var log = new GamePlayLog("user-1", Guid.NewGuid(), DateTimeOffset.UtcNow.AddDays(-2), "Casa", 2);

        // Act
        var newDate = DateTimeOffset.UtcNow;
        log.Update(newDate, "Asociación El Troquel", 5, 90, "Jugado con expansión");

        // Assert
        Assert.Equal(newDate, log.PlayDate);
        Assert.Equal("Asociación El Troquel", log.Location);
        Assert.Equal(5, log.PlayerCount);
        Assert.Equal(90, log.DurationMinutes);
        Assert.Equal("Jugado con expansión", log.Comment);
    }
}
