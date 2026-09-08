namespace Ludeka.Core.Enums;

/// <summary>
/// Estados del ciclo de vida de un reporte de incidencia de catálogo.
/// </summary>
public enum GameReportStatus
{
    /// <summary>
    /// Recibido por la comunidad y a la espera de triaje por moderación.
    /// </summary>
    Pending = 1,

    /// <summary>
    /// En revisión activa por un moderador específico.
    /// </summary>
    InReview = 2,

    /// <summary>
    /// Incidencia revisada, subsanada y cerrada satisfactoriamente.
    /// </summary>
    Resolved = 3,

    /// <summary>
    /// Descartada (reporte inválido, falso positivo, dato correcto o duplicado).
    /// </summary>
    Dismissed = 4
}
