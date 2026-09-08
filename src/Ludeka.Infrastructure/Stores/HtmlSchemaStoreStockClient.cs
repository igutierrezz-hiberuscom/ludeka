using System;
using System.Globalization;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Ludeka.Application.Contracts;
using Ludeka.Core.Enums;
using Ludeka.Core.ValueObjects;

namespace Ludeka.Infrastructure.Stores;

/// <summary>
/// Cliente HTTP que inspecciona páginas de tiendas comerciales buscando microdatos estándar Schema.org (JSON-LD / Microdata) y OpenGraph.
/// </summary>
public class HtmlSchemaStoreStockClient : IStoreStockClient
{
    private readonly HttpClient _httpClient;

    public int Priority => 100; // Prioridad estándar (fallback web)

    public HtmlSchemaStoreStockClient(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        if (!_httpClient.DefaultRequestHeaders.Contains("User-Agent"))
        {
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/128.0.0.0 Safari/537.36 LudekaStockChecker/1.0");
        }
    }

    public bool CanHandle(string storeName, string affiliateUrl)
    {
        if (string.IsNullOrWhiteSpace(affiliateUrl))
            return false;

        return Uri.TryCreate(affiliateUrl, UriKind.Absolute, out var uri) &&
               (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }

    public async Task<StoreStockInfo> CheckStockAsync(string storeName, string affiliateUrl, CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _httpClient.GetAsync(affiliateUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return StoreStockInfo.Unknown($"Tienda respondió con estado HTTP {(int)response.StatusCode}.");
            }

            var html = await response.Content.ReadAsStringAsync(cancellationToken);
            return ParseStockFromHtml(html, storeName);
        }
        catch (OperationCanceledException)
        {
            throw; // Propagar para que el servicio aplique el timeout exacto
        }
        catch (Exception ex)
        {
            return StoreStockInfo.Unknown($"Error al inspeccionar tienda: {ex.Message}");
        }
    }

    /// <summary>
    /// Analiza el documento HTML extrayendo señales de disponibilidad estructurada Schema.org, OpenGraph y heurísticas en español.
    /// </summary>
    public static StoreStockInfo ParseStockFromHtml(string html, string storeName = "Tienda")
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return StoreStockInfo.Unknown("Contenido HTML vacío.");
        }

        decimal? price = ExtractPrice(html);

        // 1. Schema.org JSON-LD / Microdata
        var schemaMatch = Regex.Match(
            html,
            @"(?:""availability""\s*:\s*""(?:https?://schema\.org/)?(InStock|OutOfStock|LimitedAvailability|PreOrder|Discontinued)""|" +
            @"itemprop=[""']availability[""'][^>]*href=[""'](?:https?://schema\.org/)?(InStock|OutOfStock|LimitedAvailability|PreOrder|Discontinued)[""']|" +
            @"href=[""'](?:https?://schema\.org/)?(InStock|OutOfStock|LimitedAvailability|PreOrder|Discontinued)[""'][^>]*itemprop=[""']availability[""'])",
            RegexOptions.IgnoreCase);

        if (schemaMatch.Success)
        {
            var value = schemaMatch.Groups[1].Success ? schemaMatch.Groups[1].Value :
                        schemaMatch.Groups[2].Success ? schemaMatch.Groups[2].Value :
                        schemaMatch.Groups[3].Value;

            return MapSchemaValue(value, price);
        }

        // 2. OpenGraph / Product Meta Tags
        var ogMatch = Regex.Match(
            html,
            @"<meta\s+(?:property|name)=[""'](?:og:availability|product:availability)[""']\s+content=[""']([^""']+)[""']",
            RegexOptions.IgnoreCase);

        if (ogMatch.Success)
        {
            var ogVal = ogMatch.Groups[1].Value.Trim().ToLowerInvariant();
            if (ogVal == "instock" || ogVal == "in stock" || ogVal == "available")
                return StoreStockInfo.InStock(price: price, note: "Confirmado por metadatos OpenGraph.");
            if (ogVal == "oos" || ogVal == "outofstock" || ogVal == "out of stock")
                return StoreStockInfo.OutOfStock("Agotado según metadatos OpenGraph.");
            if (ogVal == "lowstock" || ogVal == "low stock")
                return StoreStockInfo.LowStock(price: price, note: "Pocas unidades según metadatos OpenGraph.");
        }

        // 3. Heurísticas léxicas en español para tiendas de juegos
        if (Regex.IsMatch(html, @"\b(sin existencias|agotado|temporalmente agotado|fuera de stock|descatalogado)\b", RegexOptions.IgnoreCase))
        {
            return StoreStockInfo.OutOfStock("Detectado como agotado en la ficha de la tienda.");
        }

        if (Regex.IsMatch(html, @"\b(últimas unidades|pocas unidades|quedan pocas unidades)\b", RegexOptions.IgnoreCase))
        {
            return StoreStockInfo.LowStock(price: price, note: "Últimas unidades detectadas en tienda.");
        }

        if (Regex.IsMatch(html, @"\b(en stock|disponible para envío|añadir al carrito|comprar)\b", RegexOptions.IgnoreCase))
        {
            return StoreStockInfo.InStock(price: price, note: "Disponible en la tienda.");
        }

        return StoreStockInfo.Unknown("Sin indicación clara de disponibilidad en la web.");
    }

    private static StoreStockInfo MapSchemaValue(string value, decimal? price)
    {
        return value.ToLowerInvariant() switch
        {
            "instock" => StoreStockInfo.InStock(price: price, note: "Verificado en Schema.org (InStock)."),
            "limitedavailability" => StoreStockInfo.LowStock(price: price, note: "Disponibilidad limitada en Schema.org."),
            "preorder" => StoreStockInfo.LowStock(price: price, note: "En preventa en Schema.org."),
            "outofstock" or "discontinued" => StoreStockInfo.OutOfStock("Agotado en Schema.org (OutOfStock)."),
            _ => StoreStockInfo.Unknown($"Estado Schema.org '{value}' no concluyente.")
        };
    }

    private static decimal? ExtractPrice(string html)
    {
        var priceMatch = Regex.Match(
            html,
            @"(?:""price""\s*:\s*""?([0-9]+(?:[\.,][0-9]{1,2})?)""?|itemprop=[""']price[""'][^>]*content=[""']([0-9]+(?:[\.,][0-9]{1,2})?)[""'])",
            RegexOptions.IgnoreCase);

        if (priceMatch.Success)
        {
            var raw = priceMatch.Groups[1].Success ? priceMatch.Groups[1].Value : priceMatch.Groups[2].Value;
            raw = raw.Replace(',', '.');
            if (decimal.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed))
            {
                return parsed;
            }
        }

        return null;
    }
}
