using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit;

namespace Ludeka.UnitTests.Web;

/// <summary>
/// Contrato del catálogo de iconos Lucide (INC-35, spec iconography-lucide, requerimiento
/// «Componente Icon.razor»): el catálogo es una whitelist nombre kebab-case → paths y el
/// componente resuelve el icono con un lookup que, ante un nombre desconocido, devuelve
/// vacío sin lanzar (el componente renderiza sin salida, nunca un sustituto).
///
/// Acceso por reflexión sobre el ensamblado de Ludeka.Web (patrón INC-31, sin bUnit):
/// permite ejecutar el ciclo RED antes de que exista el tipo y no acopla el test a la
/// firma concreta más allá del miembro público «Icons».
/// </summary>
public class IconCatalogTests
{
    private static readonly string[] NombresDePortada =
    {
        "search", "dices", "gift", "newspaper", "tent", "trophy", "star", "timer",
        "rocket", "refresh-cw", "sparkles", "calendar-days", "calendar", "map-pin",
        "globe", "puzzle",
    };

    private static Type? GetIconCatalogType()
    {
        var webAssembly = typeof(Ludeka.Web.Components.Pages.HomeDashboard).Assembly;
        return webAssembly.GetType("Ludeka.Web.Components.Shared.IconCatalog", throwOnError: false);
    }

    private static IReadOnlyDictionary<string, string>? GetIconsDictionary()
    {
        var type = GetIconCatalogType();
        if (type is null)
        {
            return null;
        }

        var property = type.GetProperty("Icons", BindingFlags.Public | BindingFlags.Static);
        if (property?.GetValue(null) is IReadOnlyDictionary<string, string> fromProperty)
        {
            return fromProperty;
        }

        var field = type.GetField("Icons", BindingFlags.Public | BindingFlags.Static);
        return field?.GetValue(null) as IReadOnlyDictionary<string, string>;
    }

    [Fact]
    public void IconCatalog_ExponeElDiccionarioWhitelistDeIconos()
    {
        var icons = GetIconsDictionary();

        Assert.NotNull(icons);
        Assert.NotEmpty(icons!);
    }

    [Theory]
    [InlineData("search")]
    [InlineData("dices")]
    [InlineData("gift")]
    [InlineData("newspaper")]
    [InlineData("tent")]
    [InlineData("trophy")]
    [InlineData("star")]
    [InlineData("timer")]
    [InlineData("rocket")]
    [InlineData("refresh-cw")]
    [InlineData("sparkles")]
    [InlineData("calendar-days")]
    [InlineData("calendar")]
    [InlineData("map-pin")]
    [InlineData("globe")]
    [InlineData("puzzle")]
    public void IconCatalog_ContieneLosNombresDePortadaConPathsLucide(string iconName)
    {
        var icons = GetIconsDictionary();

        Assert.NotNull(icons);
        Assert.True(icons!.TryGetValue(iconName, out var paths),
            $"El catálogo de iconos no contiene '{iconName}'.");
        Assert.False(string.IsNullOrWhiteSpace(paths),
            $"El icono '{iconName}' existe pero su contenido de paths está vacío.");
        Assert.Contains("/>", paths!);
    }

    [Fact]
    public void IconCatalog_NombreDesconocidoDevuelveVacioSinLanzar()
    {
        var icons = GetIconsDictionary();

        Assert.NotNull(icons);
        Assert.False(icons!.TryGetValue("icono-inexistente-ludeka", out var paths));
        Assert.True(string.IsNullOrEmpty(paths));
    }
}
