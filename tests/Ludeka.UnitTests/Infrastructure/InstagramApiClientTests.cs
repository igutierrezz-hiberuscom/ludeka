using System;
using System.Net.Http;
using System.Threading.Tasks;
using Ludeka.Application.Options;
using Ludeka.Infrastructure.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace Ludeka.UnitTests.Infrastructure;

public class InstagramApiClientTests
{
    [Fact]
    public async Task CreateMediaContainer_SimulatedMode_ReturnsSimulatedContainerId()
    {
        // Arrange
        var options = Options.Create(new InstagramOptions
        {
            AccessToken = null,
            InstagramAccountId = null
        });

        using var httpClient = new HttpClient();
        var client = new InstagramApiClient(httpClient, options, NullLogger<InstagramApiClient>.Instance);

        // Act
        var creationId = await client.CreateMediaContainerAsync("https://ejemplo.com/foto.jpg", "Texto de prueba");

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(creationId));
        Assert.StartsWith("sim_container_", creationId);
    }

    [Fact]
    public async Task PublishMedia_SimulatedMode_ReturnsSimulatedMediaId()
    {
        // Arrange
        var options = Options.Create(new InstagramOptions());
        using var httpClient = new HttpClient();
        var client = new InstagramApiClient(httpClient, options, NullLogger<InstagramApiClient>.Instance);

        // Act
        var mediaId = await client.PublishMediaAsync("sim_container_12345");

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(mediaId));
        Assert.StartsWith("sim_media_", mediaId);
    }

    [Fact]
    public async Task GetPermalink_SimulatedMode_ReturnsInstagramPermalink()
    {
        // Arrange
        var options = Options.Create(new InstagramOptions());
        using var httpClient = new HttpClient();
        var client = new InstagramApiClient(httpClient, options, NullLogger<InstagramApiClient>.Instance);

        // Act
        var permalink = await client.GetPermalinkAsync("media_abc123");

        // Assert
        Assert.Equal("https://www.instagram.com/p/media_abc123/", permalink);
    }

    [Fact]
    public async Task CreateMediaContainer_EmptyImageUrl_ThrowsArgumentException()
    {
        // Arrange
        var options = Options.Create(new InstagramOptions());
        using var httpClient = new HttpClient();
        var client = new InstagramApiClient(httpClient, options, NullLogger<InstagramApiClient>.Instance);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            client.CreateMediaContainerAsync("", "Caption"));
    }

    [Fact]
    public async Task PublishMedia_EmptyCreationId_ThrowsArgumentException()
    {
        // Arrange
        var options = Options.Create(new InstagramOptions());
        using var httpClient = new HttpClient();
        var client = new InstagramApiClient(httpClient, options, NullLogger<InstagramApiClient>.Instance);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            client.PublishMediaAsync(""));
    }
}
