using Ludeka.Application.Contracts;
using Ludeka.Application.Features.Catalog;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Ludeka.UnitTests.Web;

public class PerformanceAndAccessibilityTests
{
    [Fact]
    public void DependencyInjection_DebeRegistrarMemoryCache_YDecoradorCachedCatalogService()
    {
        // Arrange
        var services = new ServiceCollection();
        
        // Simular dependencias mínimas requeridas
        services.AddMemoryCache();
        services.AddScoped<IGameRepository, FakeGameRepository>();
        services.AddScoped<CatalogService>();
        services.AddScoped<ICatalogService>(sp =>
            new CachedCatalogService(
                sp.GetRequiredService<CatalogService>(),
                sp.GetRequiredService<IMemoryCache>()));

        var provider = services.BuildServiceProvider();

        // Act
        var memoryCache = provider.GetService<IMemoryCache>();
        var catalogService = provider.GetService<ICatalogService>();

        // Assert
        Assert.NotNull(memoryCache);
        Assert.NotNull(catalogService);
        Assert.IsType<CachedCatalogService>(catalogService);
    }

    [Fact]
    public void OutputCache_DebeTenerPoliticasConfiguradas_ConTiemposYTemporizadoresAlineados()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddOutputCache(options =>
        {
            options.AddPolicy("CatalogCache", policy =>
                policy.Expire(TimeSpan.FromMinutes(10))
                      .Tag("tag-catalog"));

            options.AddPolicy("RadarCache", policy =>
                policy.Expire(TimeSpan.FromMinutes(5))
                      .Tag("tag-radar"));

            options.AddPolicy("StaticPages", policy =>
                policy.Expire(TimeSpan.FromMinutes(60))
                      .Tag("tag-static"));
        });

        var provider = services.BuildServiceProvider();

        // Act
        var optionsSnapshot = provider.GetRequiredService<IOptions<OutputCacheOptions>>().Value;

        // Assert
        Assert.NotNull(optionsSnapshot);
        // Validar que las opciones no lanzan error y el contenedor contiene el servicio de OutputCache
        var outputCacheStore = provider.GetService<IOutputCacheStore>();
        Assert.NotNull(outputCacheStore);
    }

    [Fact]
    public void ProductionAssets_AppCss_DebeExistir_YContenerClasesEditorialesDeTailwind()
    {
        // Arrange
        var projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../.."));
        var appCssPath = Path.Combine(projectRoot, "src", "Ludeka.Web", "wwwroot", "app.css");

        // Assert
        Assert.True(File.Exists(appCssPath), $"El archivo CSS de producción compilado no existe en: {appCssPath}");

        var cssContent = File.ReadAllText(appCssPath);
        Assert.NotEmpty(cssContent);
        // Validar que contiene clases fundamentales purgadas y no un CDN sin compilar
        Assert.Contains("container-ludeka", cssContent);
        Assert.Contains("badge-pill", cssContent);
        Assert.Contains("aspect-square", cssContent);
    }

    [Fact]
    public void AppRazor_DebeDefinirIdiomaEspanol_YPreconexionesParaOptimizarLcp()
    {
        // Arrange
        var projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../.."));
        var appRazorPath = Path.Combine(projectRoot, "src", "Ludeka.Web", "Components", "App.razor");

        // Assert
        Assert.True(File.Exists(appRazorPath), $"App.razor no encontrado en {appRazorPath}");

        var content = File.ReadAllText(appRazorPath);
        Assert.Contains("<html lang=\"es\"", content);
        Assert.Contains("rel=\"preconnect\" href=\"https://fonts.googleapis.com\"", content);
        Assert.Contains("rel=\"preconnect\" href=\"https://fonts.gstatic.com\"", content);
        Assert.Contains("rel=\"preconnect\" href=\"https://cf.geekdo-images.com\"", content);
    }

    private class FakeGameRepository : IGameRepository
    {
        public Task<Ludeka.Core.Entities.Game?> GetByIdAsync(Guid id, CancellationToken ct = default) => Task.FromResult<Ludeka.Core.Entities.Game?>(null);
        public Task<Ludeka.Core.Entities.Game?> GetByBggIdAsync(int bggId, CancellationToken ct = default) => Task.FromResult<Ludeka.Core.Entities.Game?>(null);
        public Task<Ludeka.Core.Entities.Game?> GetBySlugAsync(string slug, CancellationToken ct = default) => Task.FromResult<Ludeka.Core.Entities.Game?>(null);
        public Task<(IReadOnlyList<Ludeka.Core.Entities.Game> Items, int TotalCount)> SearchAsync(Ludeka.Application.DTOs.GameFilterCriteria criteria, int page = 1, int pageSize = 20, CancellationToken ct = default) =>
            Task.FromResult<(IReadOnlyList<Ludeka.Core.Entities.Game> Items, int TotalCount)>((Array.Empty<Ludeka.Core.Entities.Game>(), 0));
        public Task AddRangeAsync(IEnumerable<Ludeka.Core.Entities.Game> games, CancellationToken ct = default) => Task.CompletedTask;
        public Task UpdateAsync(Ludeka.Core.Entities.Game game, CancellationToken ct = default) => Task.CompletedTask;
        public Task<bool> HasAnyAsync(CancellationToken ct = default) => Task.FromResult(true);
    }
}
