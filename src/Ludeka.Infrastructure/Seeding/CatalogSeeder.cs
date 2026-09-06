using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Ludeka.Core.Entities;
using Ludeka.Core.Enums;
using Ludeka.Core.ValueObjects;
using Ludeka.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Ludeka.Infrastructure.Seeding;

public static class CatalogSeeder
{
    private class SeedGameModel
    {
        public int BggId { get; set; }
        public string OriginalTitle { get; set; } = string.Empty;
        public string SpanishTitle { get; set; } = string.Empty;
        public string Designer { get; set; } = string.Empty;
        public string Publisher { get; set; } = string.Empty;
        public int YearPublished { get; set; }
        public string? CoverImageUrl { get; set; }
        public string? ThumbnailUrl { get; set; }
        public string? Description { get; set; }
        public double BggRating { get; set; }
        public int? BggRank { get; set; }
        public double LudistRating { get; set; }
        public string Confrontation { get; set; } = "Competitive";
        public string Style { get; set; } = "Eurogame";
        public bool IsOfficialSolo { get; set; }
        public int BoxAge { get; set; }
        public int CommunityAge { get; set; }
        public string Language { get; set; } = "None";
        public string Footprint { get; set; } = "StandardTable";
        public int MinMinutes { get; set; }
        public int MaxMinutes { get; set; }
        public int EstimatedPerPlayerMinutes { get; set; }
        public List<SeedScalabilityModel>? Scalability { get; set; }
        public List<SeedSleeveModel>? Sleeves { get; set; }
    }

    private class SeedScalabilityModel
    {
        public int PlayerCount { get; set; }
        public string DisplayCount { get; set; } = string.Empty;
        public string Status { get; set; } = "Recommended";
        public int BestVotes { get; set; }
        public int RecommendedVotes { get; set; }
        public int NotRecommendedVotes { get; set; }
    }

    private class SeedSleeveModel
    {
        public string FormatName { get; set; } = string.Empty;
        public double WidthMm { get; set; }
        public double HeightMm { get; set; }
        public int CardCount { get; set; }
        public string? AffiliateUrl { get; set; }
    }

    public static async Task<int> SeedAsync(LudekaDbContext db, CancellationToken ct = default)
    {
        int seededCount = 0;

        if (!await db.Games.AnyAsync(ct))
        {
            string json = ReadSeedJson();
            if (!string.IsNullOrWhiteSpace(json))
            {
                var items = JsonSerializer.Deserialize<List<SeedGameModel>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (items != null && items.Count > 0)
                {
                    var games = items.Select(m =>
                    {
                        var confrontation = Enum.TryParse<ConfrontationType>(m.Confrontation, true, out var c) ? c : ConfrontationType.Competitive;
                        var style = Enum.TryParse<GameStyle>(m.Style, true, out var s) ? s : GameStyle.Eurogame;
                        var language = Enum.TryParse<LanguageDependence>(m.Language, true, out var l) ? l : LanguageDependence.None;
                        var footprint = Enum.TryParse<TableFootprint>(m.Footprint, true, out var f) ? f : TableFootprint.StandardTable;

                        var scalabilityEntries = m.Scalability?.Select(sc =>
                        {
                            var status = Enum.TryParse<ScalabilityStatus>(sc.Status, true, out var st) ? st : ScalabilityStatus.Recommended;
                            return new ScalabilityEntry(sc.PlayerCount, sc.DisplayCount, status, sc.BestVotes, sc.RecommendedVotes, sc.NotRecommendedVotes);
                        }).ToList();

                        var sleeves = m.Sleeves?.Select(sl =>
                            new SleeveItem(sl.FormatName, sl.WidthMm, sl.HeightMm, sl.CardCount, sl.AffiliateUrl)
                        ).ToList();

                        return new Game(
                            bggId: m.BggId,
                            originalTitle: m.OriginalTitle,
                            spanishTitle: m.SpanishTitle,
                            designer: m.Designer,
                            publisher: m.Publisher,
                            yearPublished: m.YearPublished,
                            coverImageUrl: m.CoverImageUrl,
                            thumbnailUrl: m.ThumbnailUrl,
                            description: m.Description,
                            bggRating: m.BggRating,
                            bggRank: m.BggRank,
                            ludistRating: m.LudistRating,
                            confrontation: confrontation,
                            style: style,
                            isOfficialSolo: m.IsOfficialSolo,
                            age: new AgeRating(m.BoxAge, m.CommunityAge),
                            language: language,
                            footprint: footprint,
                            duration: new GameDuration(m.MinMinutes, m.MaxMinutes, m.EstimatedPerPlayerMinutes),
                            scalability: scalabilityEntries,
                            sleeves: sleeves
                        );
                    }).ToList();

                    await db.Games.AddRangeAsync(games, ct);
                    await db.SaveChangesAsync(ct);
                    seededCount += games.Count;
                }
            }
        }

        // Semillado de veredictos iniciales de la mesa fundadora
        await SeedFoundingVerdictsAsync(db, ct);

        // Semillado del Hub Multimedia (YouTube e Instagram)
        await SeedMediaItemsAsync(db, ct);

        return seededCount;
    }

