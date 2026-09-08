namespace Ludeka.Core.Enums;

/// <summary>
/// Representa las fases operativas por las que transita el proceso de importación
/// de colecciones de usuarios desde BoardGameGeek (BGG XMLAPI2).
/// </summary>
public enum BggImportPhase
{
    /// <summary>
    /// Inicializando los parámetros de la solicitud y preparando la conexión.
    /// </summary>
    Initializing = 0,

    /// <summary>
    /// Contactando con la API de BoardGameGeek (/xmlapi2/collection).
    /// </summary>
    RequestingBgg = 1,

    /// <summary>
    /// BGG ha respondido con código 202 Accepted; la colección se está generando en sus servidores.
    /// </summary>
    PreparingInBgg = 2,

    /// <summary>
    /// BGG o Cloudflare ha devuelto un código 429 o 503; se aguarda el tiempo de Retry-After.
    /// </summary>
    RateLimitedWaiting = 3,

    /// <summary>
    /// XML recibido y validado; cruzando los títulos con el catálogo local y la cola de catalogación.
    /// </summary>
    ProcessingItems = 4,

    /// <summary>
    /// La importación se ha completado satisfactoriamente.
    /// </summary>
    Completed = 5,

    /// <summary>
    /// La importación ha fallado debido a un error irrecuperable o agotamiento de tiempo de espera.
    /// </summary>
    Failed = 6
}
