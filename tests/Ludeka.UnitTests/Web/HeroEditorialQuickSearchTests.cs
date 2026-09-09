using System;
using System.Reflection;
using Ludeka.Web.Components.Home;
using Microsoft.AspNetCore.Components;
using Xunit;

namespace Ludeka.UnitTests.Web;

/// <summary>
/// Comportamiento del buscador rápido del hero editorial (INC-35 PR-2, spec
/// home-landing-hero, requerimiento «Buscador rápido conservado»): al enviar el
/// formulario, la navegación resulta en /catalogo?q={termino}. Tras la extracción del
/// hero (Decisión 3), el handler vive en HeroEditorial y se ejecuta real contra un
/// NavigationManager falso (monitor) que captura el URI destino.
///
/// Migración del antiguo HomeDashboardQuickSearchTests: mismo escenario, mismo patrón
/// de reflexión (sin bUnit, decisión de diseño de INC-31), componente instanciado
/// directamente y propiedad inyectada resuelta por reflexión.
/// </summary>
public class HeroEditorialQuickSearchTests
{
    private class FakeNavigationManager : NavigationManager
    {
        public string? LastNavigatedUri { get; private set; }

        public FakeNavigationManager()
        {
            Initialize("http://localhost/", "http://localhost/");
        }

        protected override void NavigateToCore(string uri, bool forceLoad)
        {
            LastNavigatedUri = uri;
        }
    }

    private static FakeNavigationManager InvokeQuickSearch(string searchTerm)
    {
        var navigation = new FakeNavigationManager();
        var hero = new HeroEditorial();

        SetInjectedNavigation(hero, navigation);

        var searchField = typeof(HeroEditorial).GetField("_searchTerm", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("No se encontró el campo '_searchTerm' en HeroEditorial.");
        searchField.SetValue(hero, searchTerm);

        var handler = typeof(HeroEditorial).GetMethod("HandleQuickSearch", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("No se encontró el método 'HandleQuickSearch' en HeroEditorial.");
        handler.Invoke(hero, null);

        return navigation;
    }

    private static void SetInjectedNavigation(HeroEditorial hero, NavigationManager navigation)
    {
        var type = typeof(HeroEditorial);
        var property = type.GetProperty("Navigation", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        if (property?.GetSetMethod(nonPublic: true) != null)
        {
            property.SetValue(hero, navigation);
            return;
        }

        var backingField = type.GetField("<Navigation>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("No se pudo asignar el NavigationManager inyectado de HeroEditorial.");
        backingField.SetValue(hero, navigation);
    }

    [Fact]
    public void QuickSearch_SubmitNavigatesToCatalogoWithQueryAndFallsBackWithoutTerm()
    {
        // Escenario del spec: el visitante introduce "azul" y envía → /catalogo?q=azul
        var navigation = InvokeQuickSearch("azul");
        Assert.Equal("/catalogo?q=azul", navigation.LastNavigatedUri);

        // Caso borde: término vacío → el catálogo sin parámetro de búsqueda
        var emptyNavigation = InvokeQuickSearch(string.Empty);
        Assert.Equal("/catalogo", emptyNavigation.LastNavigatedUri);
    }
}
