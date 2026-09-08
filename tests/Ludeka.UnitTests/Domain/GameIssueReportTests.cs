using System;
using Ludeka.Core.Entities;
using Ludeka.Core.Enums;
using Xunit;

namespace Ludeka.UnitTests.Domain;

public class GameIssueReportTests
{
    [Fact]
    public void Constructor_WithValidArguments_InitializesPendingReportCorrectly()
    {
        // Arrange
        var gameId = Guid.NewGuid();

        // Act
        var report = new GameIssueReport(
            gameId: gameId,
            gameSlug: "catan",
            gameTitle: "Catan",
            issueType: GameIssueType.WrongImage,
            details: "La imagen corresponde a una edición en inglés",
            reporterNameOrAlias: "Marta Jugona",
            reportedByUserId: "user-123"
        );

        // Assert
        Assert.NotEqual(Guid.Empty, report.Id);
        Assert.Equal(gameId, report.GameId);
        Assert.Equal("catan", report.GameSlug);
        Assert.Equal("Catan", report.GameTitle);
        Assert.Equal(GameIssueType.WrongImage, report.IssueType);
        Assert.Equal("La imagen corresponde a una edición en inglés", report.Details);
        Assert.Equal("Marta Jugona", report.ReporterNameOrAlias);
        Assert.Equal("user-123", report.ReportedByUserId);
        Assert.Equal(GameReportStatus.Pending, report.Status);
        Assert.Null(report.ModeratorNotes);
        Assert.Null(report.ResolvedByUserId);
        Assert.Null(report.ResolvedAt);
        Assert.True(report.CreatedAt <= DateTimeOffset.UtcNow);
    }

    [Fact]
    public void Constructor_WithoutReporterAlias_DefaultsToAnonymousCommunity()
    {
        // Act
        var report = new GameIssueReport(
            gameId: Guid.NewGuid(),
            gameSlug: "carcassonne",
            gameTitle: "Carcassonne",
            issueType: GameIssueType.BrokenPurchaseLink
        );

        // Assert
        Assert.Equal("Comunidad anónima", report.ReporterNameOrAlias);
        Assert.Null(report.ReportedByUserId);
        Assert.Null(report.Details);
    }

    [Fact]
    public void Constructor_WithEmptyGameId_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() => new GameIssueReport(
            gameId: Guid.Empty,
            gameSlug: "catan",
            gameTitle: "Catan",
            issueType: GameIssueType.WrongImage
        ));
        Assert.Contains("El identificador del juego no puede estar vacío", ex.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithEmptyGameSlug_ThrowsArgumentException(string? invalidSlug)
    {
        var ex = Assert.Throws<ArgumentException>(() => new GameIssueReport(
            gameId: Guid.NewGuid(),
            gameSlug: invalidSlug!,
            gameTitle: "Catan",
            issueType: GameIssueType.WrongImage
        ));
        Assert.Contains("El slug del juego no puede estar vacío", ex.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithEmptyGameTitle_ThrowsArgumentException(string? invalidTitle)
    {
        var ex = Assert.Throws<ArgumentException>(() => new GameIssueReport(
            gameId: Guid.NewGuid(),
            gameSlug: "catan",
            gameTitle: invalidTitle!,
            issueType: GameIssueType.WrongImage
        ));
        Assert.Contains("El título del juego no puede estar vacío", ex.Message);
    }

    [Fact]
    public void MarkAsInReview_UpdatesStatusAndModerator()
    {
        // Arrange
        var report = new GameIssueReport(Guid.NewGuid(), "catan", "Catan", GameIssueType.IncorrectDuration);

        // Act
        report.MarkAsInReview("mod-1", "Comprobando reglamento oficial");

        // Assert
        Assert.Equal(GameReportStatus.InReview, report.Status);
        Assert.Equal("mod-1", report.ResolvedByUserId);
        Assert.Equal("Comprobando reglamento oficial", report.ModeratorNotes);
        Assert.NotNull(report.UpdatedAt);
    }

    [Fact]
    public void MarkAsInReview_WithEmptyModeratorId_ThrowsArgumentException()
    {
        var report = new GameIssueReport(Guid.NewGuid(), "catan", "Catan", GameIssueType.IncorrectDuration);
        var ex = Assert.Throws<ArgumentException>(() => report.MarkAsInReview(" "));
        Assert.Contains("Debe especificarse el moderador que toma el reporte en revisión", ex.Message);
    }

    [Fact]
    public void Resolve_UpdatesStatusAndTimestamps()
    {
        // Arrange
        var report = new GameIssueReport(Guid.NewGuid(), "catan", "Catan", GameIssueType.WrongImage);

        // Act
        report.Resolve("mod-1", "Carátula actualizada a la edición en español");

        // Assert
        Assert.Equal(GameReportStatus.Resolved, report.Status);
        Assert.Equal("mod-1", report.ResolvedByUserId);
        Assert.Equal("Carátula actualizada a la edición en español", report.ModeratorNotes);
        Assert.NotNull(report.ResolvedAt);
        Assert.NotNull(report.UpdatedAt);
    }

    [Fact]
    public void Dismiss_UpdatesStatusAndRequiresReason()
    {
        // Arrange
        var report = new GameIssueReport(Guid.NewGuid(), "catan", "Catan", GameIssueType.IncorrectPlayerCount);

        // Act
        report.Dismiss("mod-2", "El juego base oficial indica 3-4 jugadores");

        // Assert
        Assert.Equal(GameReportStatus.Dismissed, report.Status);
        Assert.Equal("mod-2", report.ResolvedByUserId);
        Assert.Equal("El juego base oficial indica 3-4 jugadores", report.ModeratorNotes);
        Assert.NotNull(report.ResolvedAt);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Dismiss_WithEmptyReason_ThrowsArgumentException(string? invalidReason)
    {
        var report = new GameIssueReport(Guid.NewGuid(), "catan", "Catan", GameIssueType.IncorrectPlayerCount);
        var ex = Assert.Throws<ArgumentException>(() => report.Dismiss("mod-2", invalidReason!));
        Assert.Contains("Debe indicarse el motivo del descarte", ex.Message);
    }

    [Fact]
    public void Reopen_ResetsReportToPending()
    {
        // Arrange
        var report = new GameIssueReport(Guid.NewGuid(), "catan", "Catan", GameIssueType.BrokenImage);
        report.Dismiss("mod-1", "Descarte por error");

        // Act
        report.Reopen();

        // Assert
        Assert.Equal(GameReportStatus.Pending, report.Status);
        Assert.Null(report.ResolvedAt);
        Assert.NotNull(report.UpdatedAt);
    }
}
