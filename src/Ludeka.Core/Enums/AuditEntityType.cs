namespace Ludeka.Core.Enums;

/// <summary>
/// Tipo de entidad afectada por una operación registrada en la auditoría.
/// </summary>
public enum AuditEntityType
{
    /// <summary>
    /// Ficha de juego del catálogo.
    /// </summary>
    Game,

    /// <summary>
    /// Ficha de editorial lúdica.
    /// </summary>
    Publisher,

    /// <summary>
    /// Ficha de autor, diseñador o ilustrador.
    /// </summary>
    Creator,

    /// <summary>
    /// Ficha de tienda o enlaces afiliados.
    /// </summary>
    Store,

    /// <summary>
    /// Contenido multimedia (vídeo de YouTube/Instagram).
    /// </summary>
    Media,

    /// <summary>
    /// Reporte o incidencia comunitaria.
    /// </summary>
    Report,

    /// <summary>
    /// Usuario, roles o permisos de cuenta.
    /// </summary>
    User,

    /// <summary>
    /// Publicación oficial en Instagram.
    /// </summary>
    InstagramPost
}
