using System;
using Ludeka.Core.Enums;

namespace Ludeka.Application.DTOs;

/// <summary>
/// Representa el resultado de una búsqueda quirúrgica de YouTube (real o simulada).
/// </summary>
public record YouTubeSearchResultDto(
    string VideoId,
    string Title,
    string Description,
    string Url,
    string EmbedUrl,
    string ThumbnailUrl,
    string ChannelTitle,
    int? DurationSeconds,
    string FormattedDuration,
    MediaType SuggestedType,
    string? ExtractedPlayerBadge,
    int RelevanceScore,
    bool IsReferenceChannel,
    string? ChannelCategory,
    DateTimeOffset? PublishedAt
);

/// <summary>
/// Solicitud de incorporación (ingesta) de un vídeo de YouTube a la base de datos de Ludeka.
/// </summary>
public record YouTubeIngestRequestDto(
    Guid GameId,
    string VideoId,
    MediaType Type,
    string Title,
    string Url,
    string ThumbnailUrl,
    string ChannelTitle,
    int? DurationSeconds = null,
    string? PlayerCountBadge = null,
    bool AutoApprove = false
);
