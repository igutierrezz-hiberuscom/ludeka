using System.Threading.Tasks;
using Ludeka.Core.Entities;
using Ludeka.Core.Enums;
using Ludeka.Infrastructure.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Ludeka.UnitTests.Infrastructure;

public class SqlitePendingBggImportRepositoryTests : IAsyncLifetime
{
    private SqliteConnection _connection = null!;
    private LudekaDbContext _context = null!;
    private SqlitePendingBggImportRepository _repository = null!;

    public async Task InitializeAsync()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        await _connection.OpenAsync();

        var options = new DbContextOptionsBuilder<LudekaDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new LudekaDbContext(options);
        await _context.Database.EnsureCreatedAsync();

        _repository = new SqlitePendingBggImportRepository(_context);
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
        await _connection.DisposeAsync();
    }

    [Fact]
    public async Task AddAndGetByBggIdAsync_WorksCorrectly()
    {
        // Arrange
        var item = new PendingBggImport(342942, "Ark Nova", 2021, "https://example.com/thumb.jpg");

        // Act
        await _repository.AddAsync(item);
        var retrieved = await _repository.GetByBggIdAsync(342942);

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal(342942, retrieved.BggId);
        Assert.Equal("Ark Nova", retrieved.Title);
        Assert.Equal(CatalogQueueStatus.Pending, retrieved.Status);
    }

    [Fact]
    public async Task GetTopPendingAsync_ReturnsOrderedByRequestedCountDescending()
    {
        // Arrange
        var item1 = new PendingBggImport(1, "Juego A"); // count = 1
        var item2 = new PendingBggImport(2, "Juego B");
        item2.IncrementRequestCount(); // count = 2
        var item3 = new PendingBggImport(3, "Juego C");
        item3.IncrementRequestCount();
        item3.IncrementRequestCount(); // count = 3

        var itemCompleted = new PendingBggImport(4, "Juego D");
        itemCompleted.MarkAsCompleted(); // Completed no debe salir en pending

        await _repository.AddAsync(item1);
        await _repository.AddAsync(item2);
        await _repository.AddAsync(item3);
        await _repository.AddAsync(itemCompleted);

        // Act
        var top = await _repository.GetTopPendingAsync(2);

        // Assert
        Assert.Equal(2, top.Count);
        Assert.Equal(3, top[0].BggId); // count 3
        Assert.Equal(2, top[1].BggId); // count 2
    }

    [Fact]
    public async Task GetTotalPendingCountAsync_CountsOnlyPendingStatus()
    {
        // Arrange
        var item1 = new PendingBggImport(1, "Juego 1");
        var item2 = new PendingBggImport(2, "Juego 2");
        item2.MarkAsCompleted();

        await _repository.AddAsync(item1);
        await _repository.AddAsync(item2);

        // Act
        var count = await _repository.GetTotalPendingCountAsync();

        // Assert
        Assert.Equal(1, count);
    }
}
