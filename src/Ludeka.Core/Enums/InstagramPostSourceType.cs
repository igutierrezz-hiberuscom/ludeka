namespace Ludeka.Core.Enums;

/// <summary>
/// Tipo de contenido de origen a partir del cual se compone un post para Instagram.
/// </summary>
public enum InstagramPostSourceType
{
    /// <summary>
    /// Sorteo lúdico activo en la plataforma.
    /// </summary>
    Giveaway,

    /// <summary>
    /// Novedad editorial o lanzamiento de tiendas.
    /// </summary>
    WeeklyRelease,

    /// <summary>
    /// Ficha técnica y veredicto de juego del catálogo.
    /// </summary>
    Game,

    /// <summary>
    /// Publicación manual o libre creada por moderación.
    /// </summary>
    Manual
}
