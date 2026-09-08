using System;
using Ludeka.Core.Entities;
using Ludeka.Core.Enums;
using Xunit;

namespace Ludeka.UnitTests.Domain;

public class InstagramPostDraftTests
{
    [Fact]
    public void Constructor_ValidArguments_CreatesDraft()
    {
        // Arrange & Act
        var draft = new InstagramPostDraft(
            title: "Sorteo Ark Nova",
            caption: "Participa en el sorteo de Ark Nova!",
            sourceType: InstagramPostSourceType.Giveaway,
            sourceId: Guid.NewGuid().ToString(),
            createdByUserId: "user_123",
            createdByUserName: "Moderador 1",
            imageUrl: "https://ejemplo.com/arknova.jpg",
            svgContent: "<svg></svg>",
            theme: "Dark");

        // Assert
        Assert.NotEqual(Guid.Empty, draft.Id);
        Assert.Equal(InstagramPostSourceType.Giveaway, draft.SourceType);
        Assert.Equal("Sorteo Ark Nova", draft.Title);
        Assert.Equal("Participa en el sorteo de Ark Nova!", draft.Caption);
        Assert.Equal("https://ejemplo.com/arknova.jpg", draft.ImageUrl);
        Assert.Equal("<svg></svg>", draft.SvgContent);
        Assert.Equal("Dark", draft.Theme);
        Assert.Equal(InstagramPostDraftStatus.Draft, draft.Status);
        Assert.Equal("user_123", draft.CreatedByUserId);
        Assert.Equal("Moderador 1", draft.CreatedByUserName);
        Assert.Null(draft.InstagramMediaId);
        Assert.Null(draft.InstagramPermalink);
        Assert.Null(draft.PublishedAt);
        Assert.Null(draft.ErrorMessage);
    }

    [Theory]
    [InlineData("", "Caption valido", "user_1")]
    [InlineData("   ", "Caption valido", "user_1")]
    [InlineData("Titulo valido", "", "user_1")]
    [InlineData("Titulo valido", "   ", "user_1")]
    [InlineData("Titulo valido", "Caption valido", "")]
    [InlineData("Titulo valido", "Caption valido", "   ")]
    public void Constructor_InvalidArguments_ThrowsArgumentException(string title, string caption, string userId)
    {
        Assert.Throws<ArgumentException>(() => new InstagramPostDraft(
            title: title,
            caption: caption,
            sourceType: InstagramPostSourceType.WeeklyRelease,
            sourceId: Guid.NewGuid().ToString(),
            createdByUserId: userId,
            createdByUserName: "Moderador"));
    }

    [Fact]
    public void UpdateDraft_ModifiesDraftProperly()
    {
        // Arrange
        var draft = new InstagramPostDraft(
            "Brass Birmingham",
            "Texto inicial",
            InstagramPostSourceType.Game,
            Guid.NewGuid().ToString(),
            "user_1",
            "Mod");

        // Act
        draft.UpdateDraft(
            caption: "Texto actualizado con más detalles #juegosdemesa",
            theme: "Light",
            imageUrl: "https://ejemplo.com/brass.png",
            svgContent: "<svg id='updated'></svg>");

        // Assert
        Assert.Equal("Texto actualizado con más detalles #juegosdemesa", draft.Caption);
        Assert.Equal("Light", draft.Theme);
        Assert.Equal("https://ejemplo.com/brass.png", draft.ImageUrl);
        Assert.Equal("<svg id='updated'></svg>", draft.SvgContent);
    }

    [Fact]
    public void MarkPublished_SetsStatusAndDetails()
    {
        // Arrange
        var draft = new InstagramPostDraft(
            "Sorteo Heat",
            "Texto de Heat",
            InstagramPostSourceType.Giveaway,
            Guid.NewGuid().ToString(),
            "user_1",
            "Mod");

        // Act
        draft.MarkPublished("media_12345", "https://instagram.com/p/media_12345/");

        // Assert
        Assert.Equal(InstagramPostDraftStatus.Published, draft.Status);
        Assert.Equal("media_12345", draft.InstagramMediaId);
        Assert.Equal("https://instagram.com/p/media_12345/", draft.InstagramPermalink);
        Assert.NotNull(draft.PublishedAt);
        Assert.Null(draft.ErrorMessage);
    }

    [Fact]
    public void MarkFailed_SetsStatusAndErrorMessage()
    {
        // Arrange
        var draft = new InstagramPostDraft(
            "Lanzamiento Wingspan",
            "Texto de Wingspan",
            InstagramPostSourceType.WeeklyRelease,
            Guid.NewGuid().ToString(),
            "user_1",
            "Mod");

        // Act
        draft.MarkFailed("Error de token inválido en Meta Graph API");

        // Assert
        Assert.Equal(InstagramPostDraftStatus.Failed, draft.Status);
        Assert.Equal("Error de token inválido en Meta Graph API", draft.ErrorMessage);
    }

    [Fact]
    public void Giveaway_MarkPublishedOnInstagram_SetsProperties()
    {
        // Arrange
        var giveaway = new Giveaway(
            title: "Sorteo Cascadia",
            organizer: "Delirium Games",
            url: "https://instagram.com/p/cascadia",
            platform: GiveawayPlatform.Instagram,
            deadlineAt: DateTimeOffset.UtcNow.AddDays(7));

        // Act
        giveaway.MarkPublishedOnInstagram("ig_giveaway_999", "https://instagram.com/p/giveaway_999");

        // Assert
        Assert.True(giveaway.IsPublishedOnInstagram);
        Assert.Equal("ig_giveaway_999", giveaway.InstagramMediaId);
        Assert.Equal("https://instagram.com/p/giveaway_999", giveaway.InstagramPermalink);
    }

    [Fact]
    public void WeeklyRelease_MarkPublishedOnInstagram_SetsProperties()
    {
        // Arrange
        var release = new WeeklyRelease(
            title: "Dune Imperium Uprising",
            publisher: "Asmodee",
            releaseDate: new DateOnly(2026, 9, 15),
            coverImageUrl: "https://cf.geekdo-images.com/dune.jpg");

        // Act
        release.MarkPublishedOnInstagram("ig_release_888", "https://instagram.com/p/release_888");

        // Assert
        Assert.True(release.IsPublishedOnInstagram);
        Assert.Equal("ig_release_888", release.InstagramMediaId);
        Assert.Equal("https://instagram.com/p/release_888", release.InstagramPermalink);
    }
}
