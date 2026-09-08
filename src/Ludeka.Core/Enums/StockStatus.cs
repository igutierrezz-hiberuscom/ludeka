namespace Ludeka.Core.Enums;

/// <summary>
/// Representa el estado de disponibilidad y stock de un juego en una tienda comercial.
/// </summary>
public enum StockStatus
{
    /// <summary>
    /// Estado no determinado, timeout en la tienda o sin datos recientes.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Producto disponible con existencias confirmadas para compra inmediata.
    /// </summary>
    InStock = 1,

    /// <summary>
    /// Últimas unidades disponibles en inventario.
    /// </summary>
    LowStock = 2,

    /// <summary>
    /// Producto sin existencias o temporalmente agotado.
    /// </summary>
    OutOfStock = 3
}
