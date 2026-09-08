using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Ludeka.Core.Enums;
using Ludeka.Infrastructure.Data;
using Ludeka.Infrastructure.Seeding;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Ludeka.UnitTests.Infrastructure;

public class CatalogSeederFullDatasetTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly LudekaDbContext _dbContext;

    public CatalogSeederFullDatasetTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<LudekaDbContext>()
            .UseSqlite(_connection)
            .Options;

        _dbContext = new LudekaDbContext(options);
        _dbContext.Database.EnsureCreated();
    }

    [Fact]
    public async Task SeedAsync_Seeds30BaseGamesAnd10Expansions_Total40Titles()
    {
        int seededCount = await CatalogSeeder.SeedAsync(_dbContext);

        Assert.Equal(31, seededCount);

        var allGames = await _dbContext.Games.ToListAsync();
        var baseGames = allGames.Where(g => g.Type == GameType.BaseGame).ToList();
        var expansions = allGames.Where(g => g.Type == GameType.Expansion || g.Type == GameType.StandaloneExpansion).ToList();

        Assert.Equal(31, baseGames.Count);
        Assert.Equal(10, expansions.Count);
        Assert.Equal(41, allGames.Count);

        // Comprobar que todas las expansiones tienen un BaseGameId válido y asignado
        foreach (var exp in expansions)
        {
            Assert.NotNull(exp.BaseGameId);
            Assert.Contains(baseGames, bg => bg.Id == exp.BaseGameId.Value);
            Assert.NotNull(exp.ExpansionNecessity);
            Assert.NotEmpty(exp.ImpactTags);
            Assert.False(string.IsNullOrWhiteSpace(exp.WhatItBringsSummary));
        }

        // Comprobar que las 3 nuevas expansiones existen
        Assert.Contains(expansions, e => e.BggId == 202976); // 7 Wonders Duel: Pantheon
        Assert.Contains(expansions, e => e.BggId == 342035); // Dune: Imperium - El Auge de Ix
        Assert.Contains(expansions, e => e.BggId == 265492); // Everdell: Bellfaire

        // Comprobar recetas de mesa semilladas
        var recipes = await _dbContext.ExpansionRecipes.ToListAsync();
        Assert.NotEmpty(recipes);
        Assert.Contains(recipes, r => r.Name == "La Era de los Dioses");
        Assert.Contains(recipes, r => r.Name == "Guerra Tecnológica en Arrakis");
        Assert.Contains(recipes, r => r.Name == "El Gran Festival de Bellfaire");
    }

    [Fact]
    public async Task SeedAsync_IsIdempotent_DoesNotDuplicateOnSecondRun()
    {
        await CatalogSeeder.SeedAsync(_dbContext);
        int initialCount = await _dbContext.Games.CountAsync();

        // Segunda ejecución sobre base de datos ya semillada
        int secondSeeded = await CatalogSeeder.SeedAsync(_dbContext);

        int finalCount = await _dbContext.Games.CountAsync();

        Assert.Equal(0, secondSeeded);
        Assert.Equal(initialCount, finalCount);
    }

    [Fact]
    public async Task SeedAsync_WhenDatabaseHasLegacySubset_AddsMissingGamesAndExpansionsIncrementally()
    {
        // Simular base de datos preexistente que solo contenía Catan
        var legacyCatan = new Ludeka.Core.Entities.Game(
            bggId: 13,
            originalTitle: "Catan",
            spanishTitle: "Catán",
            designer: "Klaus Teuber",
            publisher: "Devir",
            yearPublished: 1995,
            coverImageUrl: "/images/games/catan-old.png",
            thumbnailUrl: "/images/games/catan-old.png",
            description: "Descripción previa...",
            bggRating: 7.14,
            bggRank: 450,
            ludistRating: 7.3,
            confrontation: ConfrontationType.Competitive,
            style: GameStyle.Eurogame,
            isOfficialSolo: false,
            age: new Ludeka.Core.ValueObjects.AgeRating(10, 10),
            language: LanguageDependence.None,
            footprint: TableFootprint.StandardTable,
            duration: new Ludeka.Core.ValueObjects.GameDuration(60, 90, 25),
            scalability: [],
            sleeves: [],
            purchaseLinks: []
        );

        await _dbContext.Games.AddAsync(legacyCatan);
        await _dbContext.SaveChangesAsync();

        Assert.Equal(1, await _dbContext.Games.CountAsync());

        // Ejecutar SeedAsync sobre base de datos preexistente
        int seededAdded = await CatalogSeeder.SeedAsync(_dbContext);

        Assert.Equal(30, seededAdded); // 31 en catálogo total menos el que ya existía = 30 nuevos juegos base insertados

        var allGames = await _dbContext.Games.ToListAsync();
        var baseGames = allGames.Where(g => g.Type == GameType.BaseGame).ToList();
        var expansions = allGames.Where(g => g.Type == GameType.Expansion || g.Type == GameType.StandaloneExpansion).ToList();

        Assert.Equal(31, baseGames.Count);
        Assert.Equal(10, expansions.Count);
        Assert.Equal(41, allGames.Count);

        // Verificar que Catan actualizó sus carátulas pero no se duplicó
        var catanMatches = allGames.Where(g => g.BggId == 13).ToList();
        Assert.Single(catanMatches);
        Assert.Equal("/images/games/catan.png", catanMatches[0].CoverImageUrl);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _connection.Dispose();
    }
}
