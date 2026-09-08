using System;

namespace Ludeka.Core.ValueObjects;

public record SleeveItem(
    string FormatName,
    double WidthMm,
    double HeightMm,
    int CardCount,
    string? AffiliateUrl,
    string? StoreName = null,
    string? Country = null,
    IReadOnlyList<string>? ShippingCountries = null)
{
    public int CalculatePacksNeeded(int packSize = 50)
    {
        if (packSize <= 0) throw new ArgumentOutOfRangeException(nameof(packSize), "El tamaño de paquete debe ser mayor a 0.");
        if (CardCount <= 0) return 0;
        return (int)Math.Ceiling((double)CardCount / packSize);
    }

    public int PacksNeeded50 => CalculatePacksNeeded(50);
    public int PacksNeeded100 => CalculatePacksNeeded(100);

    public string DimensionText => $"{WidthMm.ToString("0.#", System.Globalization.CultureInfo.InvariantCulture)} x {HeightMm.ToString("0.#", System.Globalization.CultureInfo.InvariantCulture)} mm";

    public bool ShipsTo(string? targetCountry)
    {
        if (string.IsNullOrWhiteSpace(targetCountry))
            return true;

        if (string.IsNullOrWhiteSpace(Country))
            return true;

        var normalizedTarget = CountryCatalog.Normalize(targetCountry);

        if (string.Equals(CountryCatalog.Normalize(Country), normalizedTarget, StringComparison.OrdinalIgnoreCase))
            return true;

        if (CountryCatalog.IsInternational(Country))
            return true;

        if (ShippingCountries != null && ShippingCountries.Any(c => string.Equals(CountryCatalog.Normalize(c), normalizedTarget, StringComparison.OrdinalIgnoreCase)))
            return true;

        return false;
    }
}
