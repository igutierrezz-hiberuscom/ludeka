using System;
using System.Linq;
using System.Threading.Tasks;
using Ludeka.Core.Entities;
using Ludeka.Infrastructure.Data;
using Ludeka.Infrastructure.Seeding;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Ludeka.UnitTests.Infrastructure;

public class SqliteGamePlayLogRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly LudekaDbContext _context;
    private readonly SqliteGamePlayLogRepository _repository;

    public SqliteGamePlayLogRepositoryTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<LudekaDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new LudekaDbContext(options);
        _context.Database.EnsureCreated();

        _repository = new SqliteGamePlayLogRepository(_context);
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
    public async Task GetByUserAndGameAsync_ShouldNotThrowAndOrderByPlayDateDescending()
    {
        // Arrange: dos partidas el mismo día verifican el desempate por CreatedAt (desc),
        // la tercera tiene una PlayDate anterior y debe quedar última.
        var (gameA, _) = await SeedTwoGamesAsync();
        const string userId = "auth0|user-1";

        var earlier = new GamePlayLog(userId, gameA.Id, new DateTimeOffset(2024, 3, 1, 20, 0, 0, TimeSpan.Zero), "En casa", 3);
        await _repository.AddAsync(earlier);

        var sameDayFirst = new GamePlayLog(userId, gameA.Id, new DateTimeOffset(2024, 3, 5, 21, 30, 0, TimeSpan.Zero), "Club", 4);
        await _repository.AddAsync(sameDayFirst);

        // Garantiza CreatedAt estrictamente posterior para el desempate determinista.
        await Task.Delay(5);

        var sameDaySecond = new GamePlayLog(userId, gameA.Id, new DateTimeOffset(2024, 3, 5, 21, 30, 0, TimeSpan.Zero), "Club", 4);
        await _repository.AddAsync(sameDaySecond);

        // Act
        var result = await _repository.GetByUserAndGameAsync(userId, gameA.Id);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal(
            new[] { sameDaySecond.Id, sameDayFirst.Id, earlier.Id },
            result.Select(p => p.Id).ToArray());
        // La navegación Game del Include debe estar materializada.
        Assert.All(result, p => Assert.NotNull(p.Game));
    }

    [Fact]
    public async Task GetByUserAndGameAsync_ShouldFilterByUserAndGame()
    {
        // Arrange: logs cruzados de dos usuarios y dos juegos.
        var (gameA, gameB) = await SeedTwoGamesAsync();
        const string userId = "auth0|user-1";
        const string otherUserId = "auth0|user-2";

        var expected = new GamePlayLog(userId, gameA.Id, new DateTimeOffset(2024, 3, 1, 20, 0, 0, TimeSpan.Zero), "En casa", 2);
        await _repository.AddAsync(expected);
        await _repository.AddAsync(new GamePlayLog(otherUserId, gameA.Id, new DateTimeOffset(2024, 3, 2, 20, 0, 0, TimeSpan.Zero), "Club", 3));
        await _repository.AddAsync(new GamePlayLog(userId, gameB.Id, new DateTimeOffset(2024, 3, 3, 20, 0, 0, TimeSpan.Zero), "En casa", 5));

        // Act
        var result = await _repository.GetByUserAndGameAsync(userId, gameA.Id);
        var emptyForPairWithoutLogs = await _repository.GetByUserAndGameAsync(otherUserId, gameB.Id);

        // Assert
        Assert.Single(result);
        Assert.Equal(expected.Id, result[0].Id);
        Assert.Equal(expected.PlayDate, result[0].PlayDate);
        // Pareja usuario+juego sin logs: la consulta corrió y no encontró nada.
        Assert.Empty(emptyForPairWithoutLogs);
    }
}
