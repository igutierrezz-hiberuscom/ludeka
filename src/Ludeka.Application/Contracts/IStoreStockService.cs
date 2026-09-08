using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ludeka.Core.ValueObjects;

namespace Ludeka.Application.Contracts;

/// <summary>
/// Servicio de orquestación de disponibilidad de stock con caché L1, límite de tiempo estricto y degradación resiliente.
/// </summary>
public interface IStoreStockService
{
    /// <summary>
    /// Obtiene el estado de disponibilidad de una oferta comercial individual.
    /// </summary>
    ValueTask<StoreStockInfo> GetStockAsync(string storeName, string affiliateUrl, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene en lote el estado de disponibilidad de múltiples ofertas concurrentemente.
    /// </summary>
    ValueTask<IReadOnlyDictionary<string, StoreStockInfo>> GetStockBatchAsync(IEnumerable<GamePurchaseLink> offers, CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalida la entrada de caché para una URL de compra determinada.
    /// </summary>
    void InvalidateStockCache(string affiliateUrl);
}
