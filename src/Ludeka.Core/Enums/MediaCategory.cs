namespace Ludeka.Core.Enums;

/// <summary>
/// Taxonomía formal de contenidos multimedia en el Hub de Ludeka.
/// </summary>
public enum MediaCategory
{
    /// <summary>
    /// Vídeos breves ("Cómo Funciona") que resumen en 2 minutos las mecánicas principales y dinámica de juego sin ser un tutorial completo.
    /// </summary>
    QuickOverview = 0,

    /// <summary>
    /// Explicaciones detalladas de reglas completas y guías paso a paso de cómo jugar.
    /// </summary>
    Tutorial = 1,

    /// <summary>
    /// Partidas completas o demostrativas con número explícito de comensales.
    /// </summary>
    Gameplay = 2,

    /// <summary>
    /// Reseñas críticas, análisis de sensaciones, veredictos de creadores y primeras impresiones.
    /// </summary>
    ReviewOpinion = 3
}
