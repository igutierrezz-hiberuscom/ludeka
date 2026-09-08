using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ludeka.Core.Entities;
using Ludeka.Core.Enums;
using Ludeka.Core.ValueObjects;
using Ludeka.Infrastructure.Data;
using Ludeka.Infrastructure.Seeding;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Ludeka.UnitTests.Infrastructure;

public class SqlitePurchaseLinksPersistenceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly LudekaDbContext _context;

    public SqlitePurchaseLinksPersistenceTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<LudekaDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new LudekaDbContext(options);
        _context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public async Task Seeder_ShouldPopulatePurchaseLinksForPreloadedGames()
    {
        // Act
        await CatalogSeeder.SeedAsync(_context);

        // Assert
        var wingspan = await _context.Games.FirstOrDefaultAsync(g => g.Slug == "wingspan");
        Assert.NotNull(wingspan);
        Assert.NotEmpty(wingspan.PurchaseLinks);

        var zacatrusOffer = wingspan.PurchaseLinks.FirstOrDefault(l => l.StoreName == "Zacatrus");
        Assert.NotNull(zacatrusOffer);
        Assert.Equal(49.95m, zacatrusOffer.Price);
        Assert.Contains("ref=ludeka", zacatrusOffer.AffiliateUrl);
        Assert.True(zacatrusOffer.InStock);
    }

    [Fact]
    public async Task DbContext_ShouldPersistAndRetrieveCustomPurchaseLinksCorrectly()
    {
        // Arrange
        var game = new Game(
            bggId: 999999,
            originalTitle: "Test Game",
            spanishTitle: "Juego de Prueba",
            designer: "Autor Test",
            publisher: "Editorial Test",
            yearPublished: 2026,
            coverImageUrl: null,
            thumbnailUrl: null,
            description: "Descripción de prueba",
            bggRating: 7.5,
            bggRank: 100,
            ludistRating: 8.0,
            confrontation: ConfrontationType.Competitive,
            style: GameStyle.Eurogame,
            isOfficialSolo: true,
            age: new AgeRating(10, 10),
            language: LanguageDependence.None,
            footprint: TableFootprint.StandardTable,
            duration: new GameDuration(30, 60, 20),
            purchaseLinks: new List<GamePurchaseLink>
            {
                new("Tienda A", "https://tienda-a.com/game?ref=ludeka", 39.99m, "€", true, "Envío 24h", "Direct"),
                new("Tienda B", "https://tienda-b.com/game?ref=ludeka", 42.50m, "€", false, "Reserva", "Partner")
            }
        );

        _context.Games.Add(game);
        await _context.SaveChangesAsync();

        // Act - Recuperar en un nuevo DbContext sobre la misma conexión
        var options = new DbContextOptionsBuilder<LudekaDbContext>()
            .UseSqlite(_connection)
            .Options;

        using var queryContext = new LudekaDbContext(options);
        var loaded = await queryContext.Games.FirstOrDefaultAsync(g => g.BggId == 999999);

        // Assert
        Assert.NotNull(loaded);
        Assert.Equal(2, loaded.PurchaseLinks.Count);

        var first = loaded.PurchaseLinks[0];
        Assert.Equal("Tienda A", first.StoreName);
        Assert.Equal(39.99m, first.Price);
        Assert.Equal("Envío 24h", first.Badge);
        Assert.True(first.InStock);

        var second = loaded.PurchaseLinks[1];
        Assert.Equal("Tienda B", second.StoreName);
        Assert.Equal(42.50m, second.Price);
        Assert.False(second.InStock);
    }

    [Fact]
    public async Task SqliteSchemaMigrator_ShouldEnsurePurchaseLinksColumnExists()
    {
        // Act - Ejecutar reconciliación sobre la base de datos creada
        await SqliteSchemaMigrator.EnsureSchemaUpToDateAsync(_context);

        // Comprobar con PRAGMA table_info que la columna PurchaseLinks existe
        var cmd = _connection.CreateCommand();
        cmd.CommandText = "PRAGMA table_info('Games');";
        using var reader = await cmd.ExecuteReaderAsync();

        bool found = false;
        while (await reader.ReadAsync())
        {
            if (string.Equals(reader.GetString(1), "PurchaseLinks", StringComparison.OrdinalIgnoreCase))
            {
                found = true;
                break;
            }
        }

        Assert.True(found, "La columna PurchaseLinks debe existir tras la reconciliación del esquema.");
    }
}
