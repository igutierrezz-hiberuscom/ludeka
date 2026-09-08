using System;
using System.Reflection;
using Ludeka.Web.Components.Pages;
using Microsoft.AspNetCore.Components;
using Xunit;

namespace Ludeka.UnitTests.Web;

/// <summary>
/// Comportamiento del buscador rápido de la portada (INC-31, spec home-landing-hero,
/// requerimiento "Buscador rápido conservado"): al enviar el formulario, la navegación
/// resulta en /catalogo?q={termino}. Se ejecuta el handler real de HomeDashboard contra
/// un NavigationManager falso (monitor) que captura el URI destino.
///
/// Sin bUnit (decisión de diseño de INC-31): el componente se instancia directamente y
/// la propiedad inyectada se resuelve por reflexión, sin tocar código de producción.
/// </summary>
public class HomeDashboardQuickSearchTests
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
        var dashboard = new HomeDashboard();

        SetInjectedNavigation(dashboard, navigation);

        var searchField = typeof(HomeDashboard).GetField("_searchTerm", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("No se encontró el campo '_searchTerm' en HomeDashboard.");
        searchField.SetValue(dashboard, searchTerm);

        var handler = typeof(HomeDashboard).GetMethod("HandleQuickSearch", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("No se encontró el método 'HandleQuickSearch' en HomeDashboard.");
        handler.Invoke(dashboard, null);

        return navigation;
    }

    private static void SetInjectedNavigation(HomeDashboard dashboard, NavigationManager navigation)
    {
        var type = typeof(HomeDashboard);
        var property = type.GetProperty("Navigation", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        if (property?.GetSetMethod(nonPublic: true) != null)
        {
            property.SetValue(dashboard, navigation);
            return;
        }

        var backingField = type.GetField("<Navigation>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("No se pudo asignar el NavigationManager inyectado de HomeDashboard.");
        backingField.SetValue(dashboard, navigation);
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
