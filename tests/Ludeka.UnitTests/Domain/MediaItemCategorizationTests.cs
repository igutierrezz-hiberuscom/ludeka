using System;
using System.Threading;
using Ludeka.Core.Entities;
using Ludeka.Core.Enums;
using Xunit;

namespace Ludeka.UnitTests.Domain;

public class MediaItemCategorizationTests
{
    [Theory]
    [InlineData(MediaType.QuickOverview, MediaCategory.QuickOverview)]
    [InlineData(MediaType.Tutorial, MediaCategory.Tutorial)]
    [InlineData(MediaType.Playthrough, MediaCategory.Gameplay)]
    [InlineData(MediaType.InstagramPost, MediaCategory.ReviewOpinion)]
    [InlineData(MediaType.ShortReel, MediaCategory.ReviewOpinion)]
    public void Constructor_WithoutExplicitCategory_ShouldInferCategoryFromMediaType(MediaType type, MediaCategory expectedCategory)
    {
        var item = new MediaItem(
            type: type,
            platform: MediaPlatform.YouTube,
            title: "Título de prueba",
            url: "https://www.youtube.com/watch?v=abc12345",
            thumbnailUrl: "https://img.youtube.com/vi/abc12345/hqdefault.jpg",
            authorChannel: "@canal",
            playerCountBadge: type == MediaType.Playthrough ? "Partida a 4" : null
        );

        Assert.Equal(expectedCategory, item.Category);
    }

    [Fact]
    public void Constructor_WithExplicitCategory_ShouldOverrideInference()
    {
        var item = new MediaItem(
            type: MediaType.Tutorial,
            platform: MediaPlatform.YouTube,
            title: "Vídeo de opinión estructurado",
            url: "https://www.youtube.com/watch?v=review123",
            thumbnailUrl: "https://img.youtube.com/vi/review123/hqdefault.jpg",
            authorChannel: "@canal",
            category: MediaCategory.ReviewOpinion
        );

        Assert.Equal(MediaCategory.ReviewOpinion, item.Category);
    }

    [Fact]
    public void ChangeCategory_ToGameplay_WithoutBadge_ShouldAssignDefaultBadge()
    {
        var item = new MediaItem(
            type: MediaType.Tutorial,
            platform: MediaPlatform.YouTube,
            title: "Tutorial que resulta ser partida",
            url: "https://www.youtube.com/watch?v=video123",
            thumbnailUrl: "https://img.youtube.com/vi/video123/hqdefault.jpg",
            authorChannel: "@canal"
        );

        Assert.Equal(MediaCategory.Tutorial, item.Category);
        Assert.Null(item.PlayerCountBadge);

        item.ChangeCategory(MediaCategory.Gameplay);

        Assert.Equal(MediaCategory.Gameplay, item.Category);
        Assert.Equal(MediaType.Playthrough, item.Type);
        Assert.Equal("Partida a 2", item.PlayerCountBadge);
    }

    [Fact]
    public void ChangeCategory_ToQuickOverview_ShouldUpdateType()
    {
        var item = new MediaItem(
            type: MediaType.Tutorial,
            platform: MediaPlatform.YouTube,
            title: "Explicación breve de 2 min",
            url: "https://www.youtube.com/watch?v=video123",
            thumbnailUrl: "https://img.youtube.com/vi/video123/hqdefault.jpg",
            authorChannel: "@canal"
        );

        item.ChangeCategory(MediaCategory.QuickOverview);

        Assert.Equal(MediaCategory.QuickOverview, item.Category);
        Assert.Equal(MediaType.QuickOverview, item.Type);
    }

    [Fact]
    public void ReassignGame_WithValidGameId_ShouldUpdateGameIdAndUpdatedAt()
    {
        var originalGameId = Guid.NewGuid();
        var newGameId = Guid.NewGuid();

        var item = new MediaItem(
            type: MediaType.Tutorial,
            platform: MediaPlatform.YouTube,
            title: "Tutorial reasignable",
            url: "https://www.youtube.com/watch?v=video123",
            thumbnailUrl: "https://img.youtube.com/vi/video123/hqdefault.jpg",
            authorChannel: "@canal",
            gameId: originalGameId
        );

        var initialUpdatedAt = item.UpdatedAt;
        Thread.Sleep(10); // Garantizar avance de reloj

        item.ReassignGame(newGameId);

        Assert.Equal(newGameId, item.GameId);
        Assert.True(item.UpdatedAt >= initialUpdatedAt);
    }

    [Fact]
    public void ReassignGame_WithEmptyGuid_ShouldThrowArgumentException()
    {
        var item = new MediaItem(
            type: MediaType.Tutorial,
            platform: MediaPlatform.YouTube,
            title: "Tutorial",
            url: "https://www.youtube.com/watch?v=video123",
            thumbnailUrl: "https://img.youtube.com/vi/video123/hqdefault.jpg",
            authorChannel: "@canal"
        );

        var ex = Assert.Throws<ArgumentException>(() => item.ReassignGame(Guid.Empty));
        Assert.Contains("El identificador del juego no puede estar vacío", ex.Message);
    }

    [Fact]
    public void Approve_WithCategory_ShouldApproveAndChangeCategory()
    {
        var item = new MediaItem(
            type: MediaType.Tutorial,
            platform: MediaPlatform.YouTube,
            title: "Vídeo pendiente",
            url: "https://www.youtube.com/watch?v=video123",
            thumbnailUrl: "https://img.youtube.com/vi/video123/hqdefault.jpg",
            authorChannel: "@canal",
            status: ModerationStatus.PendingApproval
        );

        Assert.Equal(ModerationStatus.PendingApproval, item.Status);
        Assert.Equal(MediaCategory.Tutorial, item.Category);

        item.Approve(MediaCategory.ReviewOpinion);

        Assert.Equal(ModerationStatus.Approved, item.Status);
        Assert.Equal(MediaCategory.ReviewOpinion, item.Category);
    }
}
