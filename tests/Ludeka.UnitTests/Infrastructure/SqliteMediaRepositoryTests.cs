using System;
using System.Threading.Tasks;
using Ludeka.Core.Entities;
using Ludeka.Core.Enums;
using Ludeka.Core.ValueObjects;
using Ludeka.Infrastructure.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Ludeka.UnitTests.Infrastructure;

public class SqliteMediaRepositoryTests : IAsyncLifetime
{
    private SqliteConnection _connection = null!;
    private LudekaDbContext _context = null!;
    private SqliteMediaRepository _repository = null!;

    public async Task InitializeAsync()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        await _connection.OpenAsync();

        var options = new DbContextOptionsBuilder<LudekaDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new LudekaDbContext(options);
        await _context.Database.EnsureCreatedAsync();

        _repository = new SqliteMediaRepository(_context);
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
        await _connection.DisposeAsync();
    }

    private static int _nextBggId = 1000;

    private async Task<Game> SeedGameAsync(string title = "Catan", int? bggId = null)
    {
        var game = new Game(
            bggId: bggId ?? System.Threading.Interlocked.Increment(ref _nextBggId),
            originalTitle: title,
            spanishTitle: title,
            designer: "Klaus Teuber",
            publisher: "Devir",
            yearPublished: 1995,
            coverImageUrl: "https://example.com/catan.jpg",
            thumbnailUrl: "https://example.com/catan-thumb.jpg",
            description: "Clásico de comercio y colonización.",
            bggRating: 7.2,
            bggRank: 500,
            ludistRating: 7.5,
            confrontation: ConfrontationType.Competitive,
            style: GameStyle.Eurogame,
            isOfficialSolo: false,
            age: new AgeRating(10, 8),
            language: LanguageDependence.None,
            footprint: TableFootprint.StandardTable,
            duration: new GameDuration(60, 90, 25)
        );

        await _context.Games.AddAsync(game);
        await _context.SaveChangesAsync();
        return game;
    }

    [Fact]
    public async Task AddAndGetByIdAsync_ShouldPersistAndRetrieveMediaItem()
    {
        var game = await SeedGameAsync();
        var item = new MediaItem(
            MediaType.Tutorial,
            MediaPlatform.YouTube,
            "Tutorial Catan",
            "https://www.youtube.com/watch?v=abc123",
            "https://img.youtube.com/thumb.jpg",
            "@zacatrus",
            gameId: game.Id,
            durationSeconds: 600,
            status: ModerationStatus.Approved
        );

        await _repository.AddAsync(item);

        var retrieved = await _repository.GetByIdAsync(item.Id);
        Assert.NotNull(retrieved);
        Assert.Equal(item.Title, retrieved.Title);
        Assert.Equal(game.Id, retrieved.GameId);
        Assert.Equal(ModerationStatus.Approved, retrieved.Status);
    }

    [Fact]
    public async Task GetApprovedByGameIdAsync_ShouldReturnOnlyApprovedForSpecificGame()
    {
        var game1 = await SeedGameAsync("Catan");
        var game2 = await SeedGameAsync("Azul");

        var approvedGame1 = new MediaItem(
            MediaType.Tutorial,
            MediaPlatform.YouTube,
            "Tutorial Catan Aprobado",
            "https://youtube.com/watch?v=1",
            "https://thumb.jpg",
            "@canal",
            gameId: game1.Id,
            status: ModerationStatus.Approved
        );

        var pendingGame1 = new MediaItem(
            MediaType.Tutorial,
            MediaPlatform.YouTube,
            "Tutorial Catan Pendiente",
            "https://youtube.com/watch?v=2",
            "https://thumb.jpg",
            "@canal",
            gameId: game1.Id,
            status: ModerationStatus.PendingApproval
        );

        var approvedGame2 = new MediaItem(
            MediaType.Tutorial,
            MediaPlatform.YouTube,
            "Tutorial Azul Aprobado",
            "https://youtube.com/watch?v=3",
            "https://thumb.jpg",
            "@canal",
            gameId: game2.Id,
            status: ModerationStatus.Approved
        );

        await _repository.AddAsync(approvedGame1);
        await _repository.AddAsync(pendingGame1);
        await _repository.AddAsync(approvedGame2);

        var results = await _repository.GetApprovedByGameIdAsync(game1.Id);

        Assert.Single(results);
        Assert.Equal("Tutorial Catan Aprobado", results[0].Title);
    }

    [Fact]
    public async Task GetPendingModerationAsync_ShouldReturnOnlyPending()
    {
        var itemPending = new MediaItem(
            MediaType.Tutorial,
            MediaPlatform.YouTube,
            "Pendiente",
            "https://youtube.com/watch?v=p1",
            "https://thumb.jpg",
            "@canal",
            status: ModerationStatus.PendingApproval
        );

        var itemApproved = new MediaItem(
            MediaType.Tutorial,
            MediaPlatform.YouTube,
            "Aprobado",
            "https://youtube.com/watch?v=p2",
            "https://thumb.jpg",
            "@canal",
            status: ModerationStatus.Approved
        );

        await _repository.AddAsync(itemPending);
        await _repository.AddAsync(itemApproved);

        var pendingList = await _repository.GetPendingModerationAsync();

        Assert.Contains(pendingList, x => x.Title == "Pendiente");
        Assert.DoesNotContain(pendingList, x => x.Title == "Aprobado");
    }

    [Fact]
    public async Task GetOrphansAsync_ShouldReturnOnlyItemsWithoutGameId()
    {
        var game = await SeedGameAsync();

        var orphan = new MediaItem(
            MediaType.ShortReel,
            MediaPlatform.Instagram,
            "Reel Huérfano",
            "https://instagram.com/reel/123",
            "https://thumb.jpg",
            "@canal",
            gameId: null
        );

        var associated = new MediaItem(
            MediaType.ShortReel,
            MediaPlatform.Instagram,
            "Reel Asociado",
            "https://instagram.com/reel/456",
            "https://thumb.jpg",
            "@canal",
            gameId: game.Id
        );

        await _repository.AddAsync(orphan);
        await _repository.AddAsync(associated);

        var orphans = await _repository.GetOrphansAsync();

        Assert.Contains(orphans, x => x.Title == "Reel Huérfano");
        Assert.DoesNotContain(orphans, x => x.Title == "Reel Asociado");
    }

    [Fact]
    public async Task UpdateAsync_ShouldPersistStatusChange()
    {
        var item = new MediaItem(
            MediaType.Tutorial,
            MediaPlatform.YouTube,
            "Aprobar luego",
            "https://youtube.com/watch?v=upd",
            "https://thumb.jpg",
            "@canal",
            status: ModerationStatus.PendingApproval
        );

        await _repository.AddAsync(item);

        item.Approve();
        await _repository.UpdateAsync(item);

        var updated = await _repository.GetByIdAsync(item.Id);
        Assert.NotNull(updated);
        Assert.Equal(ModerationStatus.Approved, updated.Status);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveMediaItem()
    {
        var item = new MediaItem(
            MediaType.Tutorial,
            MediaPlatform.YouTube,
            "Borrar",
            "https://youtube.com/watch?v=del",
            "https://thumb.jpg",
            "@canal"
        );

        await _repository.AddAsync(item);
        await _repository.DeleteAsync(item.Id);

        var deleted = await _repository.GetByIdAsync(item.Id);
        Assert.Null(deleted);
    }
}
