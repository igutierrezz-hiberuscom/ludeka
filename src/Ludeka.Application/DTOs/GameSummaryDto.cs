using System;
using Ludeka.Core.Entities;
using Ludeka.Core.Enums;

namespace Ludeka.Application.DTOs;

public record GameSummaryDto(
    Guid Id,
    int BggId,
    string Slug,
    string SpanishTitle,
    string OriginalTitle,
    string Designer,
    string Publisher,
    int YearPublished,
    string? CoverImageUrl,
    string? ThumbnailUrl,
    double BggRating,
    int? BggRank,
    double LudistRating,
    string IdealPlayerCountText,
    ConfrontationType Confrontation,
    GameStyle Style,
    bool IsOfficialSolo,
    bool IsAccessibleEarlier,
    int CommunityAge,
    int BoxAge,
    LanguageDependence Language,
    TableFootprint Footprint,
    int EstimatedPerPlayerMinutes,
    GameType Type = GameType.BaseGame,
    string? BaseGameTitle = null
)
{
    public bool IsExpansion => Type == GameType.Expansion || Type == GameType.StandaloneExpansion;

    public static GameSummaryDto FromEntity(Game g) => FromEntity(g, null);

    public static GameSummaryDto FromEntity(Game g, string? baseGameTitle) => new(
        g.Id,
        g.BggId,
        g.Slug,
        g.SpanishTitle,
        g.OriginalTitle,
        g.Designer,
        g.Publisher,
        g.YearPublished,
        g.CoverImageUrl,
        g.ThumbnailUrl,
        g.BggRating,
        g.BggRank,
        g.LudistRating,
        g.IdealPlayerCountText,
        g.Confrontation,
        g.Style,
        g.IsOfficialSolo,
        g.Age.IsAccessibleEarlier,
        g.Age.CommunityAge,
        g.Age.BoxAge,
        g.Language,
        g.Footprint,
        g.Duration.EstimatedPerPlayerMinutes,
        g.Type,
        baseGameTitle
    );
}
