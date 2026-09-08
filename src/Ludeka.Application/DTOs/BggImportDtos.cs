using System;
using System.Collections.Generic;
using Ludeka.Core.Enums;

namespace Ludeka.Application.DTOs;

public record BggCollectionItemDto(
    int BggId,
    string Title,
    int? YearPublished,
    string? ThumbnailUrl,
    string? CoverImageUrl,
    bool IsOwned,
    bool IsWishlist,
    bool IsWantToBuy,
    int NumPlays
);

public record BggSearchResultDto(
    int BggId,
    string Title,
    int? YearPublished,
    bool IsAlreadyCataloged = false,
    string? ExistingGameSlug = null,
    Guid? ExistingGameId = null
);

public record BggImportRequest(
    string Username,
    bool ImportOwned = true,
    bool ImportWishlist = true
);

public record BggImportResultDto(
    int TotalProcessed,
    int ImportedToCollection,
    int EnqueuedForCataloging,
    List<string> Errors
);

public record CatalogQueueItemDto(
    Guid Id,
    int BggId,
    string Title,
    int? YearPublished,
    string? ThumbnailUrl,
    int RequestedCount,
    CatalogQueueStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ProcessedAt,
    string? ErrorMessage = null,
    CatalogQueueOrigin Origin = CatalogQueueOrigin.UserImport,
    string? ExtractedTitle = null
);

public record ProcessQueueResultDto(
    int ProcessedCount,
    int SuccessCount,
    int FailedCount,
    List<string> CatalogedGameTitles
);

public record AddBggGameRequest(
    int BggId,
    CollectionStatus Status
);
