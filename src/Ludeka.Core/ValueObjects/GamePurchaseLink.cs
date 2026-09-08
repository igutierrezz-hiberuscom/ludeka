using System;
using Ludeka.Core.Enums;

namespace Ludeka.Core.ValueObjects;

/// <summary>
/// Representa una oferta o enlace de compra en una tienda con parámetros de afiliación y datos orientativos.
/// </summary>
public record GamePurchaseLink
{
    public string StoreName { get; init; } = string.Empty;
    public string Country { get; init; } = "España";
    public string AffiliateUrl { get; init; } = string.Empty;
    public decimal? Price { get; init; }
    public string Currency { get; init; } = "€";
    public bool InStock { get; init; } = true;
    public string? Badge { get; init; }
    public string? AffiliateTag { get; init; }
    public IReadOnlyList<string>? ShippingCountries { get; init; }

    // Constructor sin parámetros para deserialización JSON y EF Core
    public GamePurchaseLink() { }

    public GamePurchaseLink(
        string storeName,
        string affiliateUrl,
        decimal? price = null,
        string currency = "€",
        bool inStock = true,
        string? badge = null,
        string? affiliateTag = null,
        string country = "España",
        IEnumerable<string>? shippingCountries = null)
    {
        if (string.IsNullOrWhiteSpace(storeName))
            throw new ArgumentException("El nombre de la tienda no puede estar vacío.", nameof(storeName));

        if (string.IsNullOrWhiteSpace(affiliateUrl))
            throw new ArgumentException("La URL de afiliado no puede estar vacía.", nameof(affiliateUrl));

        StoreName = storeName.Trim();
        Country = string.IsNullOrWhiteSpace(country) ? "España" : CountryCatalog.Normalize(country);
        AffiliateUrl = affiliateUrl.Trim();
        Price = price.HasValue ? Math.Max(0m, Math.Round(price.Value, 2)) : null;
        Currency = string.IsNullOrWhiteSpace(currency) ? "€" : currency.Trim();
        InStock = inStock;
        Badge = string.IsNullOrWhiteSpace(badge) ? null : badge.Trim();
        AffiliateTag = string.IsNullOrWhiteSpace(affiliateTag) ? null : affiliateTag.Trim();

        if (shippingCountries != null)
        {
            ShippingCountries = shippingCountries.Where(c => !string.IsNullOrWhiteSpace(c)).Select(c => CountryCatalog.Normalize(c)).ToList();
        }
    }

    public string FormattedPrice => Price.HasValue ? $"{Price.Value:0.00} {Currency}" : "Consultar";

    public StockStatus InitialStockStatus => InStock ? StockStatus.InStock : StockStatus.OutOfStock;

    public bool ShipsTo(string? targetCountry)
    {
        if (string.IsNullOrWhiteSpace(targetCountry))
            return true;

        var normalizedTarget = CountryCatalog.Normalize(targetCountry);

        if (string.Equals(CountryCatalog.Normalize(Country), normalizedTarget, StringComparison.OrdinalIgnoreCase))
            return true;

        if (CountryCatalog.IsInternational(Country))
            return true;

        if (ShippingCountries != null && ShippingCountries.Any(c => CountryCatalog.IsInternational(c) ||
            string.Equals(CountryCatalog.Normalize(c), normalizedTarget, StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        return false;
    }
}
