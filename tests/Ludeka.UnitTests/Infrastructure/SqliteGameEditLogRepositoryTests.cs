using System;
using System.Linq;
using System.Threading.Tasks;
using Ludeka.Core.Entities;
using Ludeka.Infrastructure.Data;
using Ludeka.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Ludeka.UnitTests.Infrastructure;

public class SqliteGameEditLogRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly LudekaDbContext _context;
    private readonly SqliteGameEditLogRepository _repository;

    public SqliteGameEditLogRepositoryTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<LudekaDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new LudekaDbContext(options);
        _context.Database.EnsureCreated();

        _repository = new SqliteGameEditLogRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    private async Task<GameEditLog> AddLogAsync(Guid gameId, string editorUserId)
    {
        var log = new GameEditLog(gameId, editorUserId, "Editor de Pruebas", "Edicion editorial de prueba");
        await _repository.AddAsync(log);
        return log;
    }

    [Fact]
    public async Task GetByGameIdAsync_ShouldNotThrowAndOrderByEditedAtDescending()
    {
        // Arrange: dos ediciones del mismo juego con EditedAt crecientes (el campo se fija en
        // el constructor, por eso se espacian con Task.Delay); el orden esperado es
        // descendente por EditedAt.
        var gameId = Guid.NewGuid();
        var oldest = await AddLogAsync(gameId, "auth0|editor-1");
        await Task.Delay(5);
        var newest = await AddLogAsync(gameId, "auth0|editor-2");

        // Act
        var result = await _repository.GetByGameIdAsync(gameId);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal(
            new[] { newest.Id, oldest.Id },
            result.Select(l => l.Id).ToArray());
    }

    [Fact]
    public async Task GetByGameIdAsync_ShouldFilterByGameAndReturnEmptyForGameWithoutLogs()
    {
        // Arrange: dos ediciones del juego objetivo, una edicion de un tercer juego que debe
        // quedar excluida, y un juego sin ninguna edicion para el caso vacio.
        var gameWithLogs = Guid.NewGuid();
        var otherGameWithLogs = Guid.NewGuid();
        var gameWithoutLogs = Guid.NewGuid();
        var oldest = await AddLogAsync(gameWithLogs, "auth0|editor-1");
        await Task.Delay(5);
        var newest = await AddLogAsync(gameWithLogs, "auth0|editor-2");
        await AddLogAsync(otherGameWithLogs, "auth0|editor-3");

        // Act
        var result = await _repository.GetByGameIdAsync(gameWithLogs);
        var empty = await _repository.GetByGameIdAsync(gameWithoutLogs);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal(
            new[] { newest.Id, oldest.Id },
            result.Select(l => l.Id).ToArray());
        Assert.DoesNotContain(result, l => l.GameId == otherGameWithLogs);
        // Juego sin ediciones: la consulta corrio, filtro y no encontro nada.
        Assert.Empty(empty);
    }
}
