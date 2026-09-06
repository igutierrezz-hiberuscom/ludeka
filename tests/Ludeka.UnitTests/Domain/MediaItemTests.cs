using System;
using Ludeka.Core.Entities;
using Ludeka.Core.Enums;
using Xunit;

namespace Ludeka.UnitTests.Domain;

public class MediaItemTests
{
    [Fact]
    public void Constructor_WithValidTutorial_ShouldCreateMediaItem()
    {
        var gameId = Guid.NewGuid();
        var item = new MediaItem(
            MediaType.Tutorial,
            MediaPlatform.YouTube,
            "Cómo jugar a Catan en 15 minutos",
            "https://www.youtube.com/watch?v=12345ABC",
            "https://img.youtube.com/vi/12345ABC/hqdefault.jpg",
            "@analisisparalisis",
            gameId: gameId,
            durationSeconds: 930
        );

        Assert.NotEqual(Guid.Empty, item.Id);
        Assert.Equal(gameId, item.GameId);
        Assert.Equal(MediaType.Tutorial, item.Type);
        Assert.Equal(MediaPlatform.YouTube, item.Platform);
        Assert.Equal("Cómo jugar a Catan en 15 minutos", item.Title);
        Assert.Equal("https://www.youtube-nocookie.com/embed/12345ABC", item.EmbedUrl);
        Assert.Equal("@analisisparalisis", item.AuthorChannel);
        Assert.Equal(930, item.DurationSeconds);
        Assert.Equal("15:30", item.FormattedDuration);
        Assert.Equal(ModerationStatus.PendingApproval, item.Status);
        Assert.False(item.IsOrphan);
        Assert.False(item.IsBroken);
    }

    [Fact]
    public void Constructor_WithPlaythrough_RequiresPlayerCountBadge()
    {
        var ex = Assert.Throws<ArgumentException>(() => new MediaItem(
            MediaType.Playthrough,
            MediaPlatform.YouTube,
            "Partida completa épica",
            "https://www.youtube.com/watch?v=playthrough01",
            "https://img.youtube.com/vi/playthrough01/hqdefault.jpg",
            "@zacatrustv",
            playerCountBadge: null // Falta badge de comensales
        ));

        Assert.Contains("PlayerCountBadge", ex.Message);
    }

    [Fact]
    public void Constructor_WithPlaythrough_ValidBadge_ShouldSucceed()
    {
        var item = new MediaItem(
            MediaType.Playthrough,
            MediaPlatform.YouTube,
            "Partida completa a 2 jugadores",
            "https://www.youtube.com/watch?v=playthrough02",
            "https://img.youtube.com/vi/playthrough02/hqdefault.jpg",
            "@rinconlegacy",
            playerCountBadge: "Partida a 2",
            durationSeconds: 4200
        );

        Assert.Equal(MediaType.Playthrough, item.Type);
        Assert.Equal("Partida a 2", item.PlayerCountBadge);
        Assert.Equal("1:10:00", item.FormattedDuration);
    }

    [Fact]
    public void Constructor_WithInvalidUrl_ShouldThrowArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new MediaItem(
            MediaType.Tutorial,
            MediaPlatform.YouTube,
            "Tutorial inválido",
            "url-no-valida",
            "https://img.youtube.com/thumb.jpg",
            "@autor"
        ));
    }

    [Fact]
    public void Constructor_WithBlankTitle_ShouldThrowArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new MediaItem(
            MediaType.Tutorial,
            MediaPlatform.YouTube,
            "   ",
            "https://youtube.com/watch?v=abc",
            "https://img.youtube.com/thumb.jpg",
            "@autor"
        ));
    }

    [Fact]
    public void Constructor_WithoutGameId_ShouldBeOrphan()
    {
        var item = new MediaItem(
            MediaType.ShortReel,
            MediaPlatform.Instagram,
            "Unboxing exprés en 60 segundos",
            "https://www.instagram.com/reel/xyz123/",
            "https://instagram.com/p/xyz123.jpg",
            "@ludist_app",
            gameId: null,
            durationSeconds: 58
        );

        Assert.True(item.IsOrphan);
        Assert.Null(item.GameId);
    }

    [Fact]
    public void Approve_ShouldSetStatusApprovedAndRefreshUpdatedAt()
    {
        var item = new MediaItem(
            MediaType.Tutorial,
            MediaPlatform.YouTube,
            "Tutorial Ark Nova",
            "https://www.youtube.com/watch?v=arknova",
            "https://img.youtube.com/thumb.jpg",
            "@meepletopia"
        );

        Assert.Equal(ModerationStatus.PendingApproval, item.Status);
        item.Approve();
        Assert.Equal(ModerationStatus.Approved, item.Status);
    }

    [Fact]
    public void Reject_ShouldSetStatusRejectedAndRefreshUpdatedAt()
    {
        var item = new MediaItem(
            MediaType.Tutorial,
            MediaPlatform.YouTube,
            "Vídeo publicitario engañoso",
            "https://www.youtube.com/watch?v=spam",
            "https://img.youtube.com/thumb.jpg",
            "@spammer"
        );

        item.Reject();
        Assert.Equal(ModerationStatus.Rejected, item.Status);
    }

    [Fact]
    public void AssignToGame_WithValidGameId_ShouldAssociateGameAndClearOrphan()
    {
        var item = new MediaItem(
            MediaType.InstagramPost,
            MediaPlatform.Instagram,
            "Foto de mesa de Wingspan",
            "https://www.instagram.com/p/wingspanpost/",
            "https://instagram.com/thumb.jpg",
            "@eldardoludico",
            gameId: null
        );

        Assert.True(item.IsOrphan);

        var gameId = Guid.NewGuid();
        item.AssignToGame(gameId);

        Assert.False(item.IsOrphan);
        Assert.Equal(gameId, item.GameId);
    }

    [Fact]
    public void AssignToGame_WithEmptyGuid_ShouldThrowArgumentException()
    {
        var item = new MediaItem(
            MediaType.InstagramPost,
            MediaPlatform.Instagram,
            "Foto de mesa",
            "https://www.instagram.com/p/post/",
            "https://instagram.com/thumb.jpg",
            "@eldardoludico",
            gameId: null
        );

        Assert.Throws<ArgumentException>(() => item.AssignToGame(Guid.Empty));
    }

    [Fact]
    public void MarkAsBroken_ShouldToggleIsBroken()
    {
        var item = new MediaItem(
            MediaType.Tutorial,
            MediaPlatform.YouTube,
            "Tutorial antiguo",
            "https://www.youtube.com/watch?v=oldvideo",
            "https://img.youtube.com/thumb.jpg",
            "@antiguo"
        );

        Assert.False(item.IsBroken);
        item.MarkAsBroken(true);
        Assert.True(item.IsBroken);
    }

    [Theory]
    [InlineData(null, "")]
    [InlineData(0, "")]
    [InlineData(-5, "")]
    [InlineData(45, "00:45")]
    [InlineData(65, "01:05")]
    [InlineData(930, "15:30")]
    [InlineData(3665, "1:01:05")]
    public void FormatDuration_ShouldFormatCorrectly(int? seconds, string expected)
    {
        var formatted = MediaItem.FormatDuration(seconds);
        Assert.Equal(expected, formatted);
    }
}
