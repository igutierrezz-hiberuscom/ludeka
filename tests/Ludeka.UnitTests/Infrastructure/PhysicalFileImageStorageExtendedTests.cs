using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Ludeka.Infrastructure.Services;
using Xunit;

namespace Ludeka.UnitTests.Infrastructure;

public class PhysicalFileImageStorageExtendedTests : IDisposable
{
    private readonly string _tempDirectory;
    private readonly PhysicalFileImageStorageService _service;

    public PhysicalFileImageStorageExtendedTests()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), "ludeka_ext_img_tests_" + Guid.NewGuid().ToString("N"));
        _service = new PhysicalFileImageStorageService(customPath: _tempDirectory);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
        {
            try
            {
                Directory.Delete(_tempDirectory, recursive: true);
            }
            catch
            {
            }
        }
    }

    [Fact]
    public async Task SaveEventPosterAsync_ValidJpg_SavesInEventsDirectory()
    {
        // Arrange
        var content = Encoding.UTF8.GetBytes("fake-event-poster-bytes");
        using var stream = new MemoryStream(content);

        // Act
        var result = await _service.SaveEventPosterAsync(
            eventSlugOrId: "festival-cordoba-2026",
            contentStream: stream,
            originalFileName: "cartel.jpg",
            contentType: "image/jpeg");

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.RelativePath);
        Assert.StartsWith("/images/events/festival-cordoba-2026-poster-", result.RelativePath);
        Assert.EndsWith(".jpg", result.RelativePath);

        var savedFile = Path.Combine(_tempDirectory, "events", Path.GetFileName(result.RelativePath));
        Assert.True(File.Exists(savedFile));
    }

    [Fact]
    public async Task SaveCommunityImageAsync_ValidPng_SavesInSpecifiedSubfolder()
    {
        // Arrange
        var content = Encoding.UTF8.GetBytes("fake-community-image-bytes");
        using var stream = new MemoryStream(content);

        // Act
        var result = await _service.SaveCommunityImageAsync(
            subfolder: "draws",
            identifier: "sorteo-catan-aniversario",
            contentStream: stream,
            originalFileName: "sorteo.png",
            contentType: "image/png");

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.RelativePath);
        Assert.StartsWith("/images/draws/sorteo-catan-aniversario-img-", result.RelativePath);
        Assert.EndsWith(".png", result.RelativePath);

        var savedFile = Path.Combine(_tempDirectory, "draws", Path.GetFileName(result.RelativePath));
        Assert.True(File.Exists(savedFile));
    }

    [Fact]
    public async Task SaveEventPosterAsync_EmptyStream_FailsGracefully()
    {
        using var emptyStream = new MemoryStream();

        var result = await _service.SaveEventPosterAsync(
            eventSlugOrId: "evento-vacio",
            contentStream: emptyStream,
            originalFileName: "vacio.jpg",
            contentType: "image/jpeg");

        Assert.False(result.Success);
        Assert.Contains("vacío", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task SaveEventPosterAsync_DisallowedExtension_FailsGracefully()
    {
        var content = Encoding.UTF8.GetBytes("malicious-script");
        using var stream = new MemoryStream(content);

        var result = await _service.SaveEventPosterAsync(
            eventSlugOrId: "evento-peligroso",
            contentStream: stream,
            originalFileName: "script.exe",
            contentType: "application/x-msdownload");

        Assert.False(result.Success);
        Assert.Contains("Formato no admitido", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }
}
