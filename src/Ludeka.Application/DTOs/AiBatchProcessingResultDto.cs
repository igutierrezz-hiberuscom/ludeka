using System.Collections.Generic;

namespace Ludeka.Application.DTOs;

/// <summary>
/// Resultado de la ejecución de una carga nocturna o procesamiento por lotes de resúmenes con IA.
/// </summary>
public record AiBatchProcessingResultDto(
    int ProcessedCount,
    int SuccessCount,
    int FailedCount,
    IReadOnlyList<string> SummarizedTitles
);
