using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ludeka.Application.Contracts;
using Ludeka.Application.DTOs;
using Ludeka.Application.Features.Media;
using Ludeka.Core.Entities;
using Ludeka.Core.Enums;
using Ludeka.Core.ValueObjects;
using Ludeka.Infrastructure.Data;
using Ludeka.Infrastructure.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Ludeka.UnitTests.Application;

public class MediaServiceTests : IAsyncLifetime
{
    private SqliteConnection _connection = null!;
    private LudekaDbContext _context = null!;
    private IMediaRepository _mediaRepository = null!;
    private IGameRepository _gameRepository = null!;
    private IBrokenLinkCheckerService _brokenLinkChecker = null!;
    private MediaService _service = null!;

    public async Task InitializeAsync()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        await _connection.OpenAsync();

        var options = new DbContextOptionsBuilder<LudekaDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new LudekaDbContext(options);
        await _context.Database.EnsureCreatedAsync();

        _mediaRepository = new SqliteMediaRepository(_context);
        _gameRepository = new SqliteGameRepository(_context);
        _brokenLinkChecker = new BrokenLinkCheckerService(_mediaRepository, null);

        _service = new MediaService(_mediaRepository, _gameRepository, _brokenLinkChecker);
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
        await _connection.DisposeAsync();
    }

    private async Task<Game> SeedGameAsync(string title = "Terraforming Mars")
    {
        var game = new Game(
            bggId: 167791,
            originalTitle: title,
            spanishTitle: title,
            designer: "Jacob Fryxelius",
            publisher: "Maldito Games",
            yearPublished: 2016,
            coverImageUrl: "https://example.com/tfm.jpg",
            thumbnailUrl: "https://example.com/tfm-thumb.jpg",
            description: "Terraformación de Marte en el año 2400.",
            bggRating: 8.4,
            bggRank: 5,
            ludistRating: 8.6,
            confrontation: ConfrontationType.Competitive,
            style: GameStyle.Eurogame,
            isOfficialSolo: true,
            age: new AgeRating(12, 11),
            language: LanguageDependence.High,
            footprint: TableFootprint.TableMonster,
            duration: new GameDuration(90, 150, 45)
        );

        await _context.Games.AddAsync(game);
        await _context.SaveChangesAsync();
        return game;
    }

    [Fact]
    public async Task GetGameMediaAsync_ShouldSegregateIntoFourCategoriesAndFilterBroken()
    {
        var game = await SeedGameAsync();

        // 1. Tutorial aprobado
        var tutorial = new MediaItem(
            MediaType.Tutorial,
            MediaPlatform.YouTube,
            "Tutorial TFM",
            "https://youtube.com/watch?v=tuto",
            "https://thumb.jpg",
            "@analisisparalisis",
            gameId: game.Id,
            durationSeconds: 1200,
            status: ModerationStatus.Approved
        );

        // 2. Partida aprobada con badge
        var playthrough = new MediaItem(
            MediaType.Playthrough,
            MediaPlatform.YouTube,
            "Partida a 2 TFM",
            "https://youtube.com/watch?v=play",
            "https://thumb.jpg",
            "@rinconlegacy",
            gameId: game.Id,
            durationSeconds: 4500,
            playerCountBadge: "Partida a 2",
            status: ModerationStatus.Approved
        );

        // 3. Instagram post
        var post = new MediaItem(
            MediaType.InstagramPost,
            MediaPlatform.Instagram,
            "Mesa repleta",
            "https://instagram.com/p/foto",
            "https://thumb.jpg",
            "@eldardoludico",
            gameId: game.Id,
            likesCount: 150,
            status: ModerationStatus.Approved
        );

        // 4. Short Reel
        var reel = new MediaItem(
            MediaType.ShortReel,
            MediaPlatform.YouTube,
            "Reel TFM",
            "https://youtube.com/shorts/reel",
            "https://thumb.jpg",
            "@zacatrustv",
            gameId: game.Id,
            durationSeconds: 50,
            status: ModerationStatus.Approved
        );

        // 5. Tutorial Roto (IsBroken = true)
        var brokenItem = new MediaItem(
            MediaType.Tutorial,
            MediaPlatform.YouTube,
            "Tutorial Antiguo Borrado",
            "https://youtube.com/watch?v=broken",
            "https://thumb.jpg",
            "@borrado",
            gameId: game.Id,
            status: ModerationStatus.Approved
        );
        brokenItem.MarkAsBroken(true);

        // 6. Tutorial no aprobado (PendingApproval)
        var pendingItem = new MediaItem(
            MediaType.Tutorial,
            MediaPlatform.YouTube,
            "Tutorial pendiente",
            "https://youtube.com/watch?v=pending",
            "https://thumb.jpg",
            "@novato",
            gameId: game.Id,
            status: ModerationStatus.PendingApproval
        );

        await _mediaRepository.AddAsync(tutorial);
        await _mediaRepository.AddAsync(playthrough);
        await _mediaRepository.AddAsync(post);
        await _mediaRepository.AddAsync(reel);
        await _mediaRepository.AddAsync(brokenItem);
        await _mediaRepository.AddAsync(pendingItem);

        var hub = await _service.GetGameMediaAsync(game.Id);

        Assert.NotNull(hub);
        Assert.Single(hub.Tutorials);
        Assert.Equal("Tutorial TFM", hub.Tutorials[0].Title);

        Assert.Single(hub.Playthroughs);
        Assert.Equal("Partida a 2 TFM", hub.Playthroughs[0].Title);
        Assert.Equal("Partida a 2", hub.Playthroughs[0].PlayerCountBadge);

        Assert.Single(hub.InstagramPosts);
        Assert.Equal("Mesa repleta", hub.InstagramPosts[0].Title);

        Assert.Single(hub.ShortReels);
        Assert.Equal("Reel TFM", hub.ShortReels[0].Title);

        Assert.Equal(4, hub.TotalCount);
        Assert.True(hub.HasMedia);
        Assert.True(hub.HasTutorials);
        Assert.True(hub.HasPlaythroughs);
        Assert.True(hub.HasSocial);
    }

    [Fact]
    public async Task ApproveMediaAsync_WhenFound_ShouldSetApproved()
    {
        var game = await SeedGameAsync();
        var item = new MediaItem(
            MediaType.Tutorial,
            MediaPlatform.YouTube,
            "Tutorial Pendiente",
            "https://youtube.com/watch?v=test",
            "https://thumb.jpg",
            "@autor",
            gameId: game.Id,
            status: ModerationStatus.PendingApproval
        );

        await _mediaRepository.AddAsync(item);

        var result = await _service.ApproveMediaAsync(item.Id);

        Assert.NotNull(result);
        Assert.Equal(ModerationStatus.Approved, result.Status);
        Assert.Equal(game.SpanishTitle, result.GameTitle);

        var inDb = await _mediaRepository.GetByIdAsync(item.Id);
        Assert.NotNull(inDb);
        Assert.Equal(ModerationStatus.Approved, inDb.Status);
    }

    [Fact]
    public async Task RejectMediaAsync_WhenFound_ShouldSetRejected()
    {
        var item = new MediaItem(
            MediaType.Tutorial,
            MediaPlatform.YouTube,
            "Spam de vídeo",
            "https://youtube.com/watch?v=spam",
            "https://thumb.jpg",
            "@spammer",
            status: ModerationStatus.PendingApproval
        );

        await _mediaRepository.AddAsync(item);

        var result = await _service.RejectMediaAsync(item.Id);

        Assert.NotNull(result);
        Assert.Equal(ModerationStatus.Rejected, result.Status);
    }

    [Fact]
    public async Task AssignOrphanMediaAsync_WithValidGame_ShouldAssignGame()
    {
        var game = await SeedGameAsync("Wingspan");
        var orphan = new MediaItem(
            MediaType.ShortReel,
            MediaPlatform.YouTube,
            "Reel de pájaros",
            "https://youtube.com/shorts/aves",
            "https://thumb.jpg",
            "@autor",
            gameId: null,
            status: ModerationStatus.PendingApproval
        );

        await _mediaRepository.AddAsync(orphan);

        var result = await _service.AssignOrphanMediaAsync(orphan.Id, game.Id);

        Assert.NotNull(result);
        Assert.False(result.IsOrphan);
        Assert.Equal(game.Id, result.GameId);
        Assert.Equal("Wingspan", result.GameTitle);
    }

    [Fact]
    public async Task AssignOrphanMediaAsync_WithNonExistentGame_ShouldThrowArgumentException()
    {
        var orphan = new MediaItem(
            MediaType.ShortReel,
            MediaPlatform.YouTube,
            "Reel huérfano",
            "https://youtube.com/shorts/aves2",
            "https://thumb.jpg",
            "@autor",
            gameId: null
        );

        await _mediaRepository.AddAsync(orphan);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.AssignOrphanMediaAsync(orphan.Id, Guid.NewGuid()));
    }

    [Fact]
    public async Task CheckBrokenLinksAsync_ShouldDetectBrokenLinks()
    {
        var brokenItem = new MediaItem(
            MediaType.Tutorial,
            MediaPlatform.YouTube,
            "Enlace roto conocido",
            "https://youtube.com/watch?v=broken_video_404",
            "https://thumb.jpg",
            "@canal"
        );

        var validItem = new MediaItem(
            MediaType.Tutorial,
            MediaPlatform.YouTube,
            "Enlace válido",
            "https://youtube.com/watch?v=valid_video",
            "https://thumb.jpg",
            "@canal"
        );

        await _mediaRepository.AddAsync(brokenItem);
        await _mediaRepository.AddAsync(validItem);

        var report = await _service.CheckBrokenLinksAsync();

        Assert.Equal(2, report.TotalChecked);
        Assert.Equal(1, report.BrokenCount);
        Assert.Single(report.BrokenItems);
        Assert.Equal("Enlace roto conocido", report.BrokenItems[0].Title);
    }
}
