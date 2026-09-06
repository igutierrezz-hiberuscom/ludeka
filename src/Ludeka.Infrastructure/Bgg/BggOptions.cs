namespace Ludeka.Infrastructure.Bgg;

public class BggOptions
{
    public const string SectionName = "Bgg";

    /// <summary>
    /// Token de autorización Bearer emitido por BoardGameGeek (Applications).
    /// </summary>
    public string? ApiToken { get; set; }

    /// <summary>
    /// URL base para la API XMLAPI2.
    /// </summary>
    public string BaseUrl { get; set; } = "https://boardgamegeek.com/xmlapi2/";

    /// <summary>
    /// Cabecera User-Agent requerida por las directrices de BGG.
    /// </summary>
    public string UserAgent { get; set; } = "LudekaApp/1.0 (https://ludeka.es; contacto@ludeka.es)";
}
