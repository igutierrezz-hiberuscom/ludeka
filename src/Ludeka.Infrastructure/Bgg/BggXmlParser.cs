using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Ludeka.Core.Entities;
using Ludeka.Core.Enums;
using Ludeka.Core.ValueObjects;

namespace Ludeka.Infrastructure.Bgg;

public static class BggXmlParser
{
    public static Game? ParseItem(XElement item)
    {
        if (item == null) return null;

        int bggId = int.Parse(item.Attribute("id")?.Value ?? "0");
        if (bggId <= 0) return null;

        // Títulos
        var names = item.Elements("name").ToList();
        string originalTitle = names.FirstOrDefault(n => n.Attribute("type")?.Value == "primary")?.Attribute("value")?.Value
            ?? names.FirstOrDefault()?.Attribute("value")?.Value ?? "Desconocido";

        string spanishTitle = ExtractSpanishTitle(names, originalTitle);

        // Metadatos básicos
        int yearPublished = int.TryParse(item.Element("yearpublished")?.Attribute("value")?.Value, out int yr) ? yr : DateTime.UtcNow.Year;
        string? coverImageUrl = item.Element("image")?.Value?.Trim();
        string? thumbnailUrl = item.Element("thumbnail")?.Value?.Trim();
        string? rawDescription = item.Element("description")?.Value;
        string? description = string.IsNullOrWhiteSpace(rawDescription) ? null : WebUtility.HtmlDecode(rawDescription).Trim();

        // Autores y Editorial
        string designer = item.Elements("link")
            .FirstOrDefault(l => l.Attribute("type")?.Value == "boardgamedesigner")
            ?.Attribute("value")?.Value ?? "Varios";

        string publisher = item.Elements("link")
            .FirstOrDefault(l => l.Attribute("type")?.Value == "boardgamepublisher")
            ?.Attribute("value")?.Value ?? "Varios";

        // Tiempos
        int minTime = int.TryParse(item.Element("minplaytime")?.Attribute("value")?.Value, out int mt) && mt > 0 ? mt : 30;
        int maxTime = int.TryParse(item.Element("maxplaytime")?.Attribute("value")?.Value, out int xt) && xt > 0 ? xt : minTime;
        int estPerPlayer = Math.Max(15, (minTime + maxTime) / 4);

        // Edad de caja
        int boxAge = int.TryParse(item.Element("minage")?.Attribute("value")?.Value, out int ma) && ma > 0 ? ma : 10;

        // Encuesta de edad comunitaria
        int communityAge = ParseCommunityAge(item, boxAge);

        // Encuesta de dependencia del idioma
        var language = ParseLanguageDependence(item);

        // Ratings y Rankings
        var (bggRating, bggRank) = ParseStatistics(item);

        // Encuesta de escalabilidad
        var scalability = ParseScalability(item);

        // ADN lúdico heurístico según enlaces y categorías
        var (confrontation, style, isSolo) = InferGameDna(item, scalability);

        return new Game(
            bggId: bggId,
            originalTitle: originalTitle,
            spanishTitle: spanishTitle,
            designer: designer,
            publisher: publisher,
            yearPublished: yearPublished,
            coverImageUrl: coverImageUrl,
            thumbnailUrl: thumbnailUrl,
            description: description,
            bggRating: bggRating,
            bggRank: bggRank,
            ludistRating: bggRating,
            confrontation: confrontation,
            style: style,
            isOfficialSolo: isSolo,
            age: new AgeRating(boxAge, communityAge),
            language: language,
            footprint: TableFootprint.StandardTable,
            duration: new GameDuration(minTime, maxTime, estPerPlayer),
            scalability: scalability
        );
    }

    private static string ExtractSpanishTitle(List<XElement> names, string fallback)
    {
        foreach (var name in names.Where(n => n.Attribute("type")?.Value == "alternate"))
        {
            string val = name.Attribute("value")?.Value ?? string.Empty;
            if (val.Contains("español", StringComparison.OrdinalIgnoreCase) ||
                val.Contains("spanish", StringComparison.OrdinalIgnoreCase) ||
                val.Contains("castellano", StringComparison.OrdinalIgnoreCase))
            {
                // Limpiar sufijos como "(Edición en español)", "(Spanish edition)"
                string cleaned = Regex.Replace(val, @"\s*\([^)]*(español|spanish|castellano)[^)]*\)", "", RegexOptions.IgnoreCase).Trim();
                if (!string.IsNullOrWhiteSpace(cleaned))
                {
                    return cleaned;
                }
            }
        }
        return fallback;
    }

    private static int ParseCommunityAge(XElement item, int defaultAge)
    {
        var agePoll = item.Elements("poll").FirstOrDefault(p => p.Attribute("name")?.Value == "suggested_playerage");
        if (agePoll == null) return defaultAge;

        int highestVotes = -1;
        int bestAge = defaultAge;

        foreach (var r in agePoll.Descendants("result"))
        {
            if (int.TryParse(r.Attribute("numvotes")?.Value, out int votes) &&
                int.TryParse(r.Attribute("value")?.Value, out int age))
            {
                if (votes > highestVotes && votes > 0)
                {
                    highestVotes = votes;
                    bestAge = age;
                }
            }
        }

        return bestAge;
    }

