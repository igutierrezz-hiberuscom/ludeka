using System;
using System.Collections.Generic;
using System.Linq;
using Ludeka.Application.DTOs;
using Ludeka.Core.Entities;
using Ludeka.Core.Enums;
using Ludeka.Core.Helpers;

namespace Ludeka.Infrastructure.YouTube;

/// <summary>
/// Dataset curado y generador heurístico de vídeos de YouTube.
/// Actúa como salvaguardas sin conexión a internet ni consumo de cuota para pruebas unitarias.
/// </summary>
public static class YouTubeSimulationDataset
{
    private record VideoSeed(
        string VideoId,
        string Title,
        string Description,
        string ChannelTitle,
        MediaType Type,
        int DurationSeconds,
        string? ExplicitPlayerCount = null,
        DateTimeOffset? PublishedAt = null
    );

    private static readonly Dictionary<string, List<VideoSeed>> SeededVideosBySlug = new(StringComparer.OrdinalIgnoreCase);

    static YouTubeSimulationDataset()
    {
        InitializeCuratedSeeds();
    }

    public static IReadOnlyList<YouTubeSearchResultDto> GetCuratedOrGeneratedVideos(string gameTitle, Game? game = null)
    {
        var slug = ToSlug(gameTitle);
        if (SeededVideosBySlug.TryGetValue(slug, out var curated) && curated.Count > 0)
        {
            return curated.Select(v => MapToSearchResult(v, game)).ToList();
        }

        // Generador determinista para cualquier otro título
        return GenerateDeterministicVideos(gameTitle, game);
    }

    private static YouTubeSearchResultDto MapToSearchResult(VideoSeed seed, Game? game)
    {
        var url = $"https://www.youtube.com/watch?v=yP5J9q6P4Jg"; // URL válida y canónica
        var embedUrl = $"https://www.youtube-nocookie.com/embed/{seed.VideoId}";
        var thumb = "https://images.unsplash.com/photo-1610890716171-6b1bb98ffd09?auto=format&fit=crop&w=640&q=80";

        var playerBadge = seed.Type == MediaType.Playthrough
            ? seed.ExplicitPlayerCount ?? PlayerCountExtractor.ExtractPlayerBadge(seed.Title, seed.Description, game)
            : null;

        var durationMinutes = seed.DurationSeconds / 60;
        var durationSeconds = seed.DurationSeconds % 60;
        var formattedDuration = $"{durationMinutes}:{durationSeconds:D2}";

        return new YouTubeSearchResultDto(
            VideoId: seed.VideoId,
            Title: seed.Title,
            Description: seed.Description,
            Url: url,
            EmbedUrl: embedUrl,
            ThumbnailUrl: thumb,
            ChannelTitle: seed.ChannelTitle,
            DurationSeconds: seed.DurationSeconds,
            FormattedDuration: formattedDuration,
            SuggestedType: seed.Type,
            ExtractedPlayerBadge: playerBadge,
            RelevanceScore: 90,
            IsReferenceChannel: true,
            ChannelCategory: "Creator",
            PublishedAt: seed.PublishedAt ?? DateTimeOffset.UtcNow.AddMonths(-3)
        );
    }

    private static List<YouTubeSearchResultDto> GenerateDeterministicVideos(string gameTitle, Game? game)
    {
        var results = new List<YouTubeSearchResultDto>();

        // 1. QuickOverview (⚡ Cómo funciona en 2 min)
        results.Add(new YouTubeSearchResultDto(
            VideoId: "mock-quick-" + Math.Abs(gameTitle.GetHashCode()),
            Title: $"{gameTitle} en 2 minutos: Cómo funciona y mecánicas clave",
            Description: $"Vistazo rápido de reglas y dinámica de {gameTitle} para ver si encaja con tu grupo de juego.",
            Url: "https://www.youtube.com/watch?v=yP5J9q6P4Jg",
            EmbedUrl: "https://www.youtube-nocookie.com/embed/mock-quick",
            ThumbnailUrl: "https://images.unsplash.com/photo-1610890716171-6b1bb98ffd09?auto=format&fit=crop&w=640&q=80",
            ChannelTitle: "Zacatrus!",
            DurationSeconds: 118,
            FormattedDuration: "1:58",
            SuggestedType: MediaType.QuickOverview,
            ExtractedPlayerBadge: null,
            RelevanceScore: 85,
            IsReferenceChannel: true,
            ChannelCategory: "Store",
            PublishedAt: DateTimeOffset.UtcNow.AddDays(-20)
        ));

        // 2. Tutorial (10-18 min)
        results.Add(new YouTubeSearchResultDto(
            VideoId: "mock-tut-" + Math.Abs(gameTitle.GetHashCode()),
            Title: $"Cómo se juega a {gameTitle} - Tutorial Completo en Español",
            Description: $"Explicación paso a paso de las reglas de {gameTitle}, preparación de partida y fin del juego.",
            Url: "https://www.youtube.com/watch?v=yP5J9q6P4Jg",
            EmbedUrl: "https://www.youtube-nocookie.com/embed/mock-tut",
            ThumbnailUrl: "https://images.unsplash.com/photo-1606167668584-78701c57f13d?auto=format&fit=crop&w=640&q=80",
            ChannelTitle: "Meepletopía",
            DurationSeconds: 780,
            FormattedDuration: "13:00",
            SuggestedType: MediaType.Tutorial,
            ExtractedPlayerBadge: null,
            RelevanceScore: 92,
            IsReferenceChannel: true,
            ChannelCategory: "Creator",
            PublishedAt: DateTimeOffset.UtcNow.AddMonths(-2)
        ));

        // 3. Playthrough (Partida completa)
        var badge = PlayerCountExtractor.ExtractPlayerBadge($"Partida a 2 jugadores a {gameTitle}", null, game);
        results.Add(new YouTubeSearchResultDto(
            VideoId: "mock-play-" + Math.Abs(gameTitle.GetHashCode()),
            Title: $"{gameTitle} - Partida completa a 2 jugadores en mesa",
            Description: $"Partida de principio a fin de {gameTitle} analizando jugadas y tensión en mesa.",
            Url: "https://www.youtube.com/watch?v=Xh0Y-3L1pSk",
            EmbedUrl: "https://www.youtube-nocookie.com/embed/mock-play",
            ThumbnailUrl: "https://images.unsplash.com/photo-1511512578047-dfb367046420?auto=format&fit=crop&w=640&q=80",
            ChannelTitle: "Análisis Parálisis",
            DurationSeconds: 2700,
            FormattedDuration: "45:00",
            SuggestedType: MediaType.Playthrough,
            ExtractedPlayerBadge: badge,
            RelevanceScore: 95,
            IsReferenceChannel: true,
            ChannelCategory: "Creator",
            PublishedAt: DateTimeOffset.UtcNow.AddMonths(-1)
        ));

        return results;
    }

