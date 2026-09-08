using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ludeka.Application.Contracts;
using Ludeka.Application.Options;
using Ludeka.Core.ValueObjects;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Ludeka.Application.Features.Catalog;

/// <summary>
/// Implementación de IStoreStockService con caché L1 en memoria, límite de tiempo de 1.5s y degradación resiliente.
/// </summary>
public class StoreStockService : IStoreStockService
{
    private readonly IMemoryCache _cache;
    private readonly IEnumerable<IStoreStockClient> _clients;
    private readonly StoreStockOptions _options;
    private readonly ILogger<StoreStockService>? _logger;

    public StoreStockService(
        IMemoryCache cache,
        IEnumerable<IStoreStockClient> clients,
        IOptions<StoreStockOptions> options,
        ILogger<StoreStockService>? logger = null)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _clients = clients ?? throw new ArgumentNullException(nameof(clients));
        _options = options?.Value ?? new StoreStockOptions();
        _logger = logger;
    }

    public async ValueTask<StoreStockInfo> GetStockAsync(string storeName, string affiliateUrl, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(affiliateUrl))
        {
            return StoreStockInfo.Unknown("URL de compra no especificada.");
        }

        var normalizedStore = string.IsNullOrWhiteSpace(storeName) ? "Tienda" : storeName.Trim();
        var cacheKey = BuildCacheKey(affiliateUrl);

        if (_cache.TryGetValue(cacheKey, out StoreStockInfo? cached) && cached != null)
        {
            return cached;
        }

        if (!_options.EnableLiveChecking)
        {
            return StoreStockInfo.Unknown("Verificación en vivo temporalmente desactivada.");
        }

        var client = _clients
            .OrderBy(c => c.Priority)
            .FirstOrDefault(c => c.CanHandle(normalizedStore, affiliateUrl));

        if (client == null)
        {
            _logger?.LogDebug("No se encontró cliente de stock compatible para la tienda '{StoreName}'.", normalizedStore);
            return StoreStockInfo.Unknown("Sin adaptador compatible para esta tienda.");
        }

        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        linkedCts.CancelAfter(TimeSpan.FromMilliseconds(_options.TimeoutMilliseconds));

        StoreStockInfo result;
        try
        {
            result = await client.CheckStockAsync(normalizedStore, affiliateUrl, linkedCts.Token);
            var ttl = result.IsUnknown ? _options.ErrorTtlMinutes : _options.DefaultTtlMinutes;
            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(ttl));
            return result;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            _logger?.LogWarning("Timeout ({Timeout}ms) consultando stock en '{StoreName}' ({Url}).", _options.TimeoutMilliseconds, normalizedStore, affiliateUrl);
            result = StoreStockInfo.Unknown("Tiempo de espera agotado (1.5s).");
            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(_options.ErrorTtlMinutes));
            return result;
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Error al consultar stock en '{StoreName}' ({Url}): {Message}", normalizedStore, affiliateUrl, ex.Message);
            result = StoreStockInfo.Unknown("Error al conectar con la tienda.");
            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(_options.ErrorTtlMinutes));
            return result;
        }
    }

    public async ValueTask<IReadOnlyDictionary<string, StoreStockInfo>> GetStockBatchAsync(IEnumerable<GamePurchaseLink> offers, CancellationToken cancellationToken = default)
    {
        if (offers == null)
        {
            return new Dictionary<string, StoreStockInfo>();
        }

        var distinctOffers = offers
            .Where(o => !string.IsNullOrWhiteSpace(o.AffiliateUrl))
            .GroupBy(o => o.AffiliateUrl, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .ToList();

        if (distinctOffers.Count == 0)
        {
            return new Dictionary<string, StoreStockInfo>();
        }

        var tasks = distinctOffers.Select(async offer =>
        {
            var stock = await GetStockAsync(offer.StoreName, offer.AffiliateUrl, cancellationToken);
            return (offer.AffiliateUrl, stock);
        });

        var results = await Task.WhenAll(tasks);
        return results.ToDictionary(r => r.AffiliateUrl, r => r.stock, StringComparer.OrdinalIgnoreCase);
    }

    public void InvalidateStockCache(string affiliateUrl)
    {
        if (!string.IsNullOrWhiteSpace(affiliateUrl))
        {
            _cache.Remove(BuildCacheKey(affiliateUrl));
        }
    }

    private static string BuildCacheKey(string affiliateUrl)
    {
        return $"store_stock:{affiliateUrl.Trim().ToLowerInvariant()}";
    }
}
