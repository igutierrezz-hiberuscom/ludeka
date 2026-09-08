using System;
using Ludeka.Core.Enums;

namespace Ludeka.Core.ValueObjects;

/// <summary>
/// Encapsula la información de disponibilidad y comprobación de stock en vivo de una tienda.
/// </summary>
public record StoreStockInfo
{
    public StockStatus Status { get; init; } = StockStatus.Unknown;
    public int? AvailableQuantity { get; init; }
    public decimal? CurrentPrice { get; init; }
    public DateTime? LastCheckedUtc { get; init; }
    public string? StatusNote { get; init; }
    public bool IsChecking { get; init; }

    public bool IsInStock => Status == StockStatus.InStock;
    public bool IsLowStock => Status == StockStatus.LowStock;
    public bool IsOutOfStock => Status == StockStatus.OutOfStock;
    public bool IsUnknown => Status == StockStatus.Unknown;

    public static StoreStockInfo Checking() => new() { IsChecking = true };

    public static StoreStockInfo InStock(int? quantity = null, decimal? price = null, string? note = null) =>
        new()
        {
            Status = StockStatus.InStock,
            AvailableQuantity = quantity,
            CurrentPrice = price,
            LastCheckedUtc = DateTime.UtcNow,
            StatusNote = note
        };

    public static StoreStockInfo LowStock(int? quantity = null, decimal? price = null, string? note = null) =>
        new()
        {
            Status = StockStatus.LowStock,
            AvailableQuantity = quantity,
            CurrentPrice = price,
            LastCheckedUtc = DateTime.UtcNow,
            StatusNote = note
        };

    public static StoreStockInfo OutOfStock(string? note = null) =>
        new()
        {
            Status = StockStatus.OutOfStock,
            LastCheckedUtc = DateTime.UtcNow,
            StatusNote = note
        };

    public static StoreStockInfo Unknown(string? note = null) =>
        new()
        {
            Status = StockStatus.Unknown,
            LastCheckedUtc = DateTime.UtcNow,
            StatusNote = note
        };

    public string GetRelativeTimeText(DateTime? nowUtc = null)
    {
        if (!LastCheckedUtc.HasValue) return "Sin comprobar";
        var now = nowUtc ?? DateTime.UtcNow;
        var diff = now - LastCheckedUtc.Value;
        if (diff.TotalSeconds < 60) return "Comprobado hace instantes";
        if (diff.TotalMinutes < 60) return $"Comprobado hace {(int)diff.TotalMinutes} min";
        if (diff.TotalHours < 24) return $"Comprobado hace {(int)diff.TotalHours} h";
        return $"Comprobado el {LastCheckedUtc.Value:dd/MM}";
    }
}
