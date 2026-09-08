using System;
using System.Linq;
using System.Threading.Tasks;
using Ludeka.Core.Entities;
using Ludeka.Core.Enums;
using Ludeka.Infrastructure.Data;
using Ludeka.Infrastructure.Seeding;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Ludeka.UnitTests.Infrastructure;

public class SqliteUserCollectionRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly LudekaDbContext _context;
    private readonly SqliteUserCollectionRepository _repository;

    public SqliteUserCollectionRepositoryTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<LudekaDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new LudekaDbContext(options);
        _context.Database.EnsureCreated();

        _repository = new SqliteUserCollectionRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    private async Task<(Game GameA, Game GameB)> SeedTwoGamesAsync()
    {
        await CatalogSeeder.SeedAsync(_context);
        var games = _context.Games.OrderBy(g => g.Slug).Take(2).ToList();
        return (games[0], games[1]);
    }

    [Fact]
    public async Task GetPlayedByUserIdAsync_ShouldNotThrowAndOrderByAddedAtDescending()
    {
        // Arrange: dos items jugados con AddedAt crecientes (el campo se fija en el constructor,
        // por eso se espacian con Task.Delay) en juegos distintos; un item no jugado y uno de
        // otro usuario deben quedar excluidos. La navegacion Game debe estar materializada.
        var (gameA, gameB) = await SeedTwoGamesAsync();
        const string userId = "auth0|user-1";
        const string otherUserId = "auth0|user-2";

        var olderPlayed = new UserCollectionItem(userId, gameB.Id, CollectionStatus.InCollection, isPlayed: true);
        await _repository.AddAsync(olderPlayed);
        await Task.Delay(5);

        var newestPlayed = new UserCollectionItem(userId, gameA.Id, CollectionStatus.Played);
        await _repository.AddAsync(newestPlayed);

        var notPlayed = new UserCollectionItem(userId, gameA.Id, CollectionStatus.InCollection);
        await _repository.AddAsync(notPlayed);

        await _repository.AddAsync(new UserCollectionItem(otherUserId, gameA.Id, CollectionStatus.InCollection, isPlayed: true));

        // Act
        var result = await _repository.GetPlayedByUserIdAsync(userId);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal(
            new[] { newestPlayed.Id, olderPlayed.Id },
            result.Select(c => c.Id).ToArray());
        // La navegacion Game del Include debe estar materializada.
        Assert.All(result, c => Assert.NotNull(c.Game));
    }

    [Fact]
    public async Task GetPlayedByUserIdAsync_ShouldReturnEmptyForUserWithoutPlayedItems()
    {
        // Arrange: el usuario objetivo solo tiene un item no jugado; el item jugado sembrado
        // pertenece a otro usuario.
        var (gameA, _) = await SeedTwoGamesAsync();
        const string userId = "auth0|user-1";
        const string otherUserId = "auth0|user-2";

        await _repository.AddAsync(new UserCollectionItem(userId, gameA.Id, CollectionStatus.InCollection));
        await _repository.AddAsync(new UserCollectionItem(otherUserId, gameA.Id, CollectionStatus.Played));

        // Act
        var result = await _repository.GetPlayedByUserIdAsync(userId);

        // Assert: la consulta corrio, filtro por usuario y estado jugado sin encontrar nada.
        Assert.Empty(result);
    }
}
