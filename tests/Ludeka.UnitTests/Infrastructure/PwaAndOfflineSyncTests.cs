using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Ludeka.Application.DTOs;
using Ludeka.Core.Enums;
using Xunit;

namespace Ludeka.UnitTests.Infrastructure;

public class PwaAndOfflineSyncTests
{
    private static string FindRepoRoot()
    {
        var dir = Directory.GetCurrentDirectory();
        while (dir != null && !File.Exists(Path.Combine(dir, "Ludeka.sln")))
        {
            var parent = Directory.GetParent(dir);
            dir = parent?.FullName;
        }
        return dir ?? throw new InvalidOperationException("No se encontró la raíz del repositorio con Ludeka.sln");
    }

    [Fact]
    public void Manifest_ShouldBeValidJson_AndContainRequiredPwaFields()
    {
        // Arrange
        string root = FindRepoRoot();
        string manifestPath = Path.Combine(root, "src", "Ludeka.Web", "wwwroot", "manifest.webmanifest");

        Assert.True(File.Exists(manifestPath), $"El manifiesto no existe en {manifestPath}");

        string json = File.ReadAllText(manifestPath);

        // Act
        using var doc = JsonDocument.Parse(json);
        var rootElement = doc.RootElement;

        // Assert
        Assert.True(rootElement.TryGetProperty("name", out var nameProp));
        Assert.False(string.IsNullOrWhiteSpace(nameProp.GetString()));

        Assert.True(rootElement.TryGetProperty("short_name", out var shortNameProp));
        Assert.Equal("Ludeka", shortNameProp.GetString());

        Assert.True(rootElement.TryGetProperty("start_url", out var startUrlProp));
        Assert.Equal("/mi-ludoteca", startUrlProp.GetString());

        Assert.True(rootElement.TryGetProperty("display", out var displayProp));
        Assert.Equal("standalone", displayProp.GetString());

        Assert.True(rootElement.TryGetProperty("theme_color", out var themeColorProp));
        Assert.Equal("#d97706", themeColorProp.GetString());

        Assert.True(rootElement.TryGetProperty("icons", out var iconsProp));
        Assert.True(iconsProp.GetArrayLength() >= 3);
    }

    [Fact]
    public void Manifest_ReferencedIcons_ShouldExistPhysically()
    {
        // Arrange
        string root = FindRepoRoot();
        string wwwroot = Path.Combine(root, "src", "Ludeka.Web", "wwwroot");
        string manifestPath = Path.Combine(wwwroot, "manifest.webmanifest");

        string json = File.ReadAllText(manifestPath);
        using var doc = JsonDocument.Parse(json);
        var icons = doc.RootElement.GetProperty("icons");

        // Act & Assert
        foreach (var icon in icons.EnumerateArray())
        {
            string src = icon.GetProperty("src").GetString()!;
            string cleanSrc = src.TrimStart('/');
            string iconFullPath = Path.Combine(wwwroot, cleanSrc.Replace('/', Path.DirectorySeparatorChar));

            Assert.True(File.Exists(iconFullPath), $"El archivo de icono referenciado '{src}' no existe en {iconFullPath}");
        }
    }

    [Fact]
    public void ServiceWorker_And_OfflineHtml_ShouldExistPhysically()
    {
        // Arrange
        string root = FindRepoRoot();
        string wwwroot = Path.Combine(root, "src", "Ludeka.Web", "wwwroot");
        string swPath = Path.Combine(wwwroot, "service-worker.js");
        string offlineHtmlPath = Path.Combine(wwwroot, "offline.html");
        string offlineJsPath = Path.Combine(wwwroot, "js", "ludeka-offline.js");

        // Assert
        Assert.True(File.Exists(swPath), "service-worker.js no existe.");
        Assert.True(File.Exists(offlineHtmlPath), "offline.html no existe.");
        Assert.True(File.Exists(offlineJsPath), "ludeka-offline.js no existe.");

        string swContent = File.ReadAllText(swPath);
        Assert.Contains("PRECACHE_ASSETS", swContent);
        Assert.Contains("addEventListener('install'", swContent);
        Assert.Contains("addEventListener('activate'", swContent);
        Assert.Contains("addEventListener('fetch'", swContent);
        Assert.Contains("/offline.html", swContent);

        string offlineHtmlContent = File.ReadAllText(offlineHtmlPath);
        Assert.Contains("Sin Conexión", offlineHtmlContent, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("/mi-ludoteca", offlineHtmlContent);
    }

    [Fact]
    public void OfflineLibrarySnapshotDto_SerializationAndConversion_PreservesDataIntegrity()
    {
        // Arrange
        var item1 = new UserCollectionItemDto(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Wingspan",
            "https://images.ludeka.es/wingspan.jpg",
            "wingspan",
            CollectionStatus.InCollection,
            DateTimeOffset.UtcNow,
            false,
            1001,
            false,
            false
        );

        var loan1 = new GameLoanDto(
            Guid.NewGuid(),
            item1.GameId!.Value,
            "Wingspan",
            item1.GameCoverUrl,
            item1.GameSlug,
            "Carlos Amigo",
            DateTimeOffset.UtcNow.AddDays(-3),
            "Funda estándar cuidada",
            false,
            null
        );

        var originalSummary = new UserLibrarySummaryDto(
            TotalInCollection: 1,
            TotalPlayed: 3,
            TotalWishlist: 2,
            TotalWantToBuy: 1,
            TotalActiveLoans: 1,
            Items: [item1],
            ActiveLoans: [loan1]
        );

        // Act
        var snapshot = OfflineLibrarySnapshotDto.FromSummary("usuario-offline-1", originalSummary);

        // Serialización JSON
        string json = JsonSerializer.Serialize(snapshot);
        var deserialized = JsonSerializer.Deserialize<OfflineLibrarySnapshotDto>(json);

        Assert.NotNull(deserialized);
        Assert.Equal("usuario-offline-1", deserialized.UserId);

        var restoredSummary = deserialized.ToSummary();

        // Assert
        Assert.Equal(originalSummary.TotalInCollection, restoredSummary.TotalInCollection);
        Assert.Equal(originalSummary.TotalPlayed, restoredSummary.TotalPlayed);
        Assert.Equal(originalSummary.TotalWishlist, restoredSummary.TotalWishlist);
        Assert.Equal(originalSummary.TotalWantToBuy, restoredSummary.TotalWantToBuy);
        Assert.Equal(originalSummary.TotalActiveLoans, restoredSummary.TotalActiveLoans);

        Assert.Single(restoredSummary.Items);
        Assert.Equal("Wingspan", restoredSummary.Items[0].GameTitle);
        Assert.Equal(CollectionStatus.InCollection, restoredSummary.Items[0].Status);

        Assert.Single(restoredSummary.ActiveLoans);
        Assert.Equal("Carlos Amigo", restoredSummary.ActiveLoans[0].BorrowerName);
    }
}
