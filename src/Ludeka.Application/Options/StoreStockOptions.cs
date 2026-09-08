namespace Ludeka.Application.Options;

/// <summary>
/// Opciones de configuración para el servicio de verificación de stock y disponibilidad en tiendas.
/// </summary>
public class StoreStockOptions
{
    public const string SectionName = "StoreStock";

    /// <summary>
    /// Tiempo de expiración en minutos para consultas de stock exitosas. Por defecto 30 minutos.
    /// </summary>
    public int DefaultTtlMinutes { get; set; } = 30;

    /// <summary>
    /// Tiempo de expiración en minutos para resultados desconocidos, errores o timeouts. Por defecto 5 minutos.
    /// </summary>
    public int ErrorTtlMinutes { get; set; } = 5;

    /// <summary>
    /// Límite estricto de tiempo en milisegundos para la llamada externa a la tienda. Por defecto 1.500 ms.
    /// </summary>
    public int TimeoutMilliseconds { get; set; } = 1500;

    /// <summary>
    /// Activa o desactiva la comprobación en tiempo real.
    /// </summary>
    public bool EnableLiveChecking { get; set; } = true;

    /// <summary>
    /// Activa la prioridad del adaptador de simulación determinista para pruebas y desarrollo offline.
    /// </summary>
    public bool EnableSimulation { get; set; } = false;
}
