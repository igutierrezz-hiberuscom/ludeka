using System;
using System.Collections.Generic;
using Ludeka.Core.Entities;
using Ludeka.Core.Enums;

namespace Ludeka.Application.DTOs;

public record MediaItemDto(
    Guid Id,
    Guid? GameId,
    string? GameTitle,
    MediaType Type,
    MediaPlatform Platform,
    string Title,
    string Url,
    string? EmbedUrl,
    string ThumbnailUrl,
    string AuthorChannel,
    int? DurationSeconds,
    string FormattedDuration,
    string? PlayerCountBadge,
    int? LikesCount,
    string? Excerpt,
    ModerationStatus Status,
    bool IsBroken,
    bool IsOrphan,
    DateTimeOffset PublishedAt,
    DateTimeOffset CreatedAt
)
{
    public static MediaItemDto FromDomain(MediaItem item, string? gameTitle = null)
    {
        return new MediaItemDto(
            item.Id,
            item.GameId,
            gameTitle ?? item.Game?.SpanishTitle,
            item.Type,
            item.Platform,
            item.Title,
            item.Url,
            item.EmbedUrl,
            item.ThumbnailUrl,
            item.AuthorChannel,
            item.DurationSeconds,
            item.FormattedDuration,
            item.PlayerCountBadge,
            item.LikesCount,
            item.Excerpt,
            item.Status,
            item.IsBroken,
            item.IsOrphan,
            item.PublishedAt,
            item.CreatedAt
        );
    }
}

public record GameMediaHubDto(
    Guid GameId,
    IReadOnlyList<MediaItemDto> Tutorials,
    IReadOnlyList<MediaItemDto> Playthroughs,
    IReadOnlyList<MediaItemDto> InstagramPosts,
    IReadOnlyList<MediaItemDto> ShortReels
)
{
    public int TotalCount => Tutorials.Count + Playthroughs.Count + InstagramPosts.Count + ShortReels.Count;
    public bool HasMedia => TotalCount > 0;
    public bool HasTutorials => Tutorials.Count > 0;
    public bool HasPlaythroughs => Playthroughs.Count > 0;
    public bool HasSocial => InstagramPosts.Count > 0 || ShortReels.Count > 0;
}

public record ModerateMediaItemRequest(
    Guid MediaId,
    bool Approve
);

public record AssignMediaGameRequest(
    Guid MediaId,
    Guid GameId
);

public record BrokenLinkReportDto(
    int TotalChecked,
    int BrokenCount,
    IReadOnlyList<MediaItemDto> BrokenItems
);
