using System;
using Ludeka.Core.Entities;
using Ludeka.Core.Enums;
using Xunit;

namespace Ludeka.UnitTests.Domain;

public class GiveawayTests
{
    [Fact]
    public void Constructor_ValidArguments_CreatesGiveaway()
    {
        // Arrange
        var deadline = DateTimeOffset.UtcNow.AddDays(5);

        // Act
        var giveaway = new Giveaway(
            title: "Sorteo Brass Birmingham",
            organizer: "Maldito Games",
            url: "https://instagram.com/p/12345",
            platform: GiveawayPlatform.Instagram,
            deadlineAt: deadline,
            collaborator: "Análisis Parálisis");

        // Assert
        Assert.NotEqual(Guid.Empty, giveaway.Id);
        Assert.Equal("Sorteo Brass Birmingham", giveaway.Title);
        Assert.Equal("Maldito Games", giveaway.Organizer);
        Assert.Equal("Análisis Parálisis", giveaway.Collaborator);
        Assert.Equal("Maldito Games en colaboración con Análisis Parálisis", giveaway.FormattedOrganizer);
        Assert.False(giveaway.IsExpired);
    }

    [Fact]
    public void MergeCollaborator_AddsCollaboratorCorrectly()
    {
        // Arrange
        var giveaway = new Giveaway(
            title: "Sorteo Dune Imperium",
            organizer: "Asmodee ES",
            url: "https://instagram.com/p/abc",
            platform: GiveawayPlatform.Instagram,
            deadlineAt: DateTimeOffset.UtcNow.AddDays(3));

        // Act 1: Initial merge
        giveaway.MergeCollaborator("El Rincón Legacy");

        // Assert 1
        Assert.Equal("El Rincón Legacy", giveaway.Collaborator);
        Assert.Equal("Asmodee ES en colaboración con El Rincón Legacy", giveaway.FormattedOrganizer);

        // Act 2: Secondary merge
        giveaway.MergeCollaborator("Mesa de Juegos");

        // Assert 2
        Assert.Equal("El Rincón Legacy y Mesa de Juegos", giveaway.Collaborator);

        // Act 3: Duplicate merge should not duplicate
        giveaway.MergeCollaborator("El Rincón Legacy");
        Assert.Equal("El Rincón Legacy y Mesa de Juegos", giveaway.Collaborator);
    }

    [Fact]
    public void IsExpired_ReturnsTrue_WhenPastDeadline()
    {
        // Arrange
        var pastDeadline = DateTimeOffset.UtcNow.AddHours(-2);
        var giveaway = new Giveaway(
            title: "Sorteo Pasado",
            organizer: "Devir",
            url: "https://devir.es/sorteo",
            platform: GiveawayPlatform.Community,
            deadlineAt: pastDeadline);

        // Assert
        Assert.True(giveaway.IsExpired);
    }

    [Fact]
    public void ExtendDeadline_ValidNewDate_UpdatesDeadline()
    {
        // Arrange
        var initial = DateTimeOffset.UtcNow.AddDays(1);
        var extended = DateTimeOffset.UtcNow.AddDays(7);
        var giveaway = new Giveaway(
            title: "Sorteo Ampliado",
            organizer: "Devir",
            url: "https://devir.es/sorteo",
            platform: GiveawayPlatform.TwitterX,
            deadlineAt: initial);

        // Act
        giveaway.ExtendDeadline(extended);

        // Assert
        Assert.Equal(extended, giveaway.DeadlineAt);
    }

    [Fact]
    public void ExtendDeadline_PastDate_ThrowsInvalidOperationException()
    {
        // Arrange
        var initial = DateTimeOffset.UtcNow.AddDays(5);
        var invalidDate = DateTimeOffset.UtcNow.AddDays(2);
        var giveaway = new Giveaway(
            title: "Sorteo",
            organizer: "Devir",
            url: "https://devir.es",
            platform: GiveawayPlatform.Instagram,
            deadlineAt: initial);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => giveaway.ExtendDeadline(invalidDate));
    }

    [Fact]
    public void SetPromoted_SetsIsPromotedFlagCorrectly()
    {
        // Arrange
        var giveaway = new Giveaway(
            title: "Sorteo Destacado",
            organizer: "Maldito Games",
            url: "https://malditogames.com/sorteo",
            platform: GiveawayPlatform.Instagram,
            deadlineAt: DateTimeOffset.UtcNow.AddDays(10),
            isPromoted: false);

        Assert.False(giveaway.IsPromoted);

        // Act
        giveaway.SetPromoted(true);

        // Assert
        Assert.True(giveaway.IsPromoted);
        Assert.NotNull(giveaway.UpdatedAt);
    }
}
