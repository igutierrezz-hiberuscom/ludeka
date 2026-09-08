namespace Ludeka.Infrastructure.YouTube;

public class YouTubeOptions
{
    public const string SectionName = "YouTube";

    /// <summary>
    /// Clave oficial de Google Cloud para YouTube Data API v3.
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// URL base de la API de YouTube v3.
    /// </summary>
    public string BaseUrl { get; set; } = "https://www.googleapis.com/youtube/v3/";

    /// <summary>
    /// Si es true o si ApiKey está vacía, opera en modo de salvaguardas (dataset mock).
    /// Si es false y existe ApiKey, consulta YouTube en vivo.
    /// </summary>
    public bool Simulate { get; set; } = false;

    /// <summary>
    /// Determina si debe simular (solo si se fuerza Simulate o si no se ha provisto ApiKey).
    /// </summary>
    public bool ShouldSimulate => Simulate || string.IsNullOrWhiteSpace(ApiKey);
}
