using System.Collections.Generic;
using Ludeka.Core.ValueObjects;

namespace Ludeka.Application.Contracts;

/// <summary>
/// DTO con la información de compra contextual de una funda en una tienda asociada.
/// </summary>
public record SleevePurchaseOptionDto(
    string StoreName,
    string StoreLogoUrl,
    string PurchaseUrl,
    string Country,
    string? FormattedPrice = null,
    string? Badge = null,
    bool IsDirectPartner = true,
    IReadOnlyList<string>? ShippingCountries = null
);

/// <summary>
/// Contrato para la resolución contextual y quirúrgica de enlaces de compra de fundas por tienda y país.
/// </summary>
public interface ISleeveStoreUrlResolver
{
    /// <summary>
    /// Genera la URL directa al filtro de tamaño o búsqueda de fundas para una tienda concreta.
    /// </summary>
    string ResolveStoreUrl(string storeName, double widthMm, double heightMm, string? affiliateCode = null);

    /// <summary>
    /// Resuelve las opciones de compra disponibles para una especificación de funda dada, filtrando por el país del usuario.
    /// </summary>
    IReadOnlyList<SleevePurchaseOptionDto> ResolvePurchaseOptions(SleeveItem sleeve, string? userCountry = null);

    /// <summary>
    /// Encuentra el formato estándar del catálogo que mejor se ajusta a las dimensiones.
    /// </summary>
    StandardSleeveFormat? MatchStandardFormat(double widthMm, double heightMm);
}
