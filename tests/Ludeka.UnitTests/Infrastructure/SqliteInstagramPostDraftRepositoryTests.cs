using System;
using System.Linq;
using System.Threading.Tasks;
using Ludeka.Core.Entities;
using Ludeka.Core.Enums;
using Ludeka.Infrastructure.Data;
using Ludeka.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Ludeka.UnitTests.Infrastructure;

public class SqliteInstagramPostDraftRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly LudekaDbContext _context;
    private readonly SqliteInstagramPostDraftRepository _repository;

    public SqliteInstagramPostDraftRepositoryTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<LudekaDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new LudekaDbContext(options);
        _context.Database.EnsureCreated();

        _repository = new SqliteInstagramPostDraftRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    private async Task<InstagramPostDraft> AddDraftAsync(string title)
    {
        var draft = new InstagramPostDraft(
            title,
            $"Texto editorial de {title}.",
            InstagramPostSourceType.Manual,
            $"source-{title}",
            "auth0|moderator-1",
            "Moderador de Pruebas");
        await _repository.AddAsync(draft);
        return draft;
    }

    [Fact]
    public async Task GetDraftsAsync_ShouldNotThrowAndOrderByCreatedAtDescending()
    {
        // Arrange: tres borradores con CreatedAt crecientes (el campo se fija en el constructor,
        // por eso se espacian con Task.Delay); el orden esperado es descendente por CreatedAt.
        var oldest = await AddDraftAsync("Borrador mas antiguo");
        await Task.Delay(5);
        var middle = await AddDraftAsync("Borrador intermedio");
        await Task.Delay(5);
        var newest = await AddDraftAsync("Borrador mas reciente");

        // Act
        var result = await _repository.GetDraftsAsync();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal(
            new[] { newest.Id, middle.Id, oldest.Id },
            result.Select(d => d.Id).ToArray());
    }

    [Fact]
    public async Task GetDraftsAsync_ShouldFilterByStatusWithoutThrowing()
    {
        // Arrange: el borrador mas antiguo se publica; el filtro por estado debe respetarse
        // manteniendo el orden descendente por CreatedAt dentro de cada estado.
        var published = await AddDraftAsync("Borrador publicado");
        await Task.Delay(5);
        var olderDraft = await AddDraftAsync("Borrador en edicion 1");
        await Task.Delay(5);
        var newestDraft = await AddDraftAsync("Borrador en edicion 2");

        published.MarkPublished("media-123", "https://instagram.com/p/ludeka");
        await _repository.UpdateAsync(published);

        // Act
        var draftsOnly = await _repository.GetDraftsAsync(InstagramPostDraftStatus.Draft);
        var publishedOnly = await _repository.GetDraftsAsync(InstagramPostDraftStatus.Published);
        var failedOnly = await _repository.GetDraftsAsync(InstagramPostDraftStatus.Failed);

        // Assert
        Assert.Equal(
            new[] { newestDraft.Id, olderDraft.Id },
            draftsOnly.Select(d => d.Id).ToArray());
        Assert.Equal(new[] { published.Id }, publishedOnly.Select(d => d.Id).ToArray());
        // Sin borradores fallidos sembrados: la consulta corrio, filtro y no encontro nada.
        Assert.Empty(failedOnly);
    }
}
