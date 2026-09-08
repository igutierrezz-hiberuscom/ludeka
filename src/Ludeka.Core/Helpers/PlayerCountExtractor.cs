using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Ludeka.Core.Entities;
using Ludeka.Core.Enums;
using Ludeka.Core.ValueObjects;

namespace Ludeka.Core.Helpers;

/// <summary>
/// Extrae y normaliza el distintivo de número de comensales (PlayerCountBadge) para vídeos de partidas completas,
/// garantizando el cumplimiento de la invariante de dominio de MediaItem.
/// </summary>
public static class PlayerCountExtractor
{
    private static readonly Regex SolitaryRegex = new(
        @"(?i)\b(en\s+solitario|modo\s+solo|partida\s+solo|solitario|1\s*jugador\b|a\s+1\s*(?:jugador|p)?\b)",
        RegexOptions.Compiled);

    private static readonly Regex DuelRegex = new(
        @"(?i)\b(duelo|a\s+dos|a\s+2\s*(?:jugadores?|players?|comensales|p)?\b)",
        RegexOptions.Compiled);

    private static readonly Regex NumericPlayersRegex = new(
        @"(?i)\ba\s*(\d+)\s*(?:jugadores?|players?|comensales|p)?\b",
        RegexOptions.Compiled);

    private static readonly Regex PlaythroughExplicitRegex = new(
        @"(?i)\b(?:partida|gameplay|duelo)\s+a\s*(\d+)\b",
        RegexOptions.Compiled);

    private static readonly Dictionary<string, int> TextualNumbers = new(StringComparer.OrdinalIgnoreCase)
    {
        { "un", 1 },
        { "uno", 1 },
        { "dos", 2 },
        { "tres", 3 },
        { "cuatro", 4 },
        { "cinco", 5 },
        { "seis", 6 },
        { "siete", 7 },
        { "ocho", 8 }
    };

    /// <summary>
    /// Intenta extraer el badge de comensales desde el título y descripción del vídeo.
    /// Si no se detecta, aplica la mejor escalabilidad del juego de referencia.
    /// </summary>
    public static string ExtractPlayerBadge(string title, string? description = null, Game? game = null)
    {
        var textToAnalyze = $"{title} {description ?? string.Empty}".Trim();

        // 1. Detección de solitario
        if (SolitaryRegex.IsMatch(textToAnalyze))
        {
            return "Partida en solitario";
        }

        // 2. Detección explícita de "partida a X"
        var explicitMatch = PlaythroughExplicitRegex.Match(textToAnalyze);
        if (explicitMatch.Success && int.TryParse(explicitMatch.Groups[1].Value, out var explicitCount) && explicitCount > 0)
        {
            return explicitCount == 1 ? "Partida en solitario" : $"Partida a {explicitCount}";
        }

        // 3. Detección numérica general ("a 3", "a 4 jugadores")
        var match = NumericPlayersRegex.Match(textToAnalyze);
        if (match.Success && int.TryParse(match.Groups[1].Value, out var count) && count > 0)
        {
            return count == 1 ? "Partida en solitario" : $"Partida a {count}";
        }

        // 4. Detección de números textuales en español ("a dos jugadores", "a tres")
        foreach (var (word, val) in TextualNumbers)
        {
            var pattern = $@"(?i)\ba\s+{word}\s*(?:jugadores?|players?|comensales)?\b";
            if (Regex.IsMatch(textToAnalyze, pattern))
            {
                return val == 1 ? "Partida en solitario" : $"Partida a {val}";
            }
        }

        // 5. Detección de duelo
        if (DuelRegex.IsMatch(textToAnalyze))
        {
            return "Partida a 2";
        }

        // 6. Fallback a la escalabilidad del juego
        if (game != null)
        {
            var fallbackFromGame = GetFallbackFromGame(game);
            if (!string.IsNullOrWhiteSpace(fallbackFromGame))
            {
                return fallbackFromGame;
            }
        }

        // 7. Fallback seguro garantizado
        return "Partida a 2";
    }

    private static string? GetFallbackFromGame(Game game)
    {
        if (game.Scalability != null && game.Scalability.Count > 0)
        {
            var best = game.Scalability.FirstOrDefault(s => s.Status == ScalabilityStatus.MustPlay);
            if (best != null)
            {
                return best.PlayerCount == 1 ? "Partida en solitario" : $"Partida a {best.PlayerCount}";
            }

            var recommended = game.Scalability.FirstOrDefault(s => s.Status == ScalabilityStatus.Recommended);
            if (recommended != null)
            {
                return recommended.PlayerCount == 1 ? "Partida en solitario" : $"Partida a {recommended.PlayerCount}";
            }

            var first = game.Scalability.FirstOrDefault();
            if (first != null)
            {
                return first.PlayerCount == 1 ? "Partida en solitario" : $"Partida a {first.PlayerCount}";
            }
        }

        if (game.IsOfficialSolo)
        {
            return "Partida en solitario";
        }

        return null;
    }
}
