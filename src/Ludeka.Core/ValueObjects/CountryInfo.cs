using System;

namespace Ludeka.Core.ValueObjects;

/// <summary>
/// Representa la información canónica de un país o ámbito territorial en Ludeka.
/// </summary>
public record CountryInfo
{
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string FlagEmoji { get; init; } = string.Empty;

    public CountryInfo() { }

    public CountryInfo(string code, string name, string flagEmoji)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("El código del país no puede estar vacío.", nameof(code));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("El nombre del país no puede estar vacío.", nameof(name));

        Code = code.Trim().ToUpperInvariant();
        Name = name.Trim();
        FlagEmoji = flagEmoji?.Trim() ?? string.Empty;
    }

    public string DisplayName => !string.IsNullOrWhiteSpace(FlagEmoji) ? $"{FlagEmoji} {Name}" : Name;

    public override string ToString() => DisplayName;
}
