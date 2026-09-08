using System;
using System.Text.RegularExpressions;
using Ludeka.Core.Enums;

namespace Ludeka.Core.Helpers;

/// <summary>
/// Clasificador heurístico inteligente para inferir la categoría taxonómica de contenidos audiovisuales.
/// Evalúa patrones semánticos en títulos y descripciones en español e inglés.
/// </summary>
public static class MediaClassifier
{
    private static readonly Regex QuickOverviewRegex = new(
        @"(?i)\b(c[oó]mo\s+funciona|vistazo\s+r[aá]pido|en\s+(?:2|3|dos|tres)\s+minutos?|overview|quick\s+look|quick\s+overview|resumen\s+de\s+mec[aá]nicas|mec[aá]nicas\s+en)\b",
        RegexOptions.Compiled);

    private static readonly Regex GameplayRegex = new(
        @"(?i)\b(partida|gameplay|jugando(?:\s+a|\s+con)?|playthrough|let'?s\s+play|duelo\s+a\s+2|en\s+solitario)\b",
        RegexOptions.Compiled);

    private static readonly Regex ReviewOpinionRegex = new(
        @"(?i)\b(rese[ñn]a|opini[oó]n|an[aá]lisis|primeras\s+impresiones|vale\s+la\s+pena|merece\s+la\s+pena|veredicto|review|cr[ií]tica|unboxing|abriendo(?:\s+la\s+caja)?|desempaquetado)\b",
        RegexOptions.Compiled);

    private static readonly Regex TutorialRegex = new(
        @"(?i)\b(c[oó]mo\s+jugar|tutorial|aprende\s+a\s+jugar|reglas|explicaci[oó]n|gu[ií]a\s+paso\s+a\s+paso|how\s+to\s+play|rules)\b",
        RegexOptions.Compiled);

    /// <summary>
    /// Deduce la categoría editorial sugerida a partir del título y descripción del vídeo.
    /// </summary>
    public static MediaCategory Classify(string title, string? description = null)
    {
        var textToEvaluate = $"{title} {description ?? string.Empty}".Trim();
        if (string.IsNullOrWhiteSpace(textToEvaluate))
        {
            return MediaCategory.Tutorial;
        }

        // 1. Cómo Funciona (Vistazo de mecánicas en 2-3 min) - Precedencia antes de tutorial para capturar "cómo funciona"
        if (QuickOverviewRegex.IsMatch(textToEvaluate))
        {
            return MediaCategory.QuickOverview;
        }

        // 2. Partidas completas / Gameplay
        if (GameplayRegex.IsMatch(textToEvaluate))
        {
            return MediaCategory.Gameplay;
        }

        // 3. Reseñas, análisis, opiniones y unboxings
        if (ReviewOpinionRegex.IsMatch(textToEvaluate))
        {
            return MediaCategory.ReviewOpinion;
        }

        // 4. Tutoriales y reglas
        if (TutorialRegex.IsMatch(textToEvaluate))
        {
            return MediaCategory.Tutorial;
        }

        // 5. Fallback predeterminado a Tutorial
        return MediaCategory.Tutorial;
    }
}
