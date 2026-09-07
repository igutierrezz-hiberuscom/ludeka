using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Ludeka.Core.Enums;
using Ludeka.Core.ValueObjects;

namespace Ludeka.Core.Entities;

public partial class Game
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public int BggId { get; private set; }
    public string Slug { get; private set; } = string.Empty;
    public string OriginalTitle { get; private set; } = string.Empty;
    public string SpanishTitle { get; private set; } = string.Empty;
    public string Designer { get; private set; } = string.Empty;
    public string Publisher { get; private set; } = string.Empty;
    public int YearPublished { get; private set; }
    public string? CoverImageUrl { get; private set; }
    public string? ThumbnailUrl { get; private set; }
    public string? Description { get; private set; }
    public double BggRating { get; private set; }
    public int? BggRank { get; private set; }
    public double LudistRating { get; private set; }
    public ConfrontationType Confrontation { get; private set; }
    public GameStyle Style { get; private set; }
    public bool IsOfficialSolo { get; private set; }
    public AgeRating Age { get; private set; } = null!;
    public LanguageDependence Language { get; private set; }
    public TableFootprint Footprint { get; private set; }
    public GameDuration Duration { get; private set; } = null!;
    public List<ScalabilityEntry> Scalability { get; private set; } = [];
    public List<SleeveItem> Sleeves { get; private set; } = [];

    // --- Soporte de Expansiones y Ecosistema (Incremento 8) ---
    public GameType Type { get; private set; } = GameType.BaseGame;
    public Guid? BaseGameId { get; private set; }
    public Game? BaseGame { get; private set; }
    public List<Game> Expansions { get; private set; } = [];
    public ExpansionNecessity? ExpansionNecessity { get; private set; }
    public List<ExpansionImpactTag> ImpactTags { get; private set; } = [];
    public string? WhatItBringsSummary { get; private set; }
    public int? ExtraPlayerCount { get; private set; }
    public int? ExtraDurationMinutes { get; private set; }

    public bool IsExpansion => Type == GameType.Expansion || Type == GameType.StandaloneExpansion;

    // Constructor privado para EF Core
    private Game() { }

    public Game(
        int bggId,
        string originalTitle,
        string spanishTitle,
        string designer,
        string publisher,
        int yearPublished,
        string? coverImageUrl,
        string? thumbnailUrl,
        string? description,
        double bggRating,
        int? bggRank,
        double ludistRating,
        ConfrontationType confrontation,
        GameStyle style,
        bool isOfficialSolo,
        AgeRating age,
        LanguageDependence language,
        TableFootprint footprint,
        GameDuration duration,
        IEnumerable<ScalabilityEntry>? scalability = null,
        IEnumerable<SleeveItem>? sleeves = null,
        string? customSlug = null,
        GameType type = GameType.BaseGame,
        Guid? baseGameId = null,
        ExpansionNecessity? expansionNecessity = null,
        IEnumerable<ExpansionImpactTag>? impactTags = null,
        string? whatItBringsSummary = null,
        int? extraPlayerCount = null,
        int? extraDurationMinutes = null)
    {
        if (bggId <= 0) throw new ArgumentOutOfRangeException(nameof(bggId), "El BggId debe ser positivo.");
        if (string.IsNullOrWhiteSpace(originalTitle)) throw new ArgumentException("El título original no puede estar vacío.", nameof(originalTitle));

        BggId = bggId;
        OriginalTitle = originalTitle.Trim();
        SpanishTitle = string.IsNullOrWhiteSpace(spanishTitle) ? OriginalTitle : spanishTitle.Trim();
        Designer = designer?.Trim() ?? string.Empty;
        Publisher = publisher?.Trim() ?? string.Empty;
        YearPublished = yearPublished;
        CoverImageUrl = coverImageUrl?.Trim();
        ThumbnailUrl = thumbnailUrl?.Trim();
        Description = description?.Trim();
        BggRating = Math.Clamp(bggRating, 0.0, 10.0);
        BggRank = bggRank;
        LudistRating = Math.Clamp(ludistRating, 0.0, 10.0);
        Confrontation = confrontation;
        Style = style;
        IsOfficialSolo = isOfficialSolo;
        Age = age ?? throw new ArgumentNullException(nameof(age));
        Language = language;
        Footprint = footprint;
        Duration = duration ?? throw new ArgumentNullException(nameof(duration));

        if (scalability != null) Scalability.AddRange(scalability);
        if (sleeves != null) Sleeves.AddRange(sleeves);

        Slug = string.IsNullOrWhiteSpace(customSlug)
            ? GenerateSlug(SpanishTitle)
            : GenerateSlug(customSlug);

        Type = type;
        BaseGameId = baseGameId;
        ExpansionNecessity = expansionNecessity;
        if (impactTags != null) ImpactTags.AddRange(impactTags);
        WhatItBringsSummary = whatItBringsSummary?.Trim();
        ExtraPlayerCount = extraPlayerCount;
        ExtraDurationMinutes = extraDurationMinutes;
    }

    public string IdealPlayerCountText => CalculateIdealPlayerCountText();

    public string CalculateIdealPlayerCountText()
    {
        var bestEntries = Scalability
            .Where(s => s.Status == ScalabilityStatus.MustPlay)
            .OrderBy(s => s.PlayerCount)
            .ToList();

        if (bestEntries.Count == 0)
        {
            // Fallback al mejor recomendado si no hay MustPlay
            bestEntries = Scalability
                .Where(s => s.Status == ScalabilityStatus.Recommended)
                .OrderBy(s => s.PlayerCount)
                .ToList();
        }

        if (bestEntries.Count == 0)
        {
            return "Sin datos ideales";
        }

        if (bestEntries.Count == 1)
        {
            return bestEntries[0].PlayerCount >= 7
                ? "Ideal: 7+ jugadores"
                : $"Ideal: {bestEntries[0].PlayerCount} jugadores";
        }

        // Si son consecutivos (ej. 2 y 3)
        int min = bestEntries.First().PlayerCount;
        int max = bestEntries.Last().PlayerCount;
        if (max - min == bestEntries.Count - 1)
        {
            return $"Ideal: {min}-{max} jugadores";
        }

        // Si son dispersos
        string counts = string.Join(", ", bestEntries.Select(e => e.DisplayCount));
        return $"Ideal: {counts} jugadores";
    }

    public void UpdateLudistRating(double newAverageRating)
    {
        LudistRating = Math.Clamp(Math.Round(newAverageRating, 1), 0.0, 10.0);
    }

    public void UpdateImages(string? coverImageUrl, string? thumbnailUrl = null)
    {
        CoverImageUrl = coverImageUrl?.Trim();
        ThumbnailUrl = thumbnailUrl?.Trim();
    }

    public void ConfigureExpansion(
        Guid baseGameId,
        ExpansionNecessity necessity,
        IEnumerable<ExpansionImpactTag> impactTags,
        string whatItBringsSummary,
        int? extraPlayerCount = null,
        int? extraDurationMinutes = null)
    {
        if (baseGameId == Guid.Empty) throw new ArgumentException("El BaseGameId no puede estar vacío.", nameof(baseGameId));

        Type = GameType.Expansion;
        BaseGameId = baseGameId;
        ExpansionNecessity = necessity;
        ImpactTags.Clear();
        if (impactTags != null) ImpactTags.AddRange(impactTags);
        WhatItBringsSummary = whatItBringsSummary?.Trim();
        ExtraPlayerCount = extraPlayerCount;
        ExtraDurationMinutes = extraDurationMinutes;
    }

    public static string GenerateSlug(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;

        // 1. Descomposición canónica para separar tildes y diacríticos
        string normalized = input.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();

        foreach (char c in normalized)
        {
            UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(c);
            if (uc != UnicodeCategory.NonSpacingMark)
            {
                sb.Append(c);
            }
        }

        string cleanText = sb.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();

        // 2. Reemplazar caracteres no alfanuméricos por guiones
        cleanText = Regex.Replace(cleanText, @"[^a-z0-9\s-]", "");

        // 3. Reemplazar espacios y guiones múltiples por un único guión
        cleanText = Regex.Replace(cleanText, @"[\s-]+", "-").Trim('-');

        return cleanText;
    }
}
