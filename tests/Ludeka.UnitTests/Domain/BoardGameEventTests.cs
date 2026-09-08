using System;
using Ludeka.Core.Entities;
using Xunit;

namespace Ludeka.UnitTests.Domain;

public class BoardGameEventTests
{
    [Fact]
    public void Constructor_ValidArguments_InitializesCorrectly()
    {
        // Arrange & Act
        var evt = new BoardGameEvent(
            title: "Festival Internacional de Juegos de Córdoba",
            description: "Fiesta del juego de mesa en el Palacio de la Merced.",
            imageUrl: "https://example.com/cordoba.jpg",
            startDate: new DateOnly(2026, 10, 9),
            endDate: new DateOnly(2026, 10, 12),
            location: "Palacio de la Merced, Córdoba",
            websiteUrl: "https://festivaldejuegoscordoba.es",
            organizer: "Jugamos Tod@s",
            isOfficial: true);

        // Assert
        Assert.NotEqual(Guid.Empty, evt.Id);
        Assert.Equal("Festival Internacional de Juegos de Córdoba", evt.Title);
        Assert.Equal("Fiesta del juego de mesa en el Palacio de la Merced.", evt.Description);
        Assert.Equal("https://example.com/cordoba.jpg", evt.ImageUrl);
        Assert.Equal(new DateOnly(2026, 10, 9), evt.StartDate);
        Assert.Equal(new DateOnly(2026, 10, 12), evt.EndDate);
        Assert.Equal("Palacio de la Merced, Córdoba", evt.Location);
        Assert.Equal("https://festivaldejuegoscordoba.es", evt.WebsiteUrl);
        Assert.Equal("Jugamos Tod@s", evt.Organizer);
        Assert.True(evt.IsOfficial);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_InvalidTitle_ThrowsArgumentException(string? invalidTitle)
    {
        Assert.Throws<ArgumentException>(() => new BoardGameEvent(
            title: invalidTitle!,
            description: "Desc",
            imageUrl: "https://example.com/img.jpg",
            startDate: new DateOnly(2026, 10, 9),
            endDate: new DateOnly(2026, 10, 12),
            location: "Córdoba"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_InvalidImageUrl_ThrowsArgumentException(string? invalidImage)
    {
        Assert.Throws<ArgumentException>(() => new BoardGameEvent(
            title: "Evento",
            description: "Desc",
            imageUrl: invalidImage!,
            startDate: new DateOnly(2026, 10, 9),
            endDate: new DateOnly(2026, 10, 12),
            location: "Córdoba"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_InvalidLocation_ThrowsArgumentException(string? invalidLocation)
    {
        Assert.Throws<ArgumentException>(() => new BoardGameEvent(
            title: "Evento",
            description: "Desc",
            imageUrl: "https://example.com/img.jpg",
            startDate: new DateOnly(2026, 10, 9),
            endDate: new DateOnly(2026, 10, 12),
            location: invalidLocation!));
    }

    [Fact]
    public void Constructor_EndDateBeforeStartDate_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new BoardGameEvent(
            title: "Evento Inválido",
            description: "Desc",
            imageUrl: "https://example.com/img.jpg",
            startDate: new DateOnly(2026, 10, 12),
            endDate: new DateOnly(2026, 10, 9),
            location: "Córdoba"));
    }

    [Fact]
    public void IsOngoing_DateWithinRange_ReturnsTrue()
    {
        var evt = new BoardGameEvent(
            "InterOcio",
            "Feria en Madrid",
            "https://example.com/interocio.jpg",
            new DateOnly(2027, 3, 12),
            new DateOnly(2027, 3, 14),
            "Madrid");

        Assert.True(evt.IsOngoing(new DateOnly(2027, 3, 12)));
        Assert.True(evt.IsOngoing(new DateOnly(2027, 3, 13)));
        Assert.True(evt.IsOngoing(new DateOnly(2027, 3, 14)));
        Assert.False(evt.IsOngoing(new DateOnly(2027, 3, 11)));
        Assert.False(evt.IsOngoing(new DateOnly(2027, 3, 15)));
    }

    [Fact]
    public void IsPast_DateAfterEndDate_ReturnsTrue()
    {
        var evt = new BoardGameEvent(
            "InterOcio",
            "Feria en Madrid",
            "https://example.com/interocio.jpg",
            new DateOnly(2027, 3, 12),
            new DateOnly(2027, 3, 14),
            "Madrid");

        Assert.False(evt.IsPast(new DateOnly(2027, 3, 14)));
        Assert.True(evt.IsPast(new DateOnly(2027, 3, 15)));
    }

    [Fact]
    public void DaysUntilStart_CalculatesCorrectDifference()
    {
        var evt = new BoardGameEvent(
            "Essen SPIEL",
            "Feria mundial",
            "https://example.com/spiel.jpg",
            new DateOnly(2026, 10, 22),
            new DateOnly(2026, 10, 25),
            "Essen, Alemania");

        int days = evt.DaysUntilStart(new DateOnly(2026, 10, 12));
        Assert.Equal(10, days);
    }

    [Fact]
    public void GetFormattedDates_SameMonth_FormatsRangeNicely()
    {
        var evt = new BoardGameEvent(
            "Córdoba",
            "Festival",
            "https://example.com/c.jpg",
            new DateOnly(2026, 10, 9),
            new DateOnly(2026, 10, 12),
            "Córdoba");

        Assert.Equal("9-12 Oct 2026", evt.GetFormattedDates());
    }

    [Fact]
    public void Update_ValidArguments_UpdatesPropertiesCorrectly()
    {
        var evt = new BoardGameEvent(
            "Título Anterior",
            "Desc Anterior",
            "https://example.com/anterior.jpg",
            new DateOnly(2026, 10, 1),
            new DateOnly(2026, 10, 2),
            "Sevilla");

        evt.Update(
            "Nuevo Título",
            "Nueva Desc",
            "https://example.com/nuevo.jpg",
            new DateOnly(2026, 10, 5),
            new DateOnly(2026, 10, 8),
            "Málaga",
            "https://malaga.es",
            "Asoc Málaga",
            true);

        Assert.Equal("Nuevo Título", evt.Title);
        Assert.Equal("Nueva Desc", evt.Description);
        Assert.Equal("https://example.com/nuevo.jpg", evt.ImageUrl);
        Assert.Equal(new DateOnly(2026, 10, 5), evt.StartDate);
        Assert.Equal(new DateOnly(2026, 10, 8), evt.EndDate);
        Assert.Equal("Málaga", evt.Location);
        Assert.Equal("https://malaga.es", evt.WebsiteUrl);
        Assert.NotNull(evt.UpdatedAt);
    }
}
