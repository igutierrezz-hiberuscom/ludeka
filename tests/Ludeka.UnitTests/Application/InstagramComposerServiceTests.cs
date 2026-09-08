using System;
using Ludeka.Application.DTOs;
using Ludeka.Application.Features.Community;
using Ludeka.Application.Features.Instagram;
using Ludeka.Core.Entities;
using Ludeka.Core.Enums;
using Ludeka.Core.ValueObjects;
using Xunit;

namespace Ludeka.UnitTests.Application;

public class InstagramComposerServiceTests
{
    private readonly InstagramComposerService _composer = new(new SocialCardService());

    [Fact]
    public void ComposeSvg_FromGiveaway_DarkTheme_GeneratesValidCard()
    {
        // Arrange
        var giveaway = new Giveaway(
            title: "Sorteo Voidfall Edición Galáctica",
            organizer: "Maldito Games",
            url: "https://instagram.com/p/voidfall123",
            platform: GiveawayPlatform.Instagram,
            deadlineAt: DateTimeOffset.UtcNow.AddDays(5),
            collaborator: "El Dado Único");

        // Act
        var svg = _composer.ComposeSvg(InstagramPostSourceType.Giveaway, giveaway, "Dark");
        var caption = _composer.GenerateCaption(InstagramPostSourceType.Giveaway, giveaway);

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(svg));
        Assert.Contains("viewBox=\"0 0 1080 1080\"", svg);
        Assert.Contains("RADAR DE SORTEOS", svg);
        Assert.Contains("Sorteo Activo", svg);
        Assert.Contains("Voidfall", svg);
        Assert.Contains("Maldito Games", svg);
        Assert.Contains("Ludeka", svg);

        Assert.False(string.IsNullOrWhiteSpace(caption));
        Assert.Contains("¡SORTEO ACTIVO!", caption);
        Assert.Contains("Voidfall", caption);
        Assert.Contains("@malditogames", caption);
        Assert.Contains("#sorteojuegos", caption);
        Assert.Contains("#ludeka", caption);
    }

    [Fact]
    public void ComposeSvg_FromGiveaway_LightTheme_GeneratesLightPalette()
    {
        // Arrange
        var giveaway = new Giveaway(
            title: "Sorteo Sky Team",
            organizer: "Scorpion Masqué",
            url: "https://instagram.com/p/skyteam",
            platform: GiveawayPlatform.Instagram,
            deadlineAt: DateTimeOffset.UtcNow.AddDays(3));

        // Act
        var svg = _composer.ComposeSvg(InstagramPostSourceType.Giveaway, giveaway, "Light");

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(svg));
        Assert.Contains("#F8FAFC", svg); // Background claro
    }

    [Fact]
    public void ComposeSvg_FromWeeklyRelease_GeneratesValidCardAndCaption()
    {
        // Arrange
        var release = new WeeklyRelease(
            title: "Harmonies",
            publisher: "Libellud",
            releaseDate: new DateOnly(2026, 9, 20),
            coverImageUrl: "https://cf.geekdo-images.com/harmonies.jpg");

        // Act
        var svg = _composer.ComposeSvg(InstagramPostSourceType.WeeklyRelease, release, "Dark");
        var caption = _composer.GenerateCaption(InstagramPostSourceType.WeeklyRelease, release);

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(svg));
        Assert.Contains("NOVEDADES", svg);
        Assert.Contains("Novedad Editorial", svg);
        Assert.Contains("Harmonies", svg);
        Assert.Contains("Libellud", svg);

        Assert.False(string.IsNullOrWhiteSpace(caption));
        Assert.Contains("¡NOVEDAD EDITORIAL!", caption);
        Assert.Contains("Harmonies", caption);
        Assert.Contains("@libellud", caption);
        Assert.Contains("#novedadesludicas", caption);
        Assert.Contains("#ludeka", caption);
    }

    [Fact]
    public void ComposeSvg_FromGame_GeneratesValidCardAndCaption()
    {
        // Arrange
        var game = new Game(
            bggId: 224517,
            originalTitle: "Brass: Birmingham",
            spanishTitle: "Brass: Birmingham",
            designer: "Martin Wallace",
            publisher: "Maldito Games",
            yearPublished: 2018,
            coverImageUrl: "https://cf.geekdo-images.com/brass.jpg",
            thumbnailUrl: "https://cf.geekdo-images.com/brass_thumb.jpg",
            description: "Juego de estrategia de la era industrial",
            bggRating: 8.6,
            bggRank: 1,
            ludistRating: 8.6,
            confrontation: ConfrontationType.Competitive,
            style: GameStyle.Eurogame,
            isOfficialSolo: false,
            age: new AgeRating(14, 12),
            language: LanguageDependence.Low,
            footprint: TableFootprint.TableMonster,
            duration: new GameDuration(60, 120, 30));

        // Act
        var svg = _composer.ComposeSvg(InstagramPostSourceType.Game, game, "Dark");
        var caption = _composer.GenerateCaption(InstagramPostSourceType.Game, game);

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(svg));
        Assert.Contains("Ludeka", svg);
        Assert.Contains("Brass: Birmingham", svg);
        Assert.Contains("8.6", svg);

        Assert.False(string.IsNullOrWhiteSpace(caption));
        Assert.Contains("Brass: Birmingham (2018)", caption);
        Assert.Contains("8.6 / 10", caption);
        Assert.Contains("#juegosdemesa", caption);
    }
}
