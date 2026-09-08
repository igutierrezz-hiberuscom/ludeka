using System.Threading;
using System.Threading.Tasks;
using Ludeka.Core.ValueObjects;

namespace Ludeka.Application.Contracts;

/// <summary>
/// Contrato para clientes o adaptadores de verificación de disponibilidad y stock en tiendas comerciales.
/// </summary>
public interface IStoreStockClient
{
    /// <summary>
    /// Prioridad de evaluación del cliente (menor valor numérico indica mayor precedencia).
    /// </summary>
    int Priority { get; }

    /// <summary>
    /// Determina si este cliente tiene capacidad para verificar la oferta en la tienda dada.
    /// </summary>
    bool CanHandle(string storeName, string affiliateUrl);

    /// <summary>
    /// Ejecuta la consulta de disponibilidad y stock para la oferta comercial.
    /// </summary>
    Task<StoreStockInfo> CheckStockAsync(string storeName, string affiliateUrl, CancellationToken cancellationToken = default);
}
