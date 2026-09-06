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
        else
        {
            // Sincronizar carátulas de juegos existentes para garantizar imágenes locales
            string json = ReadSeedJson();
            if (!string.IsNullOrWhiteSpace(json))
            {
                var items = JsonSerializer.Deserialize<List<SeedGameModel>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (items != null && items.Count > 0)
                {
                    var existingGames = await db.Games.ToListAsync(ct);
                    bool modified = false;

                    foreach (var m in items)
                    {
                        var match = existingGames.FirstOrDefault(g => g.BggId == m.BggId);
                        if (match != null && (!string.Equals(match.CoverImageUrl, m.CoverImageUrl, StringComparison.OrdinalIgnoreCase) ||
                                              !string.Equals(match.ThumbnailUrl, m.ThumbnailUrl, StringComparison.OrdinalIgnoreCase)))
                        {
                            match.UpdateImages(m.CoverImageUrl, m.ThumbnailUrl ?? m.CoverImageUrl);
                            modified = true;
                        }
                    }

                    if (modified)
                    {
                        await db.SaveChangesAsync(ct);
                    }
                }
            }
        }

        // Semillado de veredictos iniciales de la mesa fundadora
        await SeedFoundingVerdictsAsync(db, ct);

        // Semillado del Hub Multimedia (YouTube e Instagram)
        await SeedMediaItemsAsync(db, ct);

        // Semillado de la cola comunitaria de auto-catalogación BGG
        await SeedPendingBggImportsAsync(db, ct);

        // Semillado del Incremento 6: Sorteos, Novedades y Consultorio de Reglas Q&A
        await SeedGiveawaysAsync(db, ct);
        await SeedWeeklyReleasesAsync(db, ct);
        await SeedRuleQAAsync(db, ct);

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
                "https://www.youtube.com/watch?v=yP5J9q6P4Jg",
                "https://images.unsplash.com/photo-1610890716171-6b1bb98ffd09?auto=format&fit=crop&w=640&q=80",
                "@devirtv",
                gameId: catan.Id,
                durationSeconds: 615,
                status: ModerationStatus.Approved
            ));

            mediaList.Add(new MediaItem(
                MediaType.Playthrough,
                MediaPlatform.YouTube,
                "Tutorial Flash - CATAN en menos de 3 minutos",
                "https://www.youtube.com/watch?v=Xh0Y-3L1pSk",
                "https://images.unsplash.com/photo-1606167668584-78701c57f13d?auto=format&fit=crop&w=640&q=80",
                "@devirtv",
                gameId: catan.Id,
                durationSeconds: 180,
                playerCountBadge: "Flash",
                status: ModerationStatus.Approved
            ));

            mediaList.Add(new MediaItem(
                MediaType.ShortReel,
                MediaPlatform.YouTube,
                "Short: Catan El Juego de Cartas y componentes",
                "https://www.youtube.com/watch?v=R94a_iH2t5Y",
                "https://images.unsplash.com/photo-1511512578047-dfb367046420?auto=format&fit=crop&w=640&q=80",
                "@zacatrustv",
                gameId: catan.Id,
                durationSeconds: 48,
                status: ModerationStatus.Approved
            ));

            mediaList.Add(new MediaItem(
                MediaType.Tutorial,
                MediaPlatform.YouTube,
                "Unboxing y componentes de Catan Plus en detalle",
                "https://www.youtube.com/watch?v=yP5J9q6P4Jg",
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
                "https://www.youtube.com/watch?v=R9jBf2xZ17Y",
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
                "https://www.youtube.com/watch?v=R9jBf2xZ17Y",
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
                "https://www.youtube.com/watch?v=kYJv8Pj32zM",
                "https://images.unsplash.com/photo-1444464666168-49d633b86797?auto=format&fit=crop&w=640&q=80",
                "@unna",
                gameId: wingspan.Id,
                durationSeconds: 985,
                status: ModerationStatus.Approved
            ));

            mediaList.Add(new MediaItem(
                MediaType.Playthrough,
                MediaPlatform.YouTube,
                "Cómo jugar a Wingspan: Tutorial Corto y Preciso",
                "https://www.youtube.com/watch?v=bO4pXpX0z8I",
                "https://images.unsplash.com/photo-1552728089-57bdde30beb3?auto=format&fit=crop&w=640&q=80",
                "@unna",
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
                "@ludeka_app",
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
                "Cómo jugar a AZUL rápido y sin leer las instrucciones",
                "https://www.youtube.com/watch?v=A3lC3-5yLhU",
                "https://images.unsplash.com/photo-1513519245088-0e12902e5a38?auto=format&fit=crop&w=640&q=80",
                "@torretoken",
                gameId: azul.Id,
                durationSeconds: 410,
                status: ModerationStatus.Approved
            ));

            mediaList.Add(new MediaItem(
                MediaType.Playthrough,
                MediaPlatform.YouTube,
                "¿Cómo se juega al AZUL? Reseña y tutorial",
                "https://www.youtube.com/watch?v=5Vz1yF9-8dM",
                "https://images.unsplash.com/photo-1528459801416-a9e53bbf4e17?auto=format&fit=crop&w=640&q=80",
                "@kinuma",
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
                "Ark Nova: Tutorial paso a paso de cómo se juega",
                "https://www.youtube.com/watch?v=bDLFTY_PuAI",
                "https://images.unsplash.com/photo-1534567153574-2b12153a87f0?auto=format&fit=crop&w=640&q=80",
                "@proyectoglirp",
                gameId: arknova.Id,
                durationSeconds: 1980,
                status: ModerationStatus.Approved
            ));

            mediaList.Add(new MediaItem(
                MediaType.Playthrough,
                MediaPlatform.YouTube,
                "Ark Nova: Análisis y Top 10 BGG",
                "https://www.youtube.com/watch?v=5rG4L7z_02A",
                "https://images.unsplash.com/photo-1504173010664-32509aeebb62?auto=format&fit=crop&w=640&q=80",
                "@analisisparalisis",
                gameId: arknova.Id,
                durationSeconds: 6850,
                playerCountBadge: "Análisis",
                status: ModerationStatus.Approved
            ));

            mediaList.Add(new MediaItem(
                MediaType.Playthrough,
                MediaPlatform.YouTube,
                "Mi primera partida en club a Ark Nova a 3 jugadores",
                "https://www.youtube.com/watch?v=vV1yXn76a4s",
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
            "Top 10 novedades presentadas en feria de juegos",
            "https://www.youtube.com/watch?v=5rG4L7z_02A",
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
            "https://www.youtube.com/watch?v=Xh0Y-3L1pSk",
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

    private static async Task SeedPendingBggImportsAsync(LudekaDbContext db, CancellationToken ct)
    {
        if (await db.PendingBggImports.AnyAsync(ct)) return;

        var pendingList = new List<PendingBggImport>
        {
            new(342942, "Ark Nova", 2021,
                "https://cf.geekdo-images.com/SoU8CSclVF58FGnr7rKn8g__thumb/img/w37H0Z5q8Y7j6vO4K44u9qKk1Z4=/fit-in/200x150/filters:strip_icc()/pic6293412.jpg",
                "https://cf.geekdo-images.com/SoU8CSclVF58FGnr7rKn8g__original/img/DR-oAhmplpM1t_2Fz-l2W1Z4d_Q=/0x0/filters:format(jpeg)/pic6293412.jpg")
            {
            },
            new(174430, "Gloomhaven", 2017,
                "https://cf.geekdo-images.com/sZYp_3BTDGjh2XdDANbvAw__thumb/img/N1tQ1y7xY4x0_Y1u6d9e0i4v_8=/fit-in/200x150/filters:strip_icc()/pic2437871.jpg",
                "https://cf.geekdo-images.com/sZYp_3BTDGjh2XdDANbvAw__original/img/N1tQ1y7xY4x0_Y1u6d9e0i4v_8=/0x0/filters:format(jpeg)/pic2437871.jpg"),
            new(162886, "Spirit Island", 2017,
                "https://cf.geekdo-images.com/kjCm4mfPZwgGQWSfioFnhg__thumb/img/1m1l8z7x_1tQ0Y5k1v_8kL8w0p8=/fit-in/200x150/filters:strip_icc()/pic3114971.jpg"),
            new(366013, "Heat: Pedal to the Metal", 2022,
                "https://cf.geekdo-images.com/0i_z2v-vj28V6Qv4T1w6_w__thumb/img/0s1k2l3v_4x0_Y1u6d9e0i4v_8=/fit-in/200x150/filters:strip_icc()/pic6900143.jpg"),
            new(365717, "Clank! Catacombs", 2022,
                "https://cf.geekdo-images.com/z4_z2v-vj28V6Qv4T1w6_w__thumb/img/0s1k2l3v_4x0_Y1u6d9e0i4v_8=/fit-in/200x150/filters:strip_icc()/pic6920143.jpg")
        };

        // Simular distintas demandas comunitarias
        for (int i = 0; i < 14; i++) pendingList[0].IncrementRequestCount(); // Ark Nova: 15 solicitudes
        for (int i = 0; i < 11; i++) pendingList[1].IncrementRequestCount(); // Gloomhaven: 12 solicitudes
        for (int i = 0; i < 8; i++) pendingList[2].IncrementRequestCount();  // Spirit Island: 9 solicitudes
        for (int i = 0; i < 6; i++) pendingList[3].IncrementRequestCount();  // Heat: 7 solicitudes
        for (int i = 0; i < 4; i++) pendingList[4].IncrementRequestCount();  // Clank! Catacombs: 5 solicitudes

        await db.PendingBggImports.AddRangeAsync(pendingList, ct);
        await db.SaveChangesAsync(ct);
    }

    private static async Task SeedGiveawaysAsync(LudekaDbContext db, CancellationToken ct)
    {
        if (await db.Giveaways.AnyAsync(ct))
            return;

        var games = await db.Games.ToListAsync(ct);
        var brass = games.FirstOrDefault(g => g.BggId == 224517);
        var terraforming = games.FirstOrDefault(g => g.BggId == 167791);
        var wingspan = games.FirstOrDefault(g => g.BggId == 266192);

        var giveaways = new List<Giveaway>
        {
            new(
                title: "Gran Sorteo Brass: Birmingham Edición Deluxe + Monedas",
                organizer: "Maldito Games",
                url: "https://www.instagram.com/p/maldito-brass-sorteo",
                platform: GiveawayPlatform.Instagram,
                deadlineAt: DateTimeOffset.UtcNow.AddDays(3),
                gameId: brass?.Id,
                gameTitle: "Brass: Birmingham",
                collaborator: "Análisis Parálisis",
                thumbnailUrl: brass?.CoverImageUrl ?? "https://cf.geekdo-images.com/x3zxjr7VhC60Ue0G0pfQnA__original/img/og98Nn6kd_e_vUf-30G1nO0b2_g=/0x0/filters:format(jpeg)/pic3490053.jpg",
                isCommunityExclusive: false),

            new(
                title: "Sorteo Novedades Devir: Dwellings of Eldervale",
                organizer: "Devir Iberia",
                url: "https://www.instagram.com/p/devir-eldervale",
                platform: GiveawayPlatform.Instagram,
                deadlineAt: DateTimeOffset.UtcNow.AddDays(5),
                gameId: null,
                gameTitle: "Dwellings of Eldervale",
                collaborator: "El Rincón Legacy",
                thumbnailUrl: "https://cf.geekdo-images.com/3N8p29x1fQnA__thumb/img/pic4801123.jpg",
                isCommunityExclusive: false),

            new(
                title: "Pack de Verano Zacatrus: Wingspan + Expansión Oceanía",
                organizer: "Zacatrus",
                url: "https://x.com/zacatrus/status/wingspan-sorteo",
                platform: GiveawayPlatform.TwitterX,
                deadlineAt: DateTimeOffset.UtcNow.AddHours(18),
                gameId: wingspan?.Id,
                gameTitle: "Wingspan",
                collaborator: null,
                thumbnailUrl: wingspan?.CoverImageUrl,
                isCommunityExclusive: false),

            new(
                title: "Sorteo Mensual Ludeka: Ark Nova + Mapa de Acrílico",
                organizer: "Comunidad Ludeka",
                url: "https://ludeka.app/sorteos",
                platform: GiveawayPlatform.Community,
                deadlineAt: DateTimeOffset.UtcNow.AddDays(12),
                gameId: null,
                gameTitle: "Ark Nova",
                collaborator: null,
                thumbnailUrl: "/images/games/ark-nova.jpg",
                isCommunityExclusive: true)
        };

        await db.Giveaways.AddRangeAsync(giveaways, ct);
        await db.SaveChangesAsync(ct);
    }

    private static async Task SeedWeeklyReleasesAsync(LudekaDbContext db, CancellationToken ct)
    {
        if (await db.WeeklyReleases.AnyAsync(ct))
            return;

        // Calcular el viernes de la semana actual
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        int daysUntilFriday = ((int)DayOfWeek.Friday - (int)today.DayOfWeek + 7) % 7;
        var thisFriday = today.AddDays(daysUntilFriday);
        var nextFriday = thisFriday.AddDays(7);

        var releases = new List<WeeklyRelease>
        {
            new(
                title: "Slay the Spire: El Juego de Mesa",
                publisher: "MasQueOca",
                releaseDate: thisFriday,
                gameId: null,
                coverImageUrl: "https://cf.geekdo-images.com/pic7123901.jpg",
                estimatedPvp: 110.00m,
                isReprint: false,
                notes: "Adaptación oficial en tablero del aclamado roguelike de construcción de mazos."),

            new(
                title: "Harmonies",
                publisher: "Asmodee / Libellud",
                releaseDate: thisFriday,
                gameId: null,
                coverImageUrl: "https://cf.geekdo-images.com/pic7981245.jpg",
                estimatedPvp: 34.99m,
                isReprint: false,
                notes: "Juego de colocación de patrones en 3D y hábitats para fauna salvaje."),

            new(
                title: "Dune: Imperium - Uprising",
                publisher: "Asmodee / Dire Wolf",
                releaseDate: thisFriday,
                gameId: null,
                coverImageUrl: "https://cf.geekdo-images.com/pic7589123.jpg",
                estimatedPvp: 59.99m,
                isReprint: true,
                notes: "Reimpresión esperada con compatibilidad total con expansiones del juego base."),

            new(
                title: "Las Ruinas Perdidas de Arnak: Líderes de la Expedición",
                publisher: "Devir Iberia",
                releaseDate: nextFriday,
                gameId: null,
                coverImageUrl: "https://cf.geekdo-images.com/pic6349120.jpg",
                estimatedPvp: 29.95m,
                isReprint: true,
                notes: "Reimpresión de la expansión con 6 líderes con habilidades asimétricas únicas.")
        };

        await db.WeeklyReleases.AddRangeAsync(releases, ct);
        await db.SaveChangesAsync(ct);
    }

    private static async Task SeedRuleQAAsync(LudekaDbContext db, CancellationToken ct)
    {
        if (await db.RuleQuestions.AnyAsync(ct))
            return;

        var games = await db.Games.ToListAsync(ct);
        var brass = games.FirstOrDefault(g => g.BggId == 224517);
        var terraforming = games.FirstOrDefault(g => g.BggId == 167791);

        if (brass != null)
        {
            var q1 = new RuleQuestion(
                gameId: brass.Id,
                userId: "carlos-jugon",
                userName: "Carlos",
                title: "¿Puedo consumir carbón de una mina que no sea de mi propiedad?",
                body: "Durante la era de los canales, quería construir una fábrica textil y necesitaba carbón. Había una mina de otro jugador conectada por canal. ¿Puedo consumir su carbón gratis?");

            q1.Upvote();
            q1.Upvote();

            var a1 = new RuleAnswer(
                questionId: q1.Id,
                userId: "elena-rules",
                userName: "Elena M.",
                body: "¡Sí, totalmente! El carbón en Brass se consume de la fuente conectada más cercana, sin importar de quién sea la mina. De hecho, al consumir su último carbón, ¡darás la vuelta a su loseta dándole puntos y dinero a ese jugador!",
                officialRuleReference: "Reglamento oficial de Brass: Birmingham, pág. 11, sección 'Fuentes de Carbón'");

            a1.Upvote();
            a1.Upvote();
            a1.Upvote();

            q1.AddAnswer(a1);
            q1.MarkAcceptedAnswer(a1.Id, "carlos-jugon", isModerator: false);

            await db.RuleQuestions.AddAsync(q1, ct);
            await db.RuleAnswers.AddAsync(a1, ct);
        }

        if (terraforming != null)
        {
            var q2 = new RuleQuestion(
                gameId: terraforming.Id,
                userId: "marta-marte",
                userName: "Marta",
                title: "¿El hito de Jardinero cuenta bosques colocados por eventos?",
                body: "Un jugador colocó un bosque mediante una carta de evento roja y quería reclamar el hito de Jardinero (3 bosques). ¿Cuenta para el hito?");

            q2.Upvote();

            var a2 = new RuleAnswer(
                questionId: q2.Id,
                userId: "pablo-vet",
                userName: "Pablo Vet",
                body: "Sí. Para el hito de Jardinero cuentan las losetas físicas de bosque que tengas en el tablero de Marte bajo tu ficha de jugador, independientemente de si las colocaste por proyecto estándar o por cartas de evento.",
                officialRuleReference: "Reglamento Terraforming Mars, pág. 13");

            a2.Upvote();

            q2.AddAnswer(a2);
            q2.MarkAcceptedAnswer(a2.Id, "marta-marte", isModerator: false);

            await db.RuleQuestions.AddAsync(q2, ct);
            await db.RuleAnswers.AddAsync(a2, ct);
        }

        await db.SaveChangesAsync(ct);
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