    private static async Task SeedFoundingVerdictsAsync(LudekaDbContext db, CancellationToken ct)
    {
        if (await db.FoundingVerdicts.AnyAsync(ct))
        {
            return; // Ya existen veredictos fundadores semillados
        }

        var allGames = await db.Games.ToListAsync(ct);
        if (allGames.Count == 0) return;

        var verdicts = new List<FoundingVerdict>();

        // 1. Wingspan
        var wingspan = allGames.FirstOrDefault(g => g.Slug == "wingspan");
        if (wingspan != null)
        {
            verdicts.Add(new FoundingVerdict(
                wingspan.Id,
                "fundador-ludeka-01",
                "Mesa Fundadora (Lucía & Javi)",
                FoundingRecommendation.MustPlay,
                "Wingspan es una joya moderna del motor de cartas. Su producción visual, rigor ornitológico y fluidez de turnos lo convierten en una experiencia imprescindible en cualquier ludoteca que busque elegancia sin agobios matemáticos.",
                "A 2 jugadores brilla de forma sobresaliente. La rotación de objetivos finales es rápida, el entreturno es prácticamente nulo (~45 minutos de partida) y la competencia por el comedero de dados genera la tensión justa sin resultar lesiva.",
                "A partir de 9 o 10 años funciona de maravilla si se hace una primera partida guiada. Los iconos en las cartas son muy claros y la presencia física de los huevitos y la torre comedero fascina a pequeños y mayores.",
                [
                    new FoundingPhoto("https://images.unsplash.com/photo-1610890716171-6b1bb98ffd09?auto=format&fit=crop&w=1200&q=80", "Despliegue real de Wingspan en mesa de salón: tableros personales y comedero de dados."),
                    new FoundingPhoto("https://images.unsplash.com/photo-1606167668584-78701c57f13d?auto=format&fit=crop&w=1200&q=80", "Detalle de los componentes: cartas de aves ibéricas y reserva de huevitos pastel.")
                ]
            ));
        }

        // 2. Carcassonne
        var carcassonne = allGames.FirstOrDefault(g => g.Slug == "carcassonne");
        if (carcassonne != null)
        {
            verdicts.Add(new FoundingVerdict(
                carcassonne.Id,
                "fundador-ludeka-01",
                "Mesa Fundadora (Lucía & Javi)",
                FoundingRecommendation.RecommendedWithAdaptations,
                "Un clásico eterno de colocación de losetas que nunca pasa de moda. Sencillo de explicar en 3 minutos pero con una profundidad táctica y mala uva insospechada cuando se juega con conocedores.",
                "A 2 jugadores es un duelo a cuchillo brutal. Cada loseta cuenta, el control de las ciudades rivales es despiadado y el bloqueo de meeples convierte el juego en un pulso psicológico de alta intensidad.",
                "Para jugar en familia con niños pequeños (6-8 años), la adaptación obligatoria de la mesa es jugar SIN granjeros/campos. De este modo, la puntuación es inmediata, visual y divertida sin cálculos finales enrevesados.",
                [
                    new FoundingPhoto("https://images.unsplash.com/photo-1610890716171-6b1bb98ffd09?auto=format&fit=crop&w=1200&q=80", "Mapa medieval de Carcassonne desplegado tras 35 minutos de partida en mesa de centro.")
                ]
            ));
        }

        // 3. Azul
        var azul = allGames.FirstOrDefault(g => g.Slug == "azul");
        if (azul != null)
        {
            verdicts.Add(new FoundingVerdict(
                azul.Id,
                "fundador-ludeka-01",
                "Mesa Fundadora (Lucía & Javi)",
                FoundingRecommendation.MustPlay,
                "Azul ofrece un equilibrio perfecto entre belleza estética de azulejos portugueses y crueldad táctica en el descarte de fichas a la línea de suelo.",
                "A 2 jugadores es una partida de ajedrez implacable: calcular lo que dejas en el centro de la mesa para obligar a tu rival a comerse 5 azulejos rotos es puro disfrute competitivo.",
                "Muy accesible para jugar en familia desde los 8 años. Los componentes de baquelita tienen un tacto inmejorable que atrae a jugadores no habituales de forma instantánea.",
                [
                    new FoundingPhoto("https://images.unsplash.com/photo-1606167668584-78701c57f13d?auto=format&fit=crop&w=1200&q=80", "Tableros individuales completados en partida real a 2 jugadores.")
                ]
            ));
        }

        if (verdicts.Count > 0)
        {
            await db.FoundingVerdicts.AddRangeAsync(verdicts, ct);
            await db.SaveChangesAsync(ct);
        }
    }

