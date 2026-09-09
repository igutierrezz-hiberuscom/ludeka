using System;

namespace Ludeka.Web.Components.Home;

/// <summary>
/// Variantes de fondo del hero editorial de la portada (INC-35, Decisión 2).
/// El cambio de variante es una sola línea en <c>HomeDashboard.razor</c> vía el
/// parámetro <c>Background</c> de <c>HeroEditorial</c>, sin tocar el markup del resto
/// del hero. La variante activa vive en una única constante del orquestador: si la
/// foto elegida no convence, cambiarla no exige redeploy de assets nuevos.
/// </summary>
public enum HeroBackgroundVariant
{
    /// <summary>Foto de ambiente: eurogame en marcha sobre el tapete (variante por defecto).</summary>
    FotoEurogame,

    /// <summary>Foto de ambiente: grupo de amigos alrededor de una mesa de madera.</summary>
    FotoMesaAmigos,

    /// <summary>Foto de ambiente: primer plano de manos colocando piezas sobre el tablero.</summary>
    FotoPrimerPlano,

    /// <summary>Escena de serie compuesta con CSS y variables de tema (sin peticiones de imagen).</summary>
    CssScene,

    /// <summary>Ilustración editorial futura (convención de nombre reservada: hero-ilustracion.*).</summary>
    Ilustracion,
}

/// <summary>
/// Mapa variante → assets servidos (<c>wwwroot/images/home</c>) y texto alternativo en
/// castellano (Decisión 10). Los tres formatos AVIF/WebP/JPG comparten la misma base de
/// nombre, generada por <c>scripts/convert-hero-images.mjs</c>. La escena CSS no tiene
/// asset de imagen, por lo que sus rutas y su alt quedan vacíos por diseño: en esa
/// variante el <c>&lt;picture&gt;</c> no se renderiza.
/// </summary>
public static class HeroBackgroundAssets
{
    /// <summary>Ruta del asset AVIF de la variante (vacío en la escena CSS).</summary>
    public static string Avif(HeroBackgroundVariant variant) => BaseName(variant) + ".avif";

    /// <summary>Ruta del asset WebP de la variante (vacío en la escena CSS).</summary>
    public static string Webp(HeroBackgroundVariant variant) => BaseName(variant) + ".webp";

    /// <summary>Ruta del asset JPEG de fallback de la variante (vacío en la escena CSS).</summary>
    public static string Jpg(HeroBackgroundVariant variant) => BaseName(variant) + ".jpg";

    /// <summary>
    /// Alt descriptivo en castellano que evoca la escena de ambiente (Decisión 10);
    /// nunca vacío en las variantes con imagen.
    /// </summary>
    public static string AltText(HeroBackgroundVariant variant) => variant switch
    {
        HeroBackgroundVariant.FotoEurogame => "Mesa de juego con un eurogame en marcha sobre el tapete y una estantería lúdica al fondo",
        HeroBackgroundVariant.FotoMesaAmigos => "Grupo de amigos riendo alrededor de una mesa de madera con juegos de mesa",
        HeroBackgroundVariant.FotoPrimerPlano => "Primer plano de manos colocando piezas sobre el tablero de un juego de mesa",
        HeroBackgroundVariant.Ilustracion => "Ilustración editorial de una mesa de juego con estantería al fondo",
        _ => string.Empty,
    };

    private static string BaseName(HeroBackgroundVariant variant) => variant switch
    {
        HeroBackgroundVariant.FotoEurogame => "/images/home/hero-ambiente-eurogame",
        HeroBackgroundVariant.FotoMesaAmigos => "/images/home/hero-ambiente-mesa-amigos",
        HeroBackgroundVariant.FotoPrimerPlano => "/images/home/hero-ambiente-primer-plano",
        HeroBackgroundVariant.Ilustracion => "/images/home/hero-ilustracion",
        _ => string.Empty,
    };
}
