namespace Ludeka.Core.Enums;

/// <summary>
/// Tipologías de incidencias o erratas reportables por la comunidad en fichas de catálogo.
/// </summary>
public enum GameIssueType
{
    /// <summary>
    /// La carátula no corresponde al juego, es de baja resolución o pertenece a otra edición lingüística.
    /// </summary>
    WrongImage = 1,

    /// <summary>
    /// La imagen no carga, enlace caído o error 404.
    /// </summary>
    BrokenImage = 2,

    /// <summary>
    /// Rango de comensales (mín/máx) incorrecto o semáforo de escalabilidad desajustado.
    /// </summary>
    IncorrectPlayerCount = 3,

    /// <summary>
    /// Duración estimada por partida desajustada.
    /// </summary>
    IncorrectDuration = 4,

    /// <summary>
    /// Edad recomendada o legal inconsistente.
    /// </summary>
    IncorrectAge = 5,

    /// <summary>
    /// Título, diseñador, editorial, año de publicación o sinopsis con erratas.
    /// </summary>
    ErroneousMetadata = 6,

    /// <summary>
    /// Enlace a tienda o afiliado defectuoso, fuera de stock permanente o mal dirigido.
    /// </summary>
    BrokenPurchaseLink = 7,

    /// <summary>
    /// Otra incidencia o sugerencia lúdica no contemplada en las anteriores.
    /// </summary>
    Other = 8
}
