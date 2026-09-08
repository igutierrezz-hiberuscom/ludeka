using System.Net.Http;
using System.Threading.Tasks;
using Ludeka.Core.Enums;
using Ludeka.Infrastructure.Stores;
using Xunit;

namespace Ludeka.UnitTests.Infrastructure;

public class StoreStockClientTests
{
    [Fact]
    public void ParseStockFromHtml_WithSchemaMicrodataInStock_ShouldReturnInStock()
    {
        var html = @"
            <html>
            <head><title>Ark Nova en Zacatrus</title></head>
            <body>
                <span itemprop=""price"" content=""64.95"">64.95 €</span>
                <link itemprop=""availability"" href=""https://schema.org/InStock"" />
            </body>
            </html>";

        var result = HtmlSchemaStoreStockClient.ParseStockFromHtml(html, "Zacatrus");

        Assert.Equal(StockStatus.InStock, result.Status);
        Assert.Equal(64.95m, result.CurrentPrice);
        Assert.Contains("InStock", result.StatusNote);
    }

    [Fact]
    public void ParseStockFromHtml_WithSchemaMicrodataOutOfStock_ShouldReturnOutOfStock()
    {
        var html = @"
            <html>
            <body>
                <link itemprop=""availability"" href=""https://schema.org/OutOfStock"" />
            </body>
            </html>";

        var result = HtmlSchemaStoreStockClient.ParseStockFromHtml(html, "Cuarto de Juegos");

        Assert.Equal(StockStatus.OutOfStock, result.Status);
        Assert.Contains("OutOfStock", result.StatusNote);
    }

    [Fact]
    public void ParseStockFromHtml_WithJsonLdInStock_ShouldReturnInStock()
    {
        var html = @"
            <html>
            <head>
                <script type=""application/ld+json"">
                {
                    ""@context"": ""https://schema.org/"",
                    ""@type"": ""Product"",
                    ""name"": ""Catan"",
                    ""offers"": {
                        ""@type"": ""Offer"",
                        ""price"": ""38.50"",
                        ""priceCurrency"": ""EUR"",
                        ""availability"": ""https://schema.org/InStock""
                    }
                }
                </script>
            </head>
            <body><h1>Catan</h1></body>
            </html>";

        var result = HtmlSchemaStoreStockClient.ParseStockFromHtml(html, "Zacatrus");

        Assert.Equal(StockStatus.InStock, result.Status);
        Assert.Equal(38.50m, result.CurrentPrice);
    }

    [Fact]
    public void ParseStockFromHtml_WithOpenGraphOos_ShouldReturnOutOfStock()
    {
        var html = @"
            <html>
            <head>
                <meta property=""og:availability"" content=""oos"" />
            </head>
            <body><h1>Juego Descargado</h1></body>
            </html>";

        var result = HtmlSchemaStoreStockClient.ParseStockFromHtml(html, "Dungeon Marvels");

        Assert.Equal(StockStatus.OutOfStock, result.Status);
    }

    [Fact]
    public void ParseStockFromHtml_WithSpanishHeuristics_ShouldDetectStock()
    {
        var htmlAgotado = "<div>Lo sentimos, este producto está temporalmente agotado y sin existencias.</div>";
        var resultAgotado = HtmlSchemaStoreStockClient.ParseStockFromHtml(htmlAgotado);
        Assert.Equal(StockStatus.OutOfStock, resultAgotado.Status);

        var htmlUltimas = "<div>¡Apresúrate! Quedan últimas unidades disponibles en tienda.</div>";
        var resultUltimas = HtmlSchemaStoreStockClient.ParseStockFromHtml(htmlUltimas);
        Assert.Equal(StockStatus.LowStock, resultUltimas.Status);

        var htmlDisponible = "<button class=\"btn\">Añadir al carrito</button>";
        var resultDisponible = HtmlSchemaStoreStockClient.ParseStockFromHtml(htmlDisponible);
        Assert.Equal(StockStatus.InStock, resultDisponible.Status);
    }

    [Fact]
    public void ParseStockFromHtml_WithEmptyOrUnrecognizableContent_ShouldReturnUnknown()
    {
        var resultEmpty = HtmlSchemaStoreStockClient.ParseStockFromHtml("");
        Assert.Equal(StockStatus.Unknown, resultEmpty.Status);

        var resultGeneric = HtmlSchemaStoreStockClient.ParseStockFromHtml("<html><body><h1>Página sin datos</h1></body></html>");
        Assert.Equal(StockStatus.Unknown, resultGeneric.Status);
    }

    [Fact]
    public async Task SimulationStoreStockClient_ShouldReturnDeterministicStatuses()
    {
        var client = new SimulationStoreStockClient();

        Assert.True(client.CanHandle("Mock Store", "https://ludeka.test/juego"));
        Assert.False(client.CanHandle("Real Store", "https://tiendareal.com/juego"));

        var inStock = await client.CheckStockAsync("Test", "https://ludeka.test/wingspan");
        Assert.Equal(StockStatus.InStock, inStock.Status);
        Assert.Equal(8, inStock.AvailableQuantity);

        var outOfStock = await client.CheckStockAsync("Test", "https://ludeka.test/juego-agotado");
        Assert.Equal(StockStatus.OutOfStock, outOfStock.Status);

        var lowStock = await client.CheckStockAsync("Test", "https://ludeka.test/juego-ultimas-unidades");
        Assert.Equal(StockStatus.LowStock, lowStock.Status);
        Assert.Equal(2, lowStock.AvailableQuantity);

        await Assert.ThrowsAsync<HttpRequestException>(() =>
            client.CheckStockAsync("Test", "https://ludeka.test/juego-error-500"));
    }
}
