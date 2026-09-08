using System;
using System.Threading;
using System.Threading.Tasks;
using Ludeka.Application.Contracts;
using Ludeka.Application.Options;
using Ludeka.Core.ValueObjects;
using Microsoft.Extensions.Options;

namespace Ludeka.Infrastructure.Stores;

/// <summary>
/// Cliente determinista para entornos de desarrollo y pruebas automatizadas de verificación de stock.
/// </summary>
public class SimulationStoreStockClient : IStoreStockClient
{
    private readonly StoreStockOptions _options;

    public int Priority => 1; // Máxima precedencia cuando está activo o la URL es de prueba

    public SimulationStoreStockClient(IOptions<StoreStockOptions>? options = null)
    {
        _options = options?.Value ?? new StoreStockOptions();
    }

    public bool CanHandle(string storeName, string affiliateUrl)
    {
        if (string.IsNullOrWhiteSpace(affiliateUrl))
            return false;

        if (_options.EnableSimulation)
            return true;

        var lower = affiliateUrl.ToLowerInvariant();
        return lower.Contains("simulation") ||
               lower.Contains("mock") ||
               lower.Contains("ludeka.test") ||
               lower.Contains("example.com");
    }

    public async Task<StoreStockInfo> CheckStockAsync(string storeName, string affiliateUrl, CancellationToken cancellationToken = default)
    {
        var lower = (affiliateUrl ?? string.Empty).ToLowerInvariant();

        if (lower.Contains("timeout") || lower.Contains("lento"))
        {
            // Simular demora para provocar timeout
            await Task.Delay(2500, cancellationToken);
        }

        if (lower.Contains("error") || lower.Contains("caida"))
        {
            throw new System.Net.Http.HttpRequestException($"Fallo simulado 500 al conectar con '{storeName}'.");
        }

        if (lower.Contains("agotado") || lower.Contains("outofstock"))
        {
            return StoreStockInfo.OutOfStock("Sin existencias en almacén (simulado).");
        }

        if (lower.Contains("ultimas") || lower.Contains("lowstock"))
        {
            return StoreStockInfo.LowStock(quantity: 2, price: 34.95m, note: "Últimas 2 unidades disponibles (simulado).");
        }

        return StoreStockInfo.InStock(quantity: 8, price: 39.99m, note: "Stock verificado en tienda colaboradora (simulado).");
    }
}
