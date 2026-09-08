using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Ludeka.Application.Contracts;
using Ludeka.Application.DTOs;
using Ludeka.Core.Entities;
using Ludeka.Core.Enums;
using Ludeka.Core.ValueObjects;

namespace Ludeka.Application.Features.Library;

public class UserLibraryStatsService : IUserLibraryStatsService
{
    private readonly IUserCollectionRepository _collectionRepo;
    private readonly IGameLoanRepository _loanRepo;
    private readonly IGameRepository _gameRepo;
    private readonly ICurrentUserService _currentUserService;

    public UserLibraryStatsService(
        IUserCollectionRepository collectionRepo,
        IGameLoanRepository loanRepo,
        IGameRepository gameRepo,
        ICurrentUserService currentUserService)
    {
        _collectionRepo = collectionRepo;
        _loanRepo = loanRepo;
        _gameRepo = gameRepo;
        _currentUserService = currentUserService;
    }

    public async Task<UserLibraryStatsDto> GetUserStatsAsync(string? userId = null, CancellationToken ct = default)
    {
        string resolvedUserId = string.IsNullOrWhiteSpace(userId) ? _currentUserService.UserId : userId.Trim();
        string resolvedUserName = resolvedUserId.Equals(_currentUserService.UserId, StringComparison.OrdinalIgnoreCase)
            ? _currentUserService.UserName
            : resolvedUserId;

        var allItems = await _collectionRepo.GetByUserIdAsync(resolvedUserId, null, ct);
        var counts = await _collectionRepo.GetCountsByStatusAsync(resolvedUserId, ct);
        int activeLoansCount = await _loanRepo.GetActiveLoansCountAsync(resolvedUserId, ct);

        int totalInCollection = counts.GetValueOrDefault(CollectionStatus.InCollection, 0);
        int totalPlayed = counts.GetValueOrDefault(CollectionStatus.Played, 0);
        int totalWishlist = counts.GetValueOrDefault(CollectionStatus.Wishlist, 0);
        int totalWantToBuy = counts.GetValueOrDefault(CollectionStatus.WantToBuy, 0);

        // Extraer títulos físicos en estantería
        var inCollectionItems = allItems.Where(i => i.Status == CollectionStatus.InCollection).ToList();
        var shelfGames = new List<Game>();

        foreach (var item in inCollectionItems)
        {
            var game = item.Game ?? (item.GameId.HasValue ? await _gameRepo.GetByIdAsync(item.GameId.Value, ct) : null);
            if (game != null)
            {
                shelfGames.Add(game);
            }
        }

        // 1. Horas en estantería
        var shelfTime = ComputeShelfTime(shelfGames);

        // 2. ADN Lúdico
        var dnaDistribution = ComputeDnaDistribution(shelfGames);

        // 3. Escalabilidad y Sweet Spot
        var scalability = ComputeScalabilitySweetSpot(shelfGames);

        // 4. Top Diseñadores y Editoriales
        var topDesigners = ComputeTopEntities(shelfGames.Select(g => g.Designer), shelfGames.Count, isDesigner: true);
        var topPublishers = ComputeTopEntities(shelfGames.Select(g => g.Publisher), shelfGames.Count, isDesigner: false);

        // 5. Radar de Fundas y Protección
        var sleevesRadar = ComputeSleevesRadar(shelfGames);

        // 6. Insignias y Rasgos
        var badge = ComputePlayerBadge(shelfGames.Count, dnaDistribution);

        return new UserLibraryStatsDto(
            resolvedUserId,
            resolvedUserName,
            totalInCollection,
            totalPlayed,
            totalWishlist,
            totalWantToBuy,
            activeLoansCount,
            shelfTime,
            dnaDistribution,
            scalability,
            topDesigners,
            topPublishers,
            sleevesRadar,
            badge
        );
    }

    public async Task<PublicUserProfileDto?> GetPublicProfileAsync(string userId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(userId)) return null;

        string normalizedUserId = userId.Trim();
        var userItems = await _collectionRepo.GetByUserIdAsync(normalizedUserId, null, ct);
        var counts = await _collectionRepo.GetCountsByStatusAsync(normalizedUserId, ct);

        // Si no existen elementos de colección ni registros para ese usuario, se considera no encontrado
        int totalItemsCount = counts.Values.Sum();
        if (totalItemsCount == 0 && userItems.Count == 0)
        {
            return null;
        }

        var stats = await GetUserStatsAsync(normalizedUserId, ct);

        var inCollectionItems = userItems.Where(i => i.Status == CollectionStatus.InCollection).ToList();
        var shelfDtos = new List<UserCollectionItemDto>();

