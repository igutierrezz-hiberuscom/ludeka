using System;
using System.Reflection;
using Xunit;

namespace Ludeka.UnitTests.Web;

/// <summary>
/// Contrato de los assets por defecto de Ludeka (INC-35, spec default-image-fallbacks):
/// <c>DefaultImageAssets.Url(Domain)</c> devuelve la ruta estática del dominio, nunca un
/// valor vacío, y un dominio no contemplado cae en la variante genérica sin fallar ni
/// quedar en blanco. Acceso por reflexión sobre el ensamblado de Ludeka.Web (patrón
/// INC-31, sin bUnit): permite ejecutar el ciclo RED antes de que exista el tipo.
/// </summary>
public class DefaultImageAssetsTests
{
    private static MethodInfo? GetUrlMethod()
    {
        var webAssembly = typeof(Ludeka.Web.Components.Pages.HomeDashboard).Assembly;
        var type = webAssembly.GetType("Ludeka.Web.Components.Shared.DefaultImageAssets", throwOnError: false);
        return type?.GetMethod("Url", BindingFlags.Public | BindingFlags.Static);
    }

    private static string? InvokeUrl(int domainValue)
    {
        var method = GetUrlMethod();
        Assert.NotNull(method);

        // 0=Evento, 1=Sorteo, 2=Novedad, 3=Generico; Invoke coacciona int → enum
        return method!.Invoke(null, new object[] { domainValue }) as string;
    }

    [Theory]
    [InlineData(0, "/images/defaults/evento-default.svg")]
    [InlineData(1, "/images/defaults/sorteo-default.svg")]
    [InlineData(2, "/images/defaults/novedad-default.svg")]
    [InlineData(3, "/images/defaults/generico-default.svg")]
    public void Url_DevuelveLaRutaEstaticaPorDominio(int domainValue, string expectedUrl)
    {
        Assert.Equal(expectedUrl, InvokeUrl(domainValue));
    }

    [Fact]
    public void Url_ValorNoContempladoCaeEnGenericoSinNull()
    {
        var url = InvokeUrl(999);

        Assert.False(string.IsNullOrWhiteSpace(url));
        Assert.Equal("/images/defaults/generico-default.svg", url);
    }

    [Fact]
    public void Url_NuncaDevuelveVacioParaNingunValorDefinido()
    {
        foreach (var domainValue in new[] { 0, 1, 2, 3 })
        {
            Assert.False(string.IsNullOrWhiteSpace(InvokeUrl(domainValue)));
        }
    }
}
