using System;
using System.Collections.Generic;
using Ludeka.Core.Entities;
using Ludeka.Core.Enums;
using Ludeka.Core.ValueObjects;

namespace Ludeka.Application.DTOs;

public record GameDetailDto(
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
    string? Description,
    double BggRating,
    int? BggRank,
    double LudistRating,
    string IdealPlayerCountText,
    ConfrontationType Confrontation,
    GameStyle Style,
    bool IsOfficialSolo,
    AgeRating Age,
    LanguageDependence Language,
    TableFootprint Footprint,
    GameDuration Duration,
    IReadOnlyList<ScalabilityEntry> Scalability,
    IReadOnlyList<SleeveItem> Sleeves
)
{
    public static GameDetailDto FromEntity(Game g) => new(
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
        g.Description,
        g.BggRating,
        g.BggRank,
        g.LudistRating,
        g.IdealPlayerCountText,
        g.Confrontation,
        g.Style,
        g.IsOfficialSolo,
        g.Age,
        g.Language,
        g.Footprint,
        g.Duration,
        g.Scalability.AsReadOnly(),
        g.Sleeves.AsReadOnly()
    );
}
