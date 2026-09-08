using System;
using Ludeka.Core.Enums;

namespace Ludeka.Core.Entities;

public class PendingBggImport
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public int BggId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public int? YearPublished { get; private set; }
    public string? ThumbnailUrl { get; private set; }
    public string? CoverImageUrl { get; private set; }
    public CatalogQueueOrigin Origin { get; private set; } = CatalogQueueOrigin.UserImport;
    public string? ExtractedTitle { get; private set; }
    public int RequestedCount { get; private set; } = 1;
    public CatalogQueueStatus Status { get; private set; } = CatalogQueueStatus.Pending;
    public string? ErrorMessage { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ProcessedAt { get; private set; }

    private PendingBggImport() { }

    public PendingBggImport(
        int bggId,
        string title,
        int? yearPublished = null,
        string? thumbnailUrl = null,
        string? coverImageUrl = null,
        CatalogQueueOrigin origin = CatalogQueueOrigin.UserImport,
        string? extractedTitle = null)
    {
        if (bggId <= 0)
            throw new ArgumentException("El identificador BGG debe ser mayor a cero.", nameof(bggId));
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("El título del juego no puede estar vacío.", nameof(title));

        BggId = bggId;
        Title = title.Trim();
        YearPublished = yearPublished;
        ThumbnailUrl = thumbnailUrl?.Trim();
        CoverImageUrl = coverImageUrl?.Trim();
        Origin = origin;
        ExtractedTitle = string.IsNullOrWhiteSpace(extractedTitle) ? null : extractedTitle.Trim();
        RequestedCount = 1;
        Status = CatalogQueueStatus.Pending;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public void IncrementRequestCount()
    {
        RequestedCount++;
    }

    public void MarkAsProcessing()
    {
        Status = CatalogQueueStatus.Processing;
        ErrorMessage = null;
    }

    public void MarkAsCompleted()
    {
        Status = CatalogQueueStatus.Completed;
        ProcessedAt = DateTimeOffset.UtcNow;
        ErrorMessage = null;
    }

    public void MarkAsFailed(string error)
    {
        Status = CatalogQueueStatus.Failed;
        ErrorMessage = string.IsNullOrWhiteSpace(error) ? "Error desconocido durante la catalogación." : error.Trim();
    }

    public void ResetToPending()
    {
        Status = CatalogQueueStatus.Pending;
        ErrorMessage = null;
    }
}
