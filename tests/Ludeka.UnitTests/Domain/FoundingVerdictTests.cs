using System;
using System.Collections.Generic;
using Ludeka.Core.Entities;
using Ludeka.Core.Enums;
using Ludeka.Core.ValueObjects;
using Xunit;

namespace Ludeka.UnitTests.Domain;

public class FoundingVerdictTests
{
    [Fact]
    public void Constructor_WithValidData_ShouldCreateFoundingVerdict()
    {
        var gameId = Guid.NewGuid();
        var photos = new List<FoundingPhoto>
        {
            new("https://example.com/foto1.jpg", "Despliegue a 2 jugadores"),
            new("https://example.com/foto2.jpg", "Detalle de los componentes")
        };

        var verdict = new FoundingVerdict(
            gameId,
            "fundador-01",
            "Mesa Fundadora",
            FoundingRecommendation.MustPlay,
            "Un juego extraordinario con producción de lujo y mecánicas adictivas.",
            "A 2 jugadores funciona a la perfección, sin apenas entreturno y máxima tensión.",
            "Para familias con peques a partir de 8 años es una gozada adaptando la primera ronda.",
            photos
        );

        Assert.NotEqual(Guid.Empty, verdict.Id);
        Assert.Equal(gameId, verdict.GameId);
        Assert.Equal("fundador-01", verdict.AuthorUserId);
        Assert.Equal("Mesa Fundadora", verdict.AuthorName);
        Assert.Equal(FoundingRecommendation.MustPlay, verdict.Recommendation);
        Assert.Equal(2, verdict.Photos.Count);
        Assert.Equal(10.0, verdict.GetEquivalentRating());
    }

    [Theory]
    [InlineData("", "Análisis a 2 jugadores válido", "Análisis familiar válido")]
    [InlineData("Corto", "Análisis a 2 jugadores válido", "Análisis familiar válido")]
    [InlineData("Análisis general válido", "", "Análisis familiar válido")]
    [InlineData("Análisis general válido", "Análisis a 2 jugadores válido", "Corto")]
    public void Constructor_WithInvalidVerdicts_ShouldThrowArgumentException(
        string overall, string twoPlayer, string family)
    {
        Assert.Throws<ArgumentException>(() => new FoundingVerdict(
            Guid.NewGuid(),
            "fundador-01",
            "Mesa Fundadora",
            FoundingRecommendation.RecommendedWithAdaptations,
            overall,
            twoPlayer,
            family
        ));
    }

    [Fact]
    public void AddPhoto_MoreThanThreePhotos_ShouldThrowInvalidOperationException()
    {
        var verdict = new FoundingVerdict(
            Guid.NewGuid(),
            "fundador-01",
            "Mesa Fundadora",
            FoundingRecommendation.MustPlay,
            "Análisis general suficientemente extenso para cumplir requisitos.",
            "Análisis enfocado a dos jugadores detallado y riguroso.",
            "Análisis enfocado a familias con niños mayores de diez años.",
            [
                new("https://example.com/1.jpg", "Foto 1"),
                new("https://example.com/2.jpg", "Foto 2"),
                new("https://example.com/3.jpg", "Foto 3")
            ]
        );

        Assert.Equal(3, verdict.Photos.Count);

        var ex = Assert.Throws<InvalidOperationException>(() =>
            verdict.AddPhoto(new FoundingPhoto("https://example.com/4.jpg", "Foto 4")));

        Assert.Contains("No se pueden adjuntar más de 3 fotos", ex.Message);
    }

    [Fact]
    public void RemovePhoto_ShouldRemoveAtSpecifiedIndex()
    {
        var verdict = new FoundingVerdict(
            Guid.NewGuid(),
            "fundador-01",
            "Mesa Fundadora",
            FoundingRecommendation.Skippable,
            "Análisis general de prueba con suficiente longitud mínima.",
            "Análisis dos jugadores con suficiente longitud mínima.",
            "Análisis familiar con suficiente longitud mínima.",
            [
                new("https://example.com/1.jpg", "Foto 1"),
                new("https://example.com/2.jpg", "Foto 2")
            ]
        );

        verdict.RemovePhoto(0);

        Assert.Single(verdict.Photos);
        Assert.Equal("https://example.com/2.jpg", verdict.Photos[0].PhotoUrl);
    }

    [Theory]
    [InlineData(FoundingRecommendation.MustPlay, 10.0)]
    [InlineData(FoundingRecommendation.RecommendedWithAdaptations, 7.5)]
    [InlineData(FoundingRecommendation.Skippable, 4.0)]
    public void GetEquivalentRating_ShouldReturnExpectedScore(
        FoundingRecommendation recommendation, double expectedScore)
    {
        var verdict = new FoundingVerdict(
            Guid.NewGuid(),
            "fundador-01",
            "Mesa Fundadora",
            recommendation,
            "Análisis general de prueba con suficiente longitud mínima.",
            "Análisis dos jugadores con suficiente longitud mínima.",
            "Análisis familiar con suficiente longitud mínima."
        );

        Assert.Equal(expectedScore, verdict.GetEquivalentRating());
    }
}
