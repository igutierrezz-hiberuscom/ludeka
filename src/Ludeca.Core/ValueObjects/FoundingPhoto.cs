using System;

namespace Ludeca.Core.ValueObjects;

public record FoundingPhoto
{
    public string PhotoUrl { get; init; } = string.Empty;
    public string Caption { get; init; } = string.Empty;

    // Constructor privado para serialización EF Core / JSON
    public FoundingPhoto() { }

    public FoundingPhoto(string photoUrl, string caption)
    {
        if (string.IsNullOrWhiteSpace(photoUrl))
            throw new ArgumentException("La URL de la foto de mesa real no puede estar vacía.", nameof(photoUrl));

        PhotoUrl = photoUrl.Trim();
        Caption = caption?.Trim() ?? string.Empty;

        if (Caption.Length > 200)
            throw new ArgumentException("El pie de foto no puede exceder los 200 caracteres.", nameof(caption));
    }
}