        foreach (var item in inCollectionItems)
        {
            var game = item.Game ?? (item.GameId.HasValue ? await _gameRepo.GetByIdAsync(item.GameId.Value, ct) : null);
            var activeLoan = item.GameId.HasValue ? await _loanRepo.GetActiveLoanByUserAndGameAsync(normalizedUserId, item.GameId.Value, ct) : null;

            shelfDtos.Add(new UserCollectionItemDto(
                item.Id,
                item.GameId,
                game?.SpanishTitle ?? item.PendingTitle ?? "Juego",
                game?.CoverImageUrl ?? item.PendingThumbnailUrl,
                game?.Slug ?? string.Empty,
                item.Status,
                item.AddedAt,
                activeLoan != null,
                item.BggId ?? game?.BggId,
                item.IsPendingCataloging,
                game?.IsExpansion ?? false
            ));
        }

        return new PublicUserProfileDto(
            normalizedUserId,
            stats.UserName,
            stats,
            shelfDtos.OrderBy(g => g.GameTitle, StringComparer.CurrentCultureIgnoreCase).ToList()
        );
    }

    private static ShelfTimeStatsDto ComputeShelfTime(List<Game> games)
    {
        if (games.Count == 0)
        {
            return new ShelfTimeStatsDto(0, 0, 0.0, 0.0, "0 horas");
        }

        int totalMinMinutes = games.Sum(g => g.Duration?.MinMinutes ?? 0);
        int totalMaxMinutes = games.Sum(g => g.Duration?.MaxMinutes ?? 0);
        double totalMinHours = Math.Round(totalMinMinutes / 60.0, 1);
        double totalMaxHours = Math.Round(totalMaxMinutes / 60.0, 1);

        string formatted;
        if (totalMinHours == totalMaxHours)
        {
            formatted = $"{totalMinHours:0.#} horas de juego acumuladas";
        }
        else
        {
            formatted = $"{totalMinHours:0.#} – {totalMaxHours:0.#} horas de juego acumuladas";
        }

        return new ShelfTimeStatsDto(
            totalMinMinutes,
            totalMaxMinutes,
            totalMinHours,
            totalMaxHours,
            formatted
        );
    }

    private static PlayerDnaDistributionDto ComputeDnaDistribution(List<Game> games)
    {
        var styleMeta = new (GameStyle Style, string Name, string Color)[]
        {
            (GameStyle.Eurogame, "Eurogames", "#3b82f6"),
            (GameStyle.Ameritrash, "Temáticos / Ameritrash", "#ef4444"),
            (GameStyle.PartyGame, "Party Games", "#f59e0b"),
            (GameStyle.FillerAbstract, "Fillers / Abstractos", "#10b981"),
            (GameStyle.NarrativeCampaign, "Campaña / Narrativos", "#8b5cf6")
        };

        int totalCount = games.Count;
        var styleDtos = new List<StylePercentageDto>();

        foreach (var (style, name, color) in styleMeta)
        {
            int count = games.Count(g => g.Style == style);
            double pct = totalCount > 0 ? Math.Round((count * 100.0) / totalCount, 1) : 0.0;
            styleDtos.Add(new StylePercentageDto(style, name, count, pct, color));
        }

        // Ordenar por número de juegos descendente
        var orderedStyles = styleDtos.OrderByDescending(s => s.GameCount).ThenBy(s => s.Style).ToList();

        string dominantStyle = "Sin definir";
        if (totalCount > 0 && orderedStyles[0].GameCount > 0)
        {
            dominantStyle = orderedStyles[0].StyleDisplayName;
        }

        int coopCount = games.Count(g => g.Confrontation is ConfrontationType.Cooperative or ConfrontationType.SemiCooperative);
        double coopPct = totalCount > 0 ? Math.Round((coopCount * 100.0) / totalCount, 1) : 0.0;

        int soloCount = games.Count(g => g.IsOfficialSolo);
        double soloPct = totalCount > 0 ? Math.Round((soloCount * 100.0) / totalCount, 1) : 0.0;

        return new PlayerDnaDistributionDto(
            orderedStyles,
            dominantStyle,
            coopPct,
            coopCount,
            soloPct,
            soloCount
        );
    }

    private static ScalabilitySweetSpotDto ComputeScalabilitySweetSpot(List<Game> games)
    {
        var curve = new List<ScalabilityCountDto>();

        for (int p = 1; p <= 7; p++)
        {
            string display = p == 7 ? "7+" : p.ToString();
            int optimizedCount;
            if (p < 7)
            {
                optimizedCount = games.Count(g => g.Scalability.Any(s => s.PlayerCount == p && s.IsRecommendedOrBest));
            }
            else
            {
                optimizedCount = games.Count(g => g.Scalability.Any(s => s.PlayerCount >= 7 && s.IsRecommendedOrBest));
            }

            curve.Add(new ScalabilityCountDto(p, display, optimizedCount, false));
        }

        int maxOptimized = curve.Max(c => c.OptimizedGamesCount);
        var sweetSpotPlayerCounts = new List<int>();

        if (maxOptimized > 0)
        {
            sweetSpotPlayerCounts = curve
                .Where(c => c.OptimizedGamesCount == maxOptimized)
                .Select(c => c.PlayerCount)
                .ToList();

            // Marcar IsSweetSpot en la curva
            curve = curve.Select(c => c with { IsSweetSpot = sweetSpotPlayerCounts.Contains(c.PlayerCount) }).ToList();
        }

        string summaryText;
        if (games.Count == 0 || maxOptimized == 0)
        {
            summaryText = "Sin datos suficientes";
        }
        else if (sweetSpotPlayerCounts.Count == 1)
        {
            string pStr = sweetSpotPlayerCounts[0] == 7 ? "7+" : sweetSpotPlayerCounts[0].ToString();
            summaryText = $"Especializada en mesas de {pStr} jugadores";
        }
        else if (sweetSpotPlayerCounts.Count == 2)
        {
            string p1 = sweetSpotPlayerCounts[0] == 7 ? "7+" : sweetSpotPlayerCounts[0].ToString();
            string p2 = sweetSpotPlayerCounts[1] == 7 ? "7+" : sweetSpotPlayerCounts[1].ToString();
            summaryText = $"Optimizada para {p1} y {p2} jugadores";
        }
        else
        {
            var initial = sweetSpotPlayerCounts.Take(sweetSpotPlayerCounts.Count - 1).Select(p => p == 7 ? "7+" : p.ToString());
            string last = sweetSpotPlayerCounts.Last() == 7 ? "7+" : sweetSpotPlayerCounts.Last().ToString();
            summaryText = $"Versátil para {string.Join(", ", initial)} y {last} jugadores";
        }

        return new ScalabilitySweetSpotDto(curve, sweetSpotPlayerCounts, summaryText);
    }

    private static List<TopEntityStatDto> ComputeTopEntities(IEnumerable<string?> rawEntities, int totalGames, bool isDesigner)
    {
        var counts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var raw in rawEntities)
        {
            if (string.IsNullOrWhiteSpace(raw)) continue;

            string[] parts = raw.Split(new[] { ',', '/', '&' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                var subparts = Regex.Split(part, @"\s+y\s+", RegexOptions.IgnoreCase);
                foreach (var item in subparts)
                {
                    string clean = item.Trim();
                    if (string.IsNullOrWhiteSpace(clean) || clean.Length < 2) continue;
                    if (clean.Equals("Unknown", StringComparison.OrdinalIgnoreCase) ||
                        clean.Equals("Desconocido", StringComparison.OrdinalIgnoreCase) ||
                        clean.Equals("N/A", StringComparison.OrdinalIgnoreCase) ||
                        clean == "-")
                    {
                        continue;
                    }

                    counts[clean] = counts.GetValueOrDefault(clean, 0) + 1;
                }
            }
        }

        return counts
            .OrderByDescending(kv => kv.Value)
            .ThenBy(kv => kv.Key, StringComparer.CurrentCultureIgnoreCase)
            .Take(5)
            .Select(kv => new TopEntityStatDto(
                kv.Key,
                kv.Value,
                totalGames > 0 ? Math.Round((kv.Value * 100.0) / totalGames, 1) : 0.0
            ))
            .ToList();
    }

    private static SleeveProtectionRadarDto ComputeSleevesRadar(List<Game> games)
    {
        int totalCards = 0;
        int totalPacks = 0;
        int gamesRequiring = 0;
        var formatMap = new Dictionary<string, (int Cards, int Packs)>(StringComparer.OrdinalIgnoreCase);

        foreach (var game in games)
        {
            var sleeves = game.Sleeves ?? [];
            if (sleeves.Any(s => s.CardCount > 0))
            {
                gamesRequiring++;
            }

            foreach (var s in sleeves)
            {
                if (s.CardCount <= 0) continue;
                int packs = s.CalculatePacksNeeded(50);
                totalCards += s.CardCount;
                totalPacks += packs;

                string format = string.IsNullOrWhiteSpace(s.FormatName) ? "Estándar" : s.FormatName.Trim();
                if (formatMap.TryGetValue(format, out var existing))
                {
                    formatMap[format] = (existing.Cards + s.CardCount, existing.Packs + packs);
                }
                else
                {
                    formatMap[format] = (s.CardCount, packs);
                }
            }
        }

        var topFormats = formatMap
            .OrderByDescending(kv => kv.Value.Cards)
            .Select(kv => new SleeveFormatStatDto(kv.Key, kv.Value.Cards, kv.Value.Packs))
            .ToList();

        return new SleeveProtectionRadarDto(
            totalCards,
            totalPacks,
            gamesRequiring,
            topFormats
        );
    }

    private static PlayerBadgeDto ComputePlayerBadge(int shelfGamesCount, PlayerDnaDistributionDto dna)
    {
        // 1. Rango de Coleccionista
        string rankName;
        int rankLevel;
        string tagline;

        if (shelfGamesCount == 0)
        {
            rankName = "Estantería en Blanco";
            rankLevel = 0;
            tagline = "Tu viaje lúdico está a punto de comenzar.";
        }
        else if (shelfGamesCount <= 4)
        {
            rankName = "Iniciado de Mesa";
            rankLevel = 1;
            tagline = "Dando los primeros pasos en el tablero.";
        }
        else if (shelfGamesCount <= 14)
        {
            rankName = "Explorador Lúdico";
            rankLevel = 2;
            tagline = "Una colección en pleno crecimiento y descubrimientos.";
        }
        else if (shelfGamesCount <= 29)
        {
            rankName = "Veterano de Mesa";
            rankLevel = 3;
            tagline = "Mesa con personalidad consolidada y grandes veladas.";
        }
        else
        {
            rankName = "Mecenas Lúdico";
            rankLevel = 4;
            tagline = "Un templo de juegos digno de mención y envidia sana.";
        }

        // 2. Rasgo Lúdico Distintivo
        double euroPct = dna.Styles.FirstOrDefault(s => s.Style == GameStyle.Eurogame)?.Percentage ?? 0.0;
        double ameritrashPct = dna.Styles.FirstOrDefault(s => s.Style == GameStyle.Ameritrash)?.Percentage ?? 0.0;
        double partyPct = dna.Styles.FirstOrDefault(s => s.Style == GameStyle.PartyGame)?.Percentage ?? 0.0;
        double fillerPct = dna.Styles.FirstOrDefault(s => s.Style == GameStyle.FillerAbstract)?.Percentage ?? 0.0;
        double coopPct = dna.CooperativePercentage;
        double soloPct = dna.SoloReadyPercentage;

        string traitName;
        string iconEmoji;
        string traitDescription;

        if (shelfGamesCount == 0)
        {
            traitName = "Paladar Ecléctico";
            iconEmoji = "🌈";
            traitDescription = "Gusto versátil y abierto a cualquier propuesta que reúna a buenos amigos alrededor de una mesa.";
        }
        else if (euroPct >= 50.0)
        {
            traitName = "Cerebro Eurogamer";
            iconEmoji = "🧠";
            traitDescription = "Amante de la optimización minuciosa, la colocación de trabajadores y los puntos de victoria bien sudados.";
        }
        else if (ameritrashPct >= 50.0)
        {
            traitName = "Héroe Temático";
            iconEmoji = "⚔️";
            traitDescription = "Vives por la inmersión, el drama del azar, las miniaturas épicas y las historias inolvidables.";
        }
        else if (partyPct >= 40.0)
        {
            traitName = "Alma de la Fiesta";
            iconEmoji = "🎉";
            traitDescription = "Tu estantería es el epicentro de la diversión grupal, risas desenfrenadas y reuniones concurridas.";
        }
        else if (coopPct >= 40.0)
        {
            traitName = "Espíritu Cooperativo";
            iconEmoji = "🤝";
            traitDescription = "Para ti la victoria solo sabe bien si se celebra en equipo contra el propio tablero.";
        }
        else if (fillerPct >= 40.0)
        {
            traitName = "Maestro del Filler";
            iconEmoji = "⚡";
            traitDescription = "Elegancia en formato compacto, partidas ágiles y decisiones afiladas sin rodeos.";
        }
        else if (soloPct >= 40.0)
        {
            traitName = "Lobo Solitario";
            iconEmoji = "🐺";
            traitDescription = "Especialista en desafíos individuales y duelos tácticos contra autómas.";
        }
        else
        {
            traitName = "Paladar Ecléctico";
            iconEmoji = "🌈";
            traitDescription = "Gusto versátil y abierto a cualquier propuesta que reúna a buenos amigos alrededor de una mesa.";
        }

        return new PlayerBadgeDto(
            rankName,
            rankLevel,
            traitName,
            traitDescription,
            iconEmoji,
            tagline
        );
    }
}
