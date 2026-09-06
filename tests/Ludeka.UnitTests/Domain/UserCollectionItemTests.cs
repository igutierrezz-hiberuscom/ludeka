using System;
using Ludeka.Core.Entities;
using Ludeka.Core.Enums;
using Xunit;

namespace Ludeka.UnitTests.Domain;

public class UserCollectionItemTests
{
    [Fact]
    public void Constructor_ShouldInitializeCorrectly_WhenParametersAreValid()
    {
        // Arrange
        string userId = "user-123";
        Guid gameId = Guid.NewGuid();

        // Act
        var item = new UserCollectionItem(userId, gameId, CollectionStatus.InCollection);

        // Assert
        Assert.NotEqual(Guid.Empty, item.Id);
        Assert.Equal(userId, item.UserId);
        Assert.Equal(gameId, item.GameId);
        Assert.Equal(CollectionStatus.InCollection, item.Status);
        Assert.Null(item.UpdatedAt);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ShouldThrowArgumentException_WhenUserIdIsInvalid(string? invalidUserId)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new UserCollectionItem(invalidUserId, Guid.NewGuid(), CollectionStatus.Played));
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentException_WhenGameIdIsEmpty()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new UserCollectionItem("user-1", Guid.Empty, CollectionStatus.Wishlist));
    }

    [Fact]
    public void ChangeStatus_ShouldUpdateStatusAndTimestamp_WhenNewStatusIsDifferent()
    {
        // Arrange
        var item = new UserCollectionItem("user-1", Guid.NewGuid(), CollectionStatus.Wishlist);

        // Act
        item.ChangeStatus(CollectionStatus.Played);

        // Assert
        Assert.Equal(CollectionStatus.Played, item.Status);
        Assert.NotNull(item.UpdatedAt);
    }

    [Fact]
    public void ChangeStatus_ShouldNotModifyTimestamp_WhenStatusIsTheSame()
    {
        // Arrange
        var item = new UserCollectionItem("user-1", Guid.NewGuid(), CollectionStatus.Played);

        // Act
        item.ChangeStatus(CollectionStatus.Played);

        // Assert
        Assert.Null(item.UpdatedAt);
    }

    [Fact]
    public void Constructor_Pending_InitializesCorrectly_WithPendingState()
    {
        // Act
        var item = new UserCollectionItem(
            userId: "user-1",
            bggId: 342942,
            pendingTitle: "Ark Nova",
            status: CollectionStatus.InCollection,
            thumbnailUrl: "https://example.com/thumb.jpg",
            yearPublished: 2021
        );

        // Assert
        Assert.Null(item.GameId);
        Assert.True(item.IsPendingCataloging);
        Assert.Equal(342942, item.BggId);
        Assert.Equal("Ark Nova", item.PendingTitle);
        Assert.Equal("https://example.com/thumb.jpg", item.PendingThumbnailUrl);
        Assert.Equal(2021, item.PendingYearPublished);
        Assert.Equal(CollectionStatus.InCollection, item.Status);
    }

    [Fact]
    public void PromoteToCataloged_SetsGameIdAndUpdatesTimestamp()
    {
        // Arrange
        var item = new UserCollectionItem(
            userId: "user-1",
            bggId: 342942,
            pendingTitle: "Ark Nova",
            status: CollectionStatus.InCollection
        );
        Assert.True(item.IsPendingCataloging);

        Guid officialGameId = Guid.NewGuid();

        // Act
        item.PromoteToCataloged(officialGameId);

        // Assert
        Assert.False(item.IsPendingCataloging);
        Assert.Equal(officialGameId, item.GameId);
        Assert.NotNull(item.UpdatedAt);
    }

    [Fact]
    public void PromoteToCataloged_WithEmptyGuid_ThrowsArgumentException()
    {
        // Arrange
        var item = new UserCollectionItem("user-1", 13, "Catan", CollectionStatus.InCollection);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => item.PromoteToCataloged(Guid.Empty));
    }
}

