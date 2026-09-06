using System;

namespace Ludeca.Core.ValueObjects;

public record SleeveItem(
    string FormatName,
    double WidthMm,
    double HeightMm,
    int CardCount,
    string? AffiliateUrl)
{
    public int CalculatePacksNeeded(int packSize = 50)
    {
        if (packSize <= 0) throw new ArgumentOutOfRangeException(nameof(packSize), "El tamaño de paquete debe ser mayor a 0.");
        if (CardCount <= 0) return 0;
        return (int)Math.Ceiling((double)CardCount / packSize);
    }

    public string DimensionText => $"{WidthMm:0.#} x {HeightMm:0.#} mm";
}
