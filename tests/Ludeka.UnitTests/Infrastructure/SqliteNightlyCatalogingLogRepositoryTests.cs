using System;
using System.Linq;
using System.Threading.Tasks;
using Ludeka.Core.Entities;
using Ludeka.Infrastructure.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Ludeka.UnitTests.Infrastructure;

public class SqliteNightlyCatalogingLogRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly LudekaDbContext _context;
    private readonly SqliteNightlyCatalogingLogRepository _repository;

    public SqliteNightlyCatalogingLogRepositoryTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<LudekaDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new LudekaDbContext(options);
        _context.Database.EnsureCreated();

        _repository = new SqliteNightlyCatalogingLogRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public async Task GetRecentLogsAsync_ShouldNotThrowAndOrderByStartedAtDescendingWithLimit()
    {
        // Arrange: tres ejecuciones con StartedAt explicitos (el constructor acepta el valor,
        // por eso no hace falta Task.Delay); el limite debe recortar la mas antigua.
        var oldest = new NightlyCatalogingExecutionLog(new DateTimeOffset(2024, 3, 1, 2, 0, 0, TimeSpan.Zero));
        var middle = new NightlyCatalogingExecutionLog(new DateTimeOffset(2024, 3, 2, 2, 0, 0, TimeSpan.Zero));
        var newest = new NightlyCatalogingExecutionLog(new DateTimeOffset(2024, 3, 3, 2, 0, 0, TimeSpan.Zero));
        await _repository.AddAsync(oldest);
        await _repository.AddAsync(middle);
        await _repository.AddAsync(newest);

        // Act
        var result = await _repository.GetRecentLogsAsync(limit: 2);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal(
            new[] { newest.Id, middle.Id },
            result.Select(l => l.Id).ToArray());
    }

    [Fact]
    public async Task GetLatestLogAsync_ShouldReturnLogWithMostRecentStartedAt()
    {
        // Arrange: dos ejecuciones con StartedAt distintos; la mas reciente debe ganar.
        var older = new NightlyCatalogingExecutionLog(new DateTimeOffset(2024, 3, 1, 2, 0, 0, TimeSpan.Zero));
        var newer = new NightlyCatalogingExecutionLog(new DateTimeOffset(2024, 3, 5, 3, 30, 0, TimeSpan.Zero));
        await _repository.AddAsync(older);
        await _repository.AddAsync(newer);

        // Act
        var latest = await _repository.GetLatestLogAsync();

        // Assert
        Assert.NotNull(latest);
        Assert.Equal(newer.Id, latest.Id);
        Assert.Equal(newer.StartedAt, latest.StartedAt);
    }

    [Fact]
    public async Task GetRecentLogsAsync_AndGetLatestLogAsync_ShouldHandleEmptyLogTable()
    {
        // Arrange: tabla de bitacora vacia; ambas consultas deben correr sin lanzar.

        // Act
        var recentLogs = await _repository.GetRecentLogsAsync();
        var latestLog = await _repository.GetLatestLogAsync();

        // Assert: sin ejecuciones registradas la consulta corrio y no encontro nada.
        Assert.Empty(recentLogs);
        Assert.Null(latestLog);
    }
}
