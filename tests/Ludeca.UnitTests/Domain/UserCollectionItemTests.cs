using System;
using Ludeca.Core.Entities;
using Ludeca.Core.Enums;
using Xunit;

namespace Ludeca.UnitTests.Domain;

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
}