    private static LanguageDependence ParseLanguageDependence(XElement item)
    {
        var poll = item.Elements("poll").FirstOrDefault(p => p.Attribute("name")?.Value == "language_dependence");
        if (poll == null) return LanguageDependence.None;

        int highestVotes = -1;
        int bestLevel = 1;

        foreach (var r in poll.Descendants("result"))
        {
            if (int.TryParse(r.Attribute("numvotes")?.Value, out int votes) &&
                int.TryParse(r.Attribute("level")?.Value, out int level))
            {
                if (votes > highestVotes && votes > 0)
                {
                    highestVotes = votes;
                    bestLevel = level;
                }
            }
        }

        return bestLevel switch
        {
            1 => LanguageDependence.None,
            2 => LanguageDependence.Low,
            _ => LanguageDependence.High
        };
    }

    private static (double Rating, int? Rank) ParseStatistics(XElement item)
    {
        var stats = item.Element("statistics")?.Element("ratings");
        if (stats == null) return (0.0, null);

        double rating = 0.0;
        if (double.TryParse(stats.Element("average")?.Attribute("value")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out double avg))
        {
            rating = Math.Round(avg, 2);
        }

        int? rank = null;
        var rankElem = stats.Element("ranks")?.Elements("rank")
            .FirstOrDefault(r => r.Attribute("name")?.Value == "boardgame");

        if (rankElem != null && int.TryParse(rankElem.Attribute("value")?.Value, out int rk))
        {
            rank = rk;
        }

        return (rating, rank);
    }

    private static List<ScalabilityEntry> ParseScalability(XElement item)
    {
        var entries = new List<ScalabilityEntry>();
        var poll = item.Elements("poll").FirstOrDefault(p => p.Attribute("name")?.Value == "suggested_numplayers");
        if (poll == null) return entries;

        foreach (var results in poll.Elements("results"))
        {
            string numPlayersRaw = results.Attribute("numplayers")?.Value ?? string.Empty;
            if (string.IsNullOrWhiteSpace(numPlayersRaw)) continue;

            bool isPlus = numPlayersRaw.EndsWith("+");
            string cleanNum = numPlayersRaw.TrimEnd('+');
            if (!int.TryParse(cleanNum, out int playerCount)) continue;

            int best = 0;
            int recommended = 0;
            int notRec = 0;

            foreach (var res in results.Elements("result"))
            {
                string val = res.Attribute("value")?.Value ?? string.Empty;
                int.TryParse(res.Attribute("numvotes")?.Value, out int votes);

                if (val.Equals("Best", StringComparison.OrdinalIgnoreCase)) best = votes;
                else if (val.Equals("Recommended", StringComparison.OrdinalIgnoreCase)) recommended = votes;
                else if (val.Equals("Not Recommended", StringComparison.OrdinalIgnoreCase)) notRec = votes;
            }

            var status = ScalabilityCalculator.DetermineStatus(best, recommended, notRec);
            string display = isPlus ? $"{playerCount}J+" : $"{playerCount}J";

            entries.Add(new ScalabilityEntry(playerCount, display, status, best, recommended, notRec));
        }

        // Deduplicar por PlayerCount si BGG devolviese múltiples
        return entries
            .GroupBy(e => e.PlayerCount)
            .Select(g => g.OrderByDescending(e => e.TotalVotes).First())
            .OrderBy(e => e.PlayerCount)
            .ToList();
    }

    private static (ConfrontationType Confrontation, GameStyle Style, bool IsSolo) InferGameDna(
        XElement item, List<ScalabilityEntry> scalability)
    {
        var categories = item.Elements("link")
            .Where(l => l.Attribute("type")?.Value is "boardgamecategory" or "boardgamemechanic")
            .Select(l => l.Attribute("value")?.Value ?? string.Empty)
            .ToList();

        bool isCoop = categories.Any(c => c.Contains("Cooperative", StringComparison.OrdinalIgnoreCase));
        bool isTeams = categories.Any(c => c.Contains("Team-Based", StringComparison.OrdinalIgnoreCase) || c.Contains("Secret Identity", StringComparison.OrdinalIgnoreCase));
        bool isSemiCoop = categories.Any(c => c.Contains("Semi-Cooperative", StringComparison.OrdinalIgnoreCase));

        var confrontation = isCoop ? ConfrontationType.Cooperative
            : isSemiCoop ? ConfrontationType.SemiCooperative
            : isTeams ? ConfrontationType.HiddenRolesOrTeams
            : ConfrontationType.Competitive;

        bool isParty = categories.Any(c => c.Contains("Party Game", StringComparison.OrdinalIgnoreCase));
        bool isAbstract = categories.Any(c => c.Contains("Abstract Strategy", StringComparison.OrdinalIgnoreCase));
        bool isCampaign = categories.Any(c => c.Contains("Campaign", StringComparison.OrdinalIgnoreCase) || c.Contains("Legacy", StringComparison.OrdinalIgnoreCase));
        bool isThematic = categories.Any(c => c.Contains("Thematic", StringComparison.OrdinalIgnoreCase) || c.Contains("Wargame", StringComparison.OrdinalIgnoreCase));

        var style = isParty ? GameStyle.PartyGame
            : isCampaign ? GameStyle.NarrativeCampaign
            : isThematic ? GameStyle.Ameritrash
            : isAbstract ? GameStyle.FillerAbstract
            : GameStyle.Eurogame;

        bool isSolo = scalability.Any(s => s.PlayerCount == 1 && s.Status != ScalabilityStatus.NotRecommended);

        return (confrontation, style, isSolo);
    }
}
