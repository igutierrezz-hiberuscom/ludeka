using System;

namespace Ludeka.Application.DTOs;

public record GamePlayLogDto(
    Guid Id,
    Guid GameId,
    string GameTitle,
    string? GameCoverUrl,
    string GameSlug,
    DateTimeOffset PlayDate,
    string Location,
    int PlayerCount,
    int? DurationMinutes,
    string? Comment,
    DateTimeOffset CreatedAt
);

public record RecordPlayRequest(
    Guid GameId,
    DateTimeOffset PlayDate,
    string Location,
    int PlayerCount,
    int? DurationMinutes = null,
    string? Comment = null
);

public record UserPlaysStatsDto(
    int TotalPlays,
    string? MostPlayedGameTitle,
    int MostPlayedGameCount,
    string? FavoriteLocation,
    int? MostCommonPlayerCount,
    int PlaysThisMonth
);
