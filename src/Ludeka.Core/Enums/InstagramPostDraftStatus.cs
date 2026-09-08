namespace Ludeka.Core.Enums;

/// <summary>
/// Estado del ciclo de vida de un borrador de publicación en Instagram.
/// </summary>
public enum InstagramPostDraftStatus
{
    /// <summary>
    /// Borrador en edición y revisión editorial.
    /// </summary>
    Draft,

    /// <summary>
    /// En proceso de envío y despacho a la Meta Graph API.
    /// </summary>
    Publishing,

    /// <summary>
    /// Publicado exitosamente en el feed oficial de Instagram de Ludeka.
    /// </summary>
    Published,

    /// <summary>
    /// Error durante el intento de publicación en la API de Meta.
    /// </summary>
    Failed
}
