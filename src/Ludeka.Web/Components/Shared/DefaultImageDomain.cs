namespace Ludeka.Web.Components.Shared;

/// <summary>Dominio temático del asset por defecto de una tarjeta de Ludeka.</summary>
public enum DefaultImageDomain
{
    Evento,
    Sorteo,
    Novedad,
    Generico,
}

/// <summary>
/// Ruta estática servible del asset por defecto por dominio (variante para el caso
/// <c>onerror</c>, donde Blazor no existe en cliente). Nunca devuelve un valor vacío:
/// un dominio no contemplado cae en la variante genérica (spec default-image-fallbacks).
/// </summary>
public static class DefaultImageAssets
{
    public static string Url(DefaultImageDomain domain) => domain switch
    {
        DefaultImageDomain.Evento => "/images/defaults/evento-default.svg",
        DefaultImageDomain.Sorteo => "/images/defaults/sorteo-default.svg",
        DefaultImageDomain.Novedad => "/images/defaults/novedad-default.svg",
        _ => "/images/defaults/generico-default.svg",
    };
}
