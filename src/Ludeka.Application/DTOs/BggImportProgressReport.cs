using Ludeka.Core.Enums;

namespace Ludeka.Application.DTOs;

/// <summary>
/// Reporte inmutable de progreso emitido durante la importación de colecciones desde BoardGameGeek.
/// Proporciona retroalimentación contextual y cuantitativa para la interfaz de usuario en tiempo real.
/// </summary>
public record BggImportProgressReport(
    BggImportPhase Phase,
    string Message,
    int CurrentAttempt = 0,
    int MaxAttempts = 0,
    int? WaitSeconds = null,
    int ItemsFound = 0
);