    private static void InitializeCuratedSeeds()
    {
        // 1. Wingspan
        AddCurated("wingspan", [
            new("yP5J9q6P4Jg", "Wingspan en 2 minutos: Motor de aves y huevos", "Descubre de qué va Wingspan y sus mecánicas en 2 minutos.", "Zacatrus!", MediaType.QuickOverview, 125),
            new("wng-tut-01", "Cómo se juega a WINGSPAN (Tutorial completo)", "Guía oficial paso a paso de reglas de Wingspan en español.", "Meepletopía", MediaType.Tutorial, 890),
            new("wng-play-01", "Partida completa a 2 jugadores a Wingspan", "Partida de demostración a 2 jugadores comentada por Sergio.", "Análisis Parálisis", MediaType.Playthrough, 2850, "Partida a 2")
        ]);

        // 2. Catan
        AddCurated("catan", [
            new("ctn-quick-01", "CATAN en 2 minutos: Negociación y asentamientos", "Resumen ultra rápido de las dinámicas y comercio en Catan.", "Zacatrus!", MediaType.QuickOverview, 115),
            new("yP5J9q6P4Jg", "Cómo se juega a CATAN en 10 minutos (Reglas completas)", "Aprende a jugar al clásico de Klaus Teuber.", "Devir TV", MediaType.Tutorial, 615),
            new("Xh0Y-3L1pSk", "Partida a 3 jugadores a CATAN en directo", "Partida competitiva a 3 jugadores llena de comercio.", "Análisis Parálisis", MediaType.Playthrough, 3200, "Partida a 3")
        ]);

        // 3. Carcassonne
        AddCurated("carcassonne", [
            new("car-quick-01", "Carcassonne en 2 minutos: Losetas y meeples", "Conoce la esencia de Carcassonne en dos minutos.", "Devir TV", MediaType.QuickOverview, 110),
            new("car-tut-01", "Carcassonne: Cómo jugar tutorial en español", "Explicación de colocación de losetas, caminos, ciudades y abadías.", "La Mazmorra de Pacheco", MediaType.Tutorial, 720),
            new("car-play-01", "Duelo a 2 jugadores a Carcassonne (A cuchillo)", "Partida tensa a 2 comensales cerrando murallas.", "Pareja de Ases", MediaType.Playthrough, 2100, "Partida a 2")
        ]);

        // 4. Ark Nova
        AddCurated("ark-nova", [
            new("ark-quick-01", "Ark Nova en 2 minutos: Zoo moderno y conservación", "Vistazo general de las cartas y proyectos zoológicos.", "Maldito Games", MediaType.QuickOverview, 135),
            new("ark-tut-01", "Tutorial Ark Nova: Reglas completas y acción en tablero", "Guía paso a paso para gestionar tu parque zoológico.", "El Agujero de Hobbit", MediaType.Tutorial, 1450),
            new("ark-play-01", "Ark Nova - Partida completa a 2 jugadores", "Partida completa a 2 jugadores con cruce de marcadores.", "Análisis Parálisis", MediaType.Playthrough, 5400, "Partida a 2")
        ]);

        // 5. Terraforming Mars
        AddCurated("terraforming-mars", [
            new("tfm-quick-01", "Terraforming Mars en 2 minutos: Oxígeno, temperatura y océanos", "Resumen dinámico del motor de corporaciones en Marte.", "Zacatrus TV", MediaType.QuickOverview, 130),
            new("tfm-tut-01", "Cómo se juega a Terraforming Mars (Reglas completas)", "Aprende a jugar con todas las fases y producción de recursos.", "Meepletopía", MediaType.Tutorial, 1280),
            new("tfm-play-01", "Terraforming Mars: Partida en solitario", "Superando el reto de terraformación marciana en modo solo.", "Sentido Antihorario", MediaType.Playthrough, 3600, "Partida en solitario")
        ]);
    }

    private static void AddCurated(string slug, List<VideoSeed> seeds)
    {
        SeededVideosBySlug[slug] = seeds;
    }

    private static string ToSlug(string title)
    {
        if (string.IsNullOrWhiteSpace(title)) return string.Empty;
        return title.Trim().ToLowerInvariant()
            .Replace(" ", "-")
            .Replace(":", "")
            .Replace("'", "")
            .Replace("!", "")
            .Replace("?", "");
    }
}
