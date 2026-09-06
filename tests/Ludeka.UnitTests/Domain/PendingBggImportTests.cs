using System;
using Ludeka.Core.Entities;
using Ludeka.Core.Enums;
using Xunit;

namespace Ludeka.UnitTests.Domain;

public class PendingBggImportTests
{
    [Fact]
    public void Constructor_WithValidArguments_InitializesPendingImportCorrectly()
    {
        // Act
        var item = new PendingBggImport(
            bggId: 342942,
            title: "Ark Nova",
            yearPublished: 2021,
            thumbnailUrl: "https://example.com/thumb.jpg",
            coverImageUrl: "https://example.com/cover.jpg"
        );

        // Assert
        Assert.NotEqual(Guid.Empty, item.Id);
        Assert.Equal(342942, item.BggId);
        Assert.Equal("Ark Nova", item.Title);
        Assert.Equal(2021, item.YearPublished);
        Assert.Equal("https://example.com/thumb.jpg", item.ThumbnailUrl);
        Assert.Equal("https://example.com/cover.jpg", item.CoverImageUrl);
        Assert.Equal(1, item.RequestedCount);
        Assert.Equal(CatalogQueueStatus.Pending, item.Status);
        Assert.Null(item.ErrorMessage);
        Assert.Null(item.ProcessedAt);
        Assert.True(item.CreatedAt <= DateTimeOffset.UtcNow);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Constructor_WithInvalidBggId_ThrowsArgumentException(int invalidBggId)
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => new PendingBggImport(invalidBggId, "Juego Válido"));
        Assert.Contains("El identificador BGG debe ser mayor a cero", ex.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithEmptyTitle_ThrowsArgumentException(string? invalidTitle)
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => new PendingBggImport(13, invalidTitle!));
        Assert.Contains("El título del juego no puede estar vacío", ex.Message);
    }

    [Fact]
    public void IncrementRequestCount_IncreasesCountByOne()
    {
        // Arrange
        var item = new PendingBggImport(13, "Catan");
        Assert.Equal(1, item.RequestedCount);

        // Act
        item.IncrementRequestCount();
        item.IncrementRequestCount();

        // Assert
        Assert.Equal(3, item.RequestedCount);
    }

    [Fact]
    public void LifecycleTransitions_WorkProperly()
    {
        // Arrange
        var item = new PendingBggImport(13, "Catan");

        // Act - Processing
        item.MarkAsProcessing();
        Assert.Equal(CatalogQueueStatus.Processing, item.Status);
        Assert.Null(item.ErrorMessage);

        // Act - Failed
        item.MarkAsFailed("Error de red simulado");
        Assert.Equal(CatalogQueueStatus.Failed, item.Status);
        Assert.Equal("Error de red simulado", item.ErrorMessage);

        // Act - Completed
        item.MarkAsCompleted();
        Assert.Equal(CatalogQueueStatus.Completed, item.Status);
        Assert.Null(item.ErrorMessage);
        Assert.NotNull(item.ProcessedAt);
    }
}
