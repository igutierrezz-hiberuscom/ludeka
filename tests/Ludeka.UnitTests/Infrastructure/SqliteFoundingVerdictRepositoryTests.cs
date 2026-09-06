using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ludeka.Core.Entities;
using Ludeka.Core.Enums;
using Ludeka.Core.ValueObjects;
using Ludeka.Infrastructure.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Ludeka.UnitTests.Infrastructure;

public class SqliteFoundingVerdictRepositoryTests : IAsyncLifetime
{
    private SqliteConnection _connection = null!;
    private LudekaDbContext _context = null!;
    private SqliteFoundingVerdictRepository _repository = null!;

    public async Task InitializeAsync()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        await _connection.OpenAsync();

        var options = new DbContextOptionsBuilder<LudekaDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new LudekaDbContext(options);
        await _context.Database.EnsureCreatedAsync();

        _repository = new SqliteFoundingVerdictRepository(_context);
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
        await _connection.DisposeAsync();
    }

    private async Task<Game> SeedGameAsync()
    {
        var game = new Game(
            bggId: 7001,
            originalTitle: "Wingspan",
            spanishTitle: "Wingspan",
            designer: "Elizabeth Hargrave",
            publisher: "Maldito Games",
            yearPublished: 2019,
            coverImageUrl: "https://example.com/cover.jpg",
            thumbnailUrl: null,
            description: "Juego de aves",
            bggRating: 8.1,
            bggRank: 25,
            ludistRating: 8.1,
            confrontation: ConfrontationType.Competitive,
            style: GameStyle.Eurogame,
            isOfficialSolo: true,
            age: new AgeRating(10, 9),
            language: LanguageDependence.Low,
            footprint: TableFootprint.StandardTable,
            duration: new GameDuration(40, 70, 25)
        );
        _context.Games.Add(game);
        await _context.SaveChangesAsync();
        return game;
    }

    [Fact]
    public async Task AddAndGetByGameId_ShouldPersistAndRetrieveWithPhotosJson()
    {
        var game = await SeedGameAsync();
        var photos = new List<FoundingPhoto>
        {
            new("https://example.com/p1.jpg", "Pie de foto 1"),
            new("https://example.com/p2.jpg", "Pie de foto 2")
        };

        var verdict = new FoundingVerdict(
            game.Id,
            "autor-01",
            "Mesa Fundadora (Lucía)",
            FoundingRecommendation.MustPlay,
            "Análisis general de Wingspan: una experiencia sobresaliente.",
            "A 2 jugadores funciona como un tiro, sin apenas entreturno.",
            "Para jugar en familia con peques a partir de 9 años es fantástico.",
            photos
        );

        await _repository.AddAsync(verdict);

        var retrieved = await _repository.GetByGameIdAsync(game.Id);

        Assert.NotNull(retrieved);
        Assert.Equal(verdict.Id, retrieved.Id);
        Assert.Equal(game.Id, retrieved.GameId);
        Assert.Equal("Mesa Fundadora (Lucía)", retrieved.AuthorName);
        Assert.Equal(FoundingRecommendation.MustPlay, retrieved.Recommendation);
        Assert.Equal(2, retrieved.Photos.Count);
        Assert.Equal("Pie de foto 1", retrieved.Photos[0].Caption);
    }

    [Fact]
    public async Task Update_ShouldModifyVerdictsAndPhotos()
    {
        var game = await SeedGameAsync();
        var verdict = new FoundingVerdict(
            game.Id,
            "autor-01",
            "Mesa Fundadora",
            FoundingRecommendation.RecommendedWithAdaptations,
            "Análisis inicial del título con longitud adecuada.",
            "Análisis a 2 jugadores con longitud adecuada.",
            "Análisis familiar con longitud adecuada."
        );
        await _repository.AddAsync(verdict);

        verdict.Update(
            FoundingRecommendation.MustPlay,
            "Análisis general actualizado y pulido con mayor detalle.",
            "Análisis a 2 jugadores actualizado tras 20 partidas.",
            "Análisis familiar actualizado con peques de 8 años.",
            [new FoundingPhoto("https://example.com/actualizada.jpg", "Nueva foto")]
        );
        await _repository.UpdateAsync(verdict);

        var updated = await _repository.GetByGameIdAsync(game.Id);
        Assert.NotNull(updated);
        Assert.Equal(FoundingRecommendation.MustPlay, updated.Recommendation);
        Assert.Single(updated.Photos);
        Assert.Equal("https://example.com/actualizada.jpg", updated.Photos[0].PhotoUrl);
    }

    [Fact]
    public async Task Delete_ShouldRemoveVerdictFromDatabase()
    {
        var game = await SeedGameAsync();
        var verdict = new FoundingVerdict(
            game.Id,
            "autor-01",
            "Mesa Fundadora",
            FoundingRecommendation.Skippable,
            "Análisis general prescindible para probar borrado.",
            "Análisis a dos jugadores para probar borrado.",
            "Análisis familiar para probar borrado."
        );
        await _repository.AddAsync(verdict);

        await _repository.DeleteAsync(verdict.Id);

        var retrieved = await _repository.GetByGameIdAsync(game.Id);
        Assert.Null(retrieved);
    }
}
