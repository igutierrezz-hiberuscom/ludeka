using System;

namespace Ludeka.Core.ValueObjects;

/// <summary>
/// Representa la síntesis editorial objetiva estructurada generada mediante IA o motor heurístico de respaldo.
/// </summary>
public record AiGameSummary(
    string GeneralVerdict,
    string ScalabilitySummary,
    string AgeSummary,
    string FootprintSummary,
    string Model,
    DateTime GeneratedAt
);
