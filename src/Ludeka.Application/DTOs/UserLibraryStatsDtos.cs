using System;
using System.Collections.Generic;
using Ludeka.Core.Enums;

namespace Ludeka.Application.DTOs;

public record ShelfTimeStatsDto(
    int TotalMinMinutes,
    int TotalMaxMinutes,
    double TotalMinHours,
    double TotalMaxHours,
    string FormattedShelfHours
);

public record StylePercentageDto(
    GameStyle Style,
    string StyleDisplayName,
    int GameCount,
    double Percentage,
    string ColorHex
);

public record PlayerDnaDistributionDto(
    List<StylePercentageDto> Styles,
    string DominantStyleName,
    double CooperativePercentage,
    int CooperativeGamesCount,
    double SoloReadyPercentage,
    int SoloReadyGamesCount
);

public record ScalabilityCountDto(
    int PlayerCount,
    string DisplayCount,
    int OptimizedGamesCount,
    bool IsSweetSpot
);

public record ScalabilitySweetSpotDto(
    List<ScalabilityCountDto> Curve,
    List<int> SweetSpotPlayerCounts,
    string SweetSpotSummaryText
);

public record TopEntityStatDto(
    string Name,
    int Count,
    double Percentage
);

public record SleeveFormatStatDto(
    string FormatName,
    int CardCount,
    int PacksNeeded
);

public record SleeveProtectionRadarDto(
    int TotalCards,
    int TotalPacksEstimated,
    int GamesRequiringSleevesCount,
    List<SleeveFormatStatDto> TopFormats
);

public record PlayerBadgeDto(
    string RankName,
    int RankLevel,
    string TraitName,
    string TraitDescription,
    string IconEmoji,
    string SummaryTagline
);

public record UserLibraryStatsDto(
    string UserId,
    string UserName,
    int TotalGamesInCollection,
    int TotalPlayed,
    int TotalWishlist,
    int TotalWantToBuy,
    int TotalActiveLoans,
    ShelfTimeStatsDto ShelfTime,
    PlayerDnaDistributionDto DnaDistribution,
    ScalabilitySweetSpotDto Scalability,
    List<TopEntityStatDto> TopDesigners,
    List<TopEntityStatDto> TopPublishers,
    SleeveProtectionRadarDto SleevesRadar,
    PlayerBadgeDto Badge
);

public record PublicUserProfileDto(
    string UserId,
    string UserName,
    UserLibraryStatsDto Stats,
    List<UserCollectionItemDto> ShelfGames
);
