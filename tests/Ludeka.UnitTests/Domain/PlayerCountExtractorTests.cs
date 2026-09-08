using System;
using System.Collections.Generic;
using Ludeka.Core.Entities;
using Ludeka.Core.Enums;
using Ludeka.Core.Helpers;
using Ludeka.Core.ValueObjects;
using Xunit;

namespace Ludeka.UnitTests.Domain;

public class PlayerCountExtractorTests
{
    [Theory]
    [InlineData("Partida en solitario a Terraforming Mars", "Partida en solitario")]
    [InlineData("Gameplay modo solo Wingspan", "Partida en solitario")]
    [InlineData("Ark Nova jugado en solitario", "Partida en solitario")]
    [InlineData("Catan para 1 jugador reglas automa", "Partida en solitario")]
    [InlineData("Partida a 1 jugador en directo", "Partida en solitario")]
    public void ExtractPlayerBadge_WithSolitaryKeywords_ShouldReturnPartidaEnSolitario(string title, string expected)
    {
        var result = PlayerCountExtractor.ExtractPlayerBadge(title);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("Partida a 2 jugadores a Wingspan", "Partida a 2")]
    [InlineData("Gameplay a 3 comensales de Catan", "Partida a 3")]
    [InlineData("Partida a 4 de Carcassonne completa", "Partida a 4")]
    [InlineData("Duelo a 2 a 7 Wonders Duel", "Partida a 2")]
    public void ExtractPlayerBadge_WithNumericPlayers_ShouldReturnFormattedCount(string title, string expected)
    {
        var result = PlayerCountExtractor.ExtractPlayerBadge(title);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("Partida a dos jugadores en pareja", "Partida a 2")]
    [InlineData("Gameplay a tres jugadores", "Partida a 3")]
    [InlineData("Partida a cuatro en mesa", "Partida a 4")]
    public void ExtractPlayerBadge_WithTextualNumbers_ShouldDetectCount(string title, string expected)
    {
        var result = PlayerCountExtractor.ExtractPlayerBadge(title);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ExtractPlayerBadge_WithDuelKeyword_ShouldReturnPartidaA2()
    {
        var result = PlayerCountExtractor.ExtractPlayerBadge("Duelo épico en directo");
        Assert.Equal("Partida a 2", result);
    }

    [Fact]
    public void ExtractPlayerBadge_WithNoMatch_ShouldFallbackToGameScalability()
    {
        var game = CreateTestGame(
            title: "Brass Birmingham",
            scalability:
            [
                new ScalabilityEntry(2, "2", ScalabilityStatus.Recommended, 10, 50, 5),
                new ScalabilityEntry(3, "3", ScalabilityStatus.Recommended, 20, 60, 2),
                new ScalabilityEntry(4, "4", ScalabilityStatus.MustPlay, 80, 20, 1)
            ]
        );

        var result = PlayerCountExtractor.ExtractPlayerBadge("Brass Birmingham partida completa", null, game);
        Assert.Equal("Partida a 4", result);
    }

    [Fact]
    public void ExtractPlayerBadge_WithNoMatchAndNoGame_ShouldFallbackToPartidaA2()
    {
        var result = PlayerCountExtractor.ExtractPlayerBadge("Partida completa sin números");
        Assert.Equal("Partida a 2", result);
    }

    [Fact]
    public void ExtractPlayerBadge_WhenUsedInMediaItem_ShouldNotThrowException()
    {
        var badge = PlayerCountExtractor.ExtractPlayerBadge("Vídeo de partida sin especificar");
        
        var media = new MediaItem(
            type: MediaType.Playthrough,
            platform: MediaPlatform.YouTube,
            title: "Vídeo de partida sin especificar",
            url: "https://www.youtube.com/watch?v=yP5J9q6P4Jg",
            thumbnailUrl: "https://img.youtube.com/vi/yP5J9q6P4Jg/hqdefault.jpg",
            authorChannel: "Análisis Parálisis",
            playerCountBadge: badge
        );

        Assert.Equal("Partida a 2", media.PlayerCountBadge);
        Assert.Equal(MediaType.Playthrough, media.Type);
    }

    private static Game CreateTestGame(string title, List<ScalabilityEntry> scalability)
    {
        return new Game(
            bggId: 12345,
            originalTitle: title,
            spanishTitle: title,
            designer: "Autor Test",
            publisher: "Editorial Test",
            yearPublished: 2020,
            coverImageUrl: "https://example.com/cover.jpg",
            thumbnailUrl: "https://example.com/thumb.jpg",
            description: "Descripción de prueba",
            bggRating: 8.5,
            bggRank: 1,
            ludistRating: 9.0,
            confrontation: ConfrontationType.Competitive,
            style: GameStyle.Eurogame,
            isOfficialSolo: false,
            age: new AgeRating(14, 14),
            language: LanguageDependence.Low,
            footprint: TableFootprint.StandardTable,
            duration: new GameDuration(60, 120, 30),
            scalability: scalability
        );
    }
}
