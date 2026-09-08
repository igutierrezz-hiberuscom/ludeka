using System;
using System.Linq;
using System.Threading.Tasks;
using Ludeka.Core.Entities;
using Ludeka.Core.Enums;
using Ludeka.Infrastructure.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Ludeka.UnitTests.Infrastructure;

public class SqliteAuditLogRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly LudekaDbContext _context;
    private readonly SqliteAuditLogRepository _repository;

    public SqliteAuditLogRepositoryTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<LudekaDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new LudekaDbContext(options);
        _context.Database.EnsureCreated();

        _repository = new SqliteAuditLogRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    private static AuditLogEntry CreateEntry(
        string userId,
        AuditEntityType entityType,
        AuditAction action,
        DateTimeOffset timestamp,
        Guid? id = null)
    {
        return new AuditLogEntry(
            userId,
            "Usuario de Pruebas",
            action,
            entityType,
            entityId: "entidad-1",
            entityName: "Entidad de Pruebas",
            summary: "Resumen de prueba",
            timestamp: timestamp,
            id: id);
    }

    [Fact]
    public async Task GetLogsAsync_ShouldNotThrowAndOrderByTimestampDescendingWithIdTiebreak()
    {
        // Arrange: dos entradas comparten Timestamp y se desempatan de forma determinista
        // por Id (desc); la entrada restante tiene Timestamp anterior y va ultima.
        // Las entradas de otro usuario deben quedar excluidas.
        const string userId = "auth0|moderator-1";
        const string otherUserId = "auth0|moderator-2";
        var older = new DateTimeOffset(2024, 3, 1, 20, 0, 0, TimeSpan.Zero);
        var newer = new DateTimeOffset(2024, 3, 5, 21, 30, 0, TimeSpan.Zero);

        var tieIds = new[] { Guid.NewGuid(), Guid.NewGuid() }.OrderByDescending(g => g).ToArray();
        var firstNewer = CreateEntry(userId, AuditEntityType.Game, AuditAction.Updated, newer, tieIds[1]);
        await _repository.AddAsync(firstNewer);

        var secondNewerHighId = CreateEntry(userId, AuditEntityType.Game, AuditAction.Updated, newer, tieIds[0]);
        await _repository.AddAsync(secondNewerHighId);

        var olderEntry = CreateEntry(userId, AuditEntityType.Game, AuditAction.Created, older);
        await _repository.AddAsync(olderEntry);

        await _repository.AddAsync(CreateEntry(otherUserId, AuditEntityType.Game, AuditAction.Updated, newer));

        // Act
        var result = await _repository.GetLogsAsync(userId: userId);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal(
            new[] { secondNewerHighId.Id, firstNewer.Id, olderEntry.Id },
            result.Select(a => a.Id).ToArray());
        Assert.DoesNotContain(result, a => a.UserId == otherUserId);
    }

    [Fact]
    public async Task GetLogsAsync_ShouldFilterByEntityTypeActionAndDateRange()
    {
        // Arrange: cinco entradas combinando tipo, accion y fecha; el filtro combinado
        // (Game + Updated dentro del rango) debe devolver exactamente la entrada coincidente,
        // tanto en el listado paginado como en el conteo total.
        const string userId = "auth0|moderator-1";
        var outsideRangeBefore = new DateTimeOffset(2024, 1, 1, 10, 0, 0, TimeSpan.Zero);
        var rangeStart = new DateTimeOffset(2024, 3, 1, 0, 0, 0, TimeSpan.Zero);
        var inside = new DateTimeOffset(2024, 3, 2, 15, 0, 0, TimeSpan.Zero);
        var rangeEnd = new DateTimeOffset(2024, 3, 31, 23, 59, 59, TimeSpan.Zero);
        var outsideRangeAfter = new DateTimeOffset(2024, 6, 1, 12, 0, 0, TimeSpan.Zero);

        var matching = CreateEntry(userId, AuditEntityType.Game, AuditAction.Updated, inside);
        await _repository.AddAsync(matching);
        await _repository.AddAsync(CreateEntry(userId, AuditEntityType.Game, AuditAction.Updated, outsideRangeBefore));
        await _repository.AddAsync(CreateEntry(userId, AuditEntityType.Game, AuditAction.Created, inside));
        await _repository.AddAsync(CreateEntry(userId, AuditEntityType.Media, AuditAction.Updated, inside));
        await _repository.AddAsync(CreateEntry(userId, AuditEntityType.Game, AuditAction.Updated, outsideRangeAfter));

        // Act
        var result = await _repository.GetLogsAsync(
            entityType: AuditEntityType.Game,
            action: AuditAction.Updated,
            fromDate: rangeStart,
            toDate: rangeEnd);
        var total = await _repository.CountLogsAsync(
            entityType: AuditEntityType.Game,
            action: AuditAction.Updated,
            fromDate: rangeStart,
            toDate: rangeEnd);

        // Assert
        Assert.Single(result);
        Assert.Equal(matching.Id, result[0].Id);
        Assert.Equal(1, total);
    }

    [Fact]
    public async Task GetLogsAsync_ShouldApplySkipTakeAndReturnEmptyForUserWithoutLogs()
    {
        // Arrange: tres entradas del mismo usuario con Timestamps distintos; skip 1 + take 1
        // debe devolver la segunda mas reciente de la pagina ordenada.
        const string userId = "auth0|moderator-1";
        const string otherUserId = "auth0|moderator-2";

        var oldest = CreateEntry(userId, AuditEntityType.Game, AuditAction.Created, new DateTimeOffset(2024, 3, 1, 10, 0, 0, TimeSpan.Zero));
        var middle = CreateEntry(userId, AuditEntityType.Game, AuditAction.Updated, new DateTimeOffset(2024, 3, 2, 10, 0, 0, TimeSpan.Zero));
        var newest = CreateEntry(userId, AuditEntityType.Game, AuditAction.Deleted, new DateTimeOffset(2024, 3, 3, 10, 0, 0, TimeSpan.Zero));
        await _repository.AddAsync(oldest);
        await _repository.AddAsync(middle);
        await _repository.AddAsync(newest);

        // Act
        var page = await _repository.GetLogsAsync(userId: userId, skip: 1, take: 1);
        var emptyForUnknownUser = await _repository.GetLogsAsync(userId: otherUserId);

        // Assert
        Assert.Equal(new[] { middle.Id }, page.Select(a => a.Id).ToArray());
        // Usuario sin entradas: la consulta corrio, filtro y no encontro nada.
        Assert.Empty(emptyForUnknownUser);
    }
}
