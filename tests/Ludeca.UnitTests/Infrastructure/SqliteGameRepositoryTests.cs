using System;
using System.Linq;
using System.Threading.Tasks;
using Ludeca.Application.DTOs;
using Ludeca.Core.Enums;
using Ludeca.Infrastructure.Data;
using Ludeca.Infrastructure.Seeding;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Ludeca.UnitTests.Infrastructure;

public class SqliteGameRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly LudecaDbContext _context;
    private readonly SqliteGameRepository _repository;

    public SqliteGameRepositoryTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<LudecaDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new LudecaDbContext(options);
        _context.Database.EnsureCreated();

        _repository = new SqliteGameRepository(_context);
    }

    [Fact]
    public async Task CatalogSeeder_ShouldSeedCuratedGamesIdempotently()
    {
        // Act
        int firstSeed = await CatalogSeeder.SeedAsync(_context);
        int secondSeed = await CatalogSeeder.SeedAsync(_context);

        // Assert
        Assert.True(firstSeed > 0);
        Assert.Equal(0, secondSeed); // Idempotente
        Assert.True(await _repository.HasAnyAsync());
    }

    [Fact]
    public async Task GetBySlugAsync_ShouldRetrieveGameWithScalabilityAndSleeves()
    {
        // Arrange
        await CatalogSeeder.SeedAsync(_context);

        // Act
        var wingspan = await _repository.GetBySlugAsync("wingspan");

        // Assert
        Assert.NotNull(wingspan);
        Assert.Equal("Wingspan", wingspan.SpanishTitle);
        Assert.NotEmpty(wingspan.Scalability);
        Assert.NotEmpty(wingspan.Sleeves);
        Assert.Equal("Ideal: 2-3 jugadores", wingspan.IdealPlayerCountText);
    }

    [Fact]
    public async Task SearchAsync_EspecialParejas_ShouldReturnOnlyGamesMustPlayAtTwoPlayers()
    {
        // Arrange
        await CatalogSeeder.SeedAsync(_context);

        // Act
        var criteria = new GameFilterCriteria(EspecialParejas: true);
        var (items, total) = await _repository.SearchAsync(criteria, page: 1, pageSize: 50);

        // Assert
        Assert.True(total > 0);
        foreach (var game in items)
        {
            var twoPlayers = game.Scalability.FirstOrDefault(s => s.PlayerCount == 2);
            Assert.NotNull(twoPlayers);
            Assert.Equal(ScalabilityStatus.MustPlay, twoPlayers.Status);
        }
    }

    [Fact]
    public async Task SearchAsync_SearchTerm_ShouldFindGameBySpanishOrOriginalTitle()
    {
        // Arrange
        await CatalogSeeder.SeedAsync(_context);

        // Act
        var (items, total) = await _repository.SearchAsync(new GameFilterCriteria(SearchTerm: "Castillos de Borgoña"));

        // Assert
        Assert.True(total >= 1);
        Assert.Contains(items, g => g.Slug == "los-castillos-de-borgona");
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }
}
