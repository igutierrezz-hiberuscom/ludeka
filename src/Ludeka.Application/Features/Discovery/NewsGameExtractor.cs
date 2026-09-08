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
using Microsoft.Extensions.Logging;

namespace Ludeka.Application.Features.Discovery;

/// <summary>
/// Extractor heurístico y semántico que identifica juegos de mesa dentro de
/// publicaciones editoriales y novedades, vinculándolos al catálogo o encolándolos en BGG.
/// </summary>
public class NewsGameExtractor : INewsGameExtractor
{
    private readonly IGameRepository _gameRepo;
    private readonly IBggClient _bggClient;
    private readonly IPendingBggImportRepository _pendingRepo;
    private readonly IWeeklyReleaseRepository _releaseRepo;
    private readonly ILogger<NewsGameExtractor> _logger;

    private static readonly Regex[] TitlePatterns =
    [
        // Citas explícitas: "Apiary", «Wingspan», “Ark Nova”
        new Regex(@"[\""“«](?<title>[^\""”»]+)[\""”»]", RegexOptions.Compiled | RegexOptions.IgnoreCase),

        // Patrones verbales comunes en anuncios de editoriales en español
        new Regex(@"anuncia\s+(?:la\s+edición\s+en\s+castellano\s+de|el\s+lanzamiento\s+de|la\s+llegada\s+de|la\s+reimpresión\s+de)\s+(?<title>[A-Za-z0-9ÁÉÍÓÚáéíóúñÑüÜ\s:–\-]+?)(?:\s+para|\s+en\s+tiendas|\s*\.|\s*,|\s*$)", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        new Regex(@"anuncia\s+(?<title>[A-Za-z0-9ÁÉÍÓÚáéíóúñÑüÜ\s:–\-]+?)(?:\s+para|\s+en\s+tiendas|\s*\.|\s*,|\s*$)", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        new Regex(@"publicar[aá]\s+(?<title>[A-Za-z0-9ÁÉÍÓÚáéíóúñÑüÜ\s:–\-]+?)(?:\s+en\s+español|\s+en\s+castellano|\s+este|\s*\.|\s*,|\s*$)", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        new Regex(@"lanzamiento\s+de\s+(?<title>[A-Za-z0-9ÁÉÍÓÚáéíóúñÑüÜ\s:–\-]+?)(?:\s+en\s+tiendas|\s+por\s+parte|\s*\.|\s*,|\s*$)", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        new Regex(@"preventa\s+de\s+(?<title>[A-Za-z0-9ÁÉÍÓÚáéíóúñÑüÜ\s:–\-]+?)(?:\s+abierta|\s*\.|\s*,|\s*$)", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        new Regex(@"reimpresi[oó]n\s+de\s+(?<title>[A-Za-z0-9ÁÉÍÓÚáéíóúñÑüÜ\s:–\-]+?)(?:\s+confirmada|\s*\.|\s*,|\s*$)", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        new Regex(@"nueva\s+expansi[oó]n\s+de\s+(?<title>[A-Za-z0-9ÁÉÍÓÚáéíóúñÑüÜ\s:–\-]+?)(?:\s*\.|\s*,|\s*$)", RegexOptions.Compiled | RegexOptions.IgnoreCase)
    ];

    private static readonly Regex CleanPrefixRegex = new(
        @"^(?:[A-Za-z0-9ÁÉÍÓÚáéíóúñÑüÜ\s]+[:\-–]\s*)(?<cleanTitle>.*)$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public NewsGameExtractor(
        IGameRepository gameRepo,
        IBggClient bggClient,
        IPendingBggImportRepository pendingRepo,
        IWeeklyReleaseRepository releaseRepo,
        ILogger<NewsGameExtractor> logger)
    {
        _gameRepo = gameRepo ?? throw new ArgumentNullException(nameof(gameRepo));
        _bggClient = bggClient ?? throw new ArgumentNullException(nameof(bggClient));
        _pendingRepo = pendingRepo ?? throw new ArgumentNullException(nameof(pendingRepo));
        _releaseRepo = releaseRepo ?? throw new ArgumentNullException(nameof(releaseRepo));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public string? ExtractGameTitle(string newsTitle, string? newsNotes = null)
    {
        if (string.IsNullOrWhiteSpace(newsTitle)) return null;

        var cleanText = newsTitle.Trim();

        // 1. Probar patrones sintácticos ordenados por especificidad
        foreach (var pattern in TitlePatterns)
        {
            var match = pattern.Match(cleanText);
            if (match.Success)
            {
                var candidate = match.Groups["title"].Value.Trim();
                if (IsValidCandidate(candidate))
                {
                    return SanitizeCandidate(candidate);
                }
            }
        }

        // 2. Probar si el titular tiene prefijo "Editorial: Título"
        var prefixMatch = CleanPrefixRegex.Match(cleanText);
        if (prefixMatch.Success)
        {
            var candidate = prefixMatch.Groups["cleanTitle"].Value.Trim();
            if (IsValidCandidate(candidate))
            {
                return SanitizeCandidate(candidate);
            }
        }

        // 3. Fallback: Si el titular es corto y directo (máx 5 palabras), tomarlo directamente
        var words = cleanText.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length is >= 1 and <= 5 && IsValidCandidate(cleanText))
        {
            return SanitizeCandidate(cleanText);
        }

        return null;
    }

    public async Task<NewsExtractionResultDto> ProcessReleaseAsync(WeeklyRelease release, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(release);

        if (release.GameId.HasValue)
        {
            return new NewsExtractionResultDto(release.Id, null, true, release.GameId.Value, false, null);
        }

        var extractedTitle = ExtractGameTitle(release.Title, release.Notes);
        if (string.IsNullOrWhiteSpace(extractedTitle))
        {
            _logger.LogDebug("No se pudo extraer el título del juego para la novedad #{Id}: '{Title}'", release.Id, release.Title);
            return new NewsExtractionResultDto(release.Id, null, false, null, false, null);
        }

        // 1. Comprobar existencia en el catálogo local de Ludeka
        var searchCriteria = new GameFilterCriteria(SearchTerm: extractedTitle);
        var (localMatches, _) = await _gameRepo.SearchAsync(searchCriteria, page: 1, pageSize: 5, ct: ct);

        var existingGame = localMatches.FirstOrDefault(g =>
            string.Equals(g.SpanishTitle, extractedTitle, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(g.OriginalTitle, extractedTitle, StringComparison.OrdinalIgnoreCase));

        if (existingGame != null)
        {
            release.LinkGame(existingGame.Id);
            await _releaseRepo.UpdateAsync(release, ct);
            _logger.LogInformation("Novedad #{ReleaseId} ('{Title}') vinculada al juego existente '{GameTitle}' (#{GameId}).",
                release.Id, release.Title, existingGame.SpanishTitle, existingGame.Id);

            return new NewsExtractionResultDto(release.Id, extractedTitle, true, existingGame.Id, false, null);
        }

        // 2. Si no existe en Ludeka, verificar en BoardGameGeek (BGG Search API)
        try
        {
            var bggResults = await _bggClient.SearchGamesAsync(extractedTitle, ct);
            if (bggResults.Count > 0)
            {
                // Preferir coincidencia exacta de título o el primer resultado más relevante
                var bestMatch = bggResults.FirstOrDefault(b =>
                    string.Equals(b.Title, extractedTitle, StringComparison.OrdinalIgnoreCase)) ?? bggResults[0];

                var existingQueueItem = await _pendingRepo.GetByBggIdAsync(bestMatch.BggId, ct);
                if (existingQueueItem == null)
                {
                    var queueItem = new PendingBggImport(
                        bggId: bestMatch.BggId,
                        title: bestMatch.Title,
                        yearPublished: bestMatch.YearPublished,
                        thumbnailUrl: null,
                        coverImageUrl: null,
                        origin: CatalogQueueOrigin.NewsDiscovery,
                        extractedTitle: extractedTitle
                    );

                    await _pendingRepo.AddAsync(queueItem, ct);
                    _logger.LogInformation("Juego '{Title}' (BggId {BggId}) encolado con éxito en la cola de auto-catalogación desde novedad #{ReleaseId}.",
                        bestMatch.Title, bestMatch.BggId, release.Id);
                }
                else
                {
                    existingQueueItem.IncrementRequestCount();
                    if (existingQueueItem.Status == CatalogQueueStatus.Failed)
                    {
                        existingQueueItem.ResetToPending();
                    }
                    await _pendingRepo.UpdateAsync(existingQueueItem, ct);
                }

                return new NewsExtractionResultDto(release.Id, extractedTitle, false, null, true, bestMatch.BggId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al consultar BGG Search para el título extraído '{ExtractedTitle}' de la novedad #{ReleaseId}.",
                extractedTitle, release.Id);
        }

        return new NewsExtractionResultDto(release.Id, extractedTitle, false, null, false, null);
    }

    public async Task<int> DiscoverAndEnqueueFromReleasesAsync(CancellationToken ct = default)
    {
        var allReleases = await _releaseRepo.GetReleasesAsync(fromDate: null, ct: ct);
        var unlinkedReleases = allReleases.Where(r => !r.GameId.HasValue).ToList();

        int processed = 0;
        foreach (var release in unlinkedReleases)
        {
            var result = await ProcessReleaseAsync(release, ct);
            if (result.LinkedToExistingGame || result.EnqueuedToBgg)
            {
                processed++;
            }
        }

        return processed;
    }

    private static bool IsValidCandidate(string candidate)
    {
        if (string.IsNullOrWhiteSpace(candidate)) return false;
        var trimmed = candidate.Trim();
        if (trimmed.Length < 2) return false;

        // Descartar falsos positivos de palabras vacías
        var stopWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "lanzamiento", "novedad", "novedades", "juego", "juegos", "preventa", "reimpresión"
        };

        return !stopWords.Contains(trimmed);
    }

    private static string SanitizeCandidate(string candidate)
    {
        var sanitized = candidate.Trim().TrimEnd('.', ',', ';', ':', '!', '?');
        return sanitized;
    }
}