    private static async Task SeedMediaItemsAsync(LudekaDbContext db, CancellationToken ct)
    {
        if (await db.MediaItems.AnyAsync(ct))
        {
            return; // Ya existen medios semillados
        }

        var allGames = await db.Games.ToListAsync(ct);
        if (allGames.Count == 0) return;

        var mediaList = new List<MediaItem>();

        var catan = allGames.FirstOrDefault(g => g.Slug == "catan");
        if (catan != null)
        {
            mediaList.Add(new MediaItem(
                MediaType.Tutorial,
                MediaPlatform.YouTube,
                "Cómo se juega a CATAN en 10 minutos (Reglas completas)",
                "https://www.youtube.com/watch?v=kYQf7p_catan10",
                "https://images.unsplash.com/photo-1610890716171-6b1bb98ffd09?auto=format&fit=crop&w=640&q=80",
                "@zacatrustv",
                gameId: catan.Id,
                durationSeconds: 615,
                status: ModerationStatus.Approved
            ));

            mediaList.Add(new MediaItem(
                MediaType.Playthrough,
                MediaPlatform.YouTube,
                "Partida completa a CATAN | Guerra de ovejas y trigo a 4 jugadores",
                "https://www.youtube.com/watch?v=kYQf7p_catanplay",
                "https://images.unsplash.com/photo-1606167668584-78701c57f13d?auto=format&fit=crop&w=640&q=80",
                "@analisisparalisis",
                gameId: catan.Id,
                durationSeconds: 3420,
                playerCountBadge: "Partida a 4",
                status: ModerationStatus.Approved
            ));

            mediaList.Add(new MediaItem(
                MediaType.ShortReel,
                MediaPlatform.YouTube,
                "Short: ¿Por qué nadie cambia ovejas en el 7 de Catan?",
                "https://www.youtube.com/shorts/catan_ovejas_short",
                "https://images.unsplash.com/photo-1511512578047-dfb367046420?auto=format&fit=crop&w=640&q=80",
                "@analisisparalisis",
                gameId: catan.Id,
                durationSeconds: 48,
                status: ModerationStatus.Approved
            ));

            mediaList.Add(new MediaItem(
                MediaType.Tutorial,
                MediaPlatform.YouTube,
                "Unboxing y componentes de Catan Plus en detalle",
                "https://www.youtube.com/watch?v=catan_plus_unboxing",
                "https://images.unsplash.com/photo-1578632767115-351597cf2477?auto=format&fit=crop&w=640&q=80",
                "@comunidad_catan",
                gameId: catan.Id,
                durationSeconds: 780,
                status: ModerationStatus.PendingApproval
            ));
        }

        var tfm = allGames.FirstOrDefault(g => g.Slug == "terraforming-mars");
        if (tfm != null)
        {
            mediaList.Add(new MediaItem(
                MediaType.Tutorial,
                MediaPlatform.YouTube,
                "Aprende a jugar a Terraforming Mars: Guía completa de corporaciones",
                "https://www.youtube.com/watch?v=tfm_tutorial_ap",
                "https://images.unsplash.com/photo-1614728894747-a83421e2b9c9?auto=format&fit=crop&w=640&q=80",
                "@analisisparalisis",
                gameId: tfm.Id,
                durationSeconds: 1540,
                status: ModerationStatus.Approved
            ));

            mediaList.Add(new MediaItem(
                MediaType.Playthrough,
                MediaPlatform.YouTube,
                "Terraforming Mars: Duelo táctico en directo a 2 corporaciones",
                "https://www.youtube.com/watch?v=tfm_playthrough_2p",
                "https://images.unsplash.com/photo-1451187580459-43490279c0fa?auto=format&fit=crop&w=640&q=80",
                "@rinconlegacy",
                gameId: tfm.Id,
                durationSeconds: 5800,
                playerCountBadge: "Partida a 2",
                status: ModerationStatus.Approved
            ));

            mediaList.Add(new MediaItem(
                MediaType.InstagramPost,
                MediaPlatform.Instagram,
                "Oxígeno al 14%, océanos completados y una producción de titanio demencial. ¡Victoria épica con Tharsis Republic!",
                "https://www.instagram.com/p/tfm_marte_epico/",
                "https://images.unsplash.com/photo-1614728894747-a83421e2b9c9?auto=format&fit=crop&w=640&q=80",
                "@eldardoludico",
                gameId: tfm.Id,
                likesCount: 524,
                excerpt: "Oxígeno al 14%, océanos completados y una producción de titanio demencial. ¡Qué gran juego!",
                status: ModerationStatus.Approved
            ));
        }

        var wingspan = allGames.FirstOrDefault(g => g.Slug == "wingspan");
        if (wingspan != null)
        {
            mediaList.Add(new MediaItem(
                MediaType.Tutorial,
                MediaPlatform.YouTube,
                "Wingspan: Tutorial oficial y mecánicas de hábitats",
                "https://www.youtube.com/watch?v=wingspan_tuto_meeple",
                "https://images.unsplash.com/photo-1444464666168-49d633b86797?auto=format&fit=crop&w=640&q=80",
                "@meepletopia",
                gameId: wingspan.Id,
                durationSeconds: 985,
                status: ModerationStatus.Approved
            ));

            mediaList.Add(new MediaItem(
                MediaType.Playthrough,
                MediaPlatform.YouTube,
                "Partida a 2 jugadores a Wingspan: Comedero a tope y rapaces",
                "https://www.youtube.com/watch?v=wingspan_play_221b",
                "https://images.unsplash.com/photo-1552728089-57bdde30beb3?auto=format&fit=crop&w=640&q=80",
                "@juegosdemesa221b",
                gameId: wingspan.Id,
                durationSeconds: 3120,
                playerCountBadge: "Partida a 2",
                status: ModerationStatus.Approved
            ));

            mediaList.Add(new MediaItem(
                MediaType.InstagramPost,
                MediaPlatform.Instagram,
                "Los componentes de Wingspan son una fiesta para la vista. El comedero y los huevitos pastel dan ganas de jugar siempre.",
                "https://www.instagram.com/p/wingspan_fotos_mesa/",
                "https://images.unsplash.com/photo-1444464666168-49d633b86797?auto=format&fit=crop&w=640&q=80",
                "@ludist_app",
                gameId: wingspan.Id,
                likesCount: 680,
                excerpt: "Los componentes de Wingspan son una fiesta para la vista. El comedero y los huevitos pastel...",
                status: ModerationStatus.Approved
            ));
        }

        var azul = allGames.FirstOrDefault(g => g.Slug == "azul");
        if (azul != null)
        {
            mediaList.Add(new MediaItem(
                MediaType.Tutorial,
                MediaPlatform.YouTube,
                "Cómo jugar a AZUL en 5 minutos: Reglas claras y puntuación",
                "https://www.youtube.com/watch?v=azul_tutorial_rapido",
                "https://images.unsplash.com/photo-1513519245088-0e12902e5a38?auto=format&fit=crop&w=640&q=80",
                "@eltroquel",
                gameId: azul.Id,
                durationSeconds: 410,
                status: ModerationStatus.Approved
            ));

            mediaList.Add(new MediaItem(
                MediaType.Playthrough,
                MediaPlatform.YouTube,
                "Duelo despiadado a 2 jugadores en Azul | No hay amigos en la línea de suelo",
                "https://www.youtube.com/watch?v=azul_playthrough_2p",
                "https://images.unsplash.com/photo-1528459801416-a9e53bbf4e17?auto=format&fit=crop&w=640&q=80",
                "@zacatrustv",
                gameId: azul.Id,
                durationSeconds: 1850,
                playerCountBadge: "Partida a 2",
                status: ModerationStatus.Approved
            ));
        }

        var arknova = allGames.FirstOrDefault(g => g.Slug == "ark-nova");
        if (arknova != null)
        {
            mediaList.Add(new MediaItem(
                MediaType.Tutorial,
                MediaPlatform.YouTube,
                "Ark Nova: Cómo jugar al eurogame de zoos más aclamado",
                "https://www.youtube.com/watch?v=arknova_como_jugar",
                "https://images.unsplash.com/photo-1534567153574-2b12153a87f0?auto=format&fit=crop&w=640&q=80",
                "@analisisparalisis",
                gameId: arknova.Id,
                durationSeconds: 1980,
                status: ModerationStatus.Approved
            ));

            mediaList.Add(new MediaItem(
                MediaType.Playthrough,
                MediaPlatform.YouTube,
                "Partida completa a 2 jugadores a Ark Nova | Cierres de recinto y proyectos de conservación",
                "https://www.youtube.com/watch?v=arknova_partida_2p",
                "https://images.unsplash.com/photo-1504173010664-32509aeebb62?auto=format&fit=crop&w=640&q=80",
                "@meepletopia",
                gameId: arknova.Id,
                durationSeconds: 6850,
                playerCountBadge: "Partida a 2",
                status: ModerationStatus.Approved
            ));

            mediaList.Add(new MediaItem(
                MediaType.Playthrough,
                MediaPlatform.YouTube,
                "Mi primera partida en club a Ark Nova a 3 jugadores",
                "https://www.youtube.com/watch?v=arknova_partida_3p_pendiente",
                "https://images.unsplash.com/photo-1504173010664-32509aeebb62?auto=format&fit=crop&w=640&q=80",
                "@resenas_novatas",
                gameId: arknova.Id,
                durationSeconds: 7100,
                playerCountBadge: "Partida a 3",
                status: ModerationStatus.PendingApproval
            ));
        }

        // Elementos Huérfanos (GameId == null) para la bandeja de moderación
        mediaList.Add(new MediaItem(
            MediaType.Tutorial,
            MediaPlatform.YouTube,
            "Top 10 novedades presentadas en la feria de Córdoba",
            "https://www.youtube.com/watch?v=feria_cordoba_top10",
            "https://images.unsplash.com/photo-1511512578047-dfb367046420?auto=format&fit=crop&w=640&q=80",
            "@eltroquel",
            gameId: null,
            durationSeconds: 1140,
            status: ModerationStatus.PendingApproval
        ));

        mediaList.Add(new MediaItem(
            MediaType.InstagramPost,
            MediaPlatform.Instagram,
            "Mesa de domingo repleta de meeples, cartas y dados. ¿Qué estáis jugando vosotros hoy?",
            "https://www.instagram.com/p/domingo_ludico_mesa/",
            "https://images.unsplash.com/photo-1606167668584-78701c57f13d?auto=format&fit=crop&w=640&q=80",
            "@eldardoludico",
            gameId: null,
            likesCount: 390,
            excerpt: "Mesa de domingo repleta de meeples, cartas y dados. ¿Qué estáis jugando vosotros hoy?",
            status: ModerationStatus.PendingApproval
        ));

        mediaList.Add(new MediaItem(
            MediaType.ShortReel,
            MediaPlatform.YouTube,
            "Short: Cómo enfundar tus cartas sin que queden burbujas",
            "https://www.youtube.com/shorts/como_enfundar_express",
            "https://images.unsplash.com/photo-1578632767115-351597cf2477?auto=format&fit=crop&w=640&q=80",
            "@zacatrustv",
            gameId: null,
            durationSeconds: 45,
            status: ModerationStatus.PendingApproval
        ));

        if (mediaList.Count > 0)
        {
            await db.MediaItems.AddRangeAsync(mediaList, ct);
            await db.SaveChangesAsync(ct);
        }
    }

    private static string ReadSeedJson()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = assembly.GetManifestResourceNames()
            .FirstOrDefault(n => n.EndsWith("seed-games.json", StringComparison.OrdinalIgnoreCase));

        if (resourceName != null)
        {
            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream != null)
            {
                using var reader = new StreamReader(stream);
                return reader.ReadToEnd();
            }
        }

        // Fallback al sistema de archivos local
        string localPath = Path.Combine(AppContext.BaseDirectory, "Seeding", "seed-games.json");
        if (File.Exists(localPath))
        {
            return File.ReadAllText(localPath);
        }

        string directPath = Path.Combine(Directory.GetCurrentDirectory(), "src", "Ludeka.Infrastructure", "Seeding", "seed-games.json");
        if (File.Exists(directPath))
        {
            return File.ReadAllText(directPath);
        }

        return string.Empty;
    }
}
