using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Ludeka.Application.Contracts;
using Ludeka.Application.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Ludeka.Infrastructure.Services;

public class InstagramApiClient : IInstagramApiClient
{
    private readonly HttpClient _httpClient;
    private readonly InstagramOptions _options;
    private readonly ILogger<InstagramApiClient> _logger;

    public InstagramApiClient(
        HttpClient httpClient,
        IOptions<InstagramOptions> options,
        ILogger<InstagramApiClient> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _options = options?.Value ?? new InstagramOptions();
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<string> CreateMediaContainerAsync(string imageUrl, string caption, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
            throw new ArgumentException("La URL de la imagen no puede estar vacía.", nameof(imageUrl));

        if (!_options.IsConfigured)
        {
            _logger.LogInformation("InstagramApiClient en modo simulado: Creando contenedor para imagen '{ImageUrl}'", imageUrl);
            var simCreationId = $"sim_container_{Guid.NewGuid().ToString("N")[..12]}";
            return simCreationId;
        }

        var endpoint = $"{_options.BaseUrl.TrimEnd('/')}/{_options.InstagramAccountId}/media";

        var postData = new Dictionary<string, string>
        {
            ["image_url"] = imageUrl,
            ["caption"] = caption ?? string.Empty,
            ["access_token"] = _options.AccessToken!
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = new FormUrlEncodedContent(postData)
        };

        using var response = await _httpClient.SendAsync(request, ct);
        var responseContent = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Error al crear contenedor en Instagram API. Código: {StatusCode}, Respuesta: {Response}", response.StatusCode, responseContent);
            var errorMessage = ExtractMetaErrorMessage(responseContent) ?? $"Error {response.StatusCode} al crear contenedor de Instagram.";
            throw new HttpRequestException(errorMessage, null, response.StatusCode);
        }

        using var doc = JsonDocument.Parse(responseContent);
        if (doc.RootElement.TryGetProperty("id", out var idProp))
        {
            return idProp.GetString() ?? throw new InvalidOperationException("No se recibió el ID del contenedor de Instagram.");
        }

        throw new InvalidOperationException($"Respuesta inesperada de Instagram API: {responseContent}");
    }

    public async Task<string> PublishMediaAsync(string creationId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(creationId))
            throw new ArgumentException("El ID de creación del contenedor no puede estar vacío.", nameof(creationId));

        if (!_options.IsConfigured)
        {
            _logger.LogInformation("InstagramApiClient en modo simulado: Publicando contenedor '{CreationId}'", creationId);
            var simMediaId = $"sim_media_{Guid.NewGuid().ToString("N")[..12]}";
            return simMediaId;
        }

        var endpoint = $"{_options.BaseUrl.TrimEnd('/')}/{_options.InstagramAccountId}/media_publish";

        var postData = new Dictionary<string, string>
        {
            ["creation_id"] = creationId,
            ["access_token"] = _options.AccessToken!
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = new FormUrlEncodedContent(postData)
        };

        using var response = await _httpClient.SendAsync(request, ct);
        var responseContent = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Error al publicar contenedor en Instagram API. Código: {StatusCode}, Respuesta: {Response}", response.StatusCode, responseContent);
            var errorMessage = ExtractMetaErrorMessage(responseContent) ?? $"Error {response.StatusCode} al publicar en Instagram.";
            throw new HttpRequestException(errorMessage, null, response.StatusCode);
        }

        using var doc = JsonDocument.Parse(responseContent);
        if (doc.RootElement.TryGetProperty("id", out var idProp))
        {
            return idProp.GetString() ?? throw new InvalidOperationException("No se recibió el ID del post publicado en Instagram.");
        }

        throw new InvalidOperationException($"Respuesta inesperada de Instagram API: {responseContent}");
    }

    public async Task<string?> GetPermalinkAsync(string mediaId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(mediaId))
            return null;

        if (!_options.IsConfigured)
        {
            return $"https://www.instagram.com/p/{mediaId}/";
        }

        try
        {
            var endpoint = $"{_options.BaseUrl.TrimEnd('/')}/{mediaId}?fields=permalink&access_token={_options.AccessToken}";
            using var response = await _httpClient.GetAsync(endpoint, ct);
            var responseContent = await response.Content.ReadAsStringAsync(ct);

            if (response.IsSuccessStatusCode)
            {
                using var doc = JsonDocument.Parse(responseContent);
                if (doc.RootElement.TryGetProperty("permalink", out var permalinkProp))
                {
                    return permalinkProp.GetString();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo recuperar el permalink exacto para el media ID '{MediaId}'", mediaId);
        }

        return $"https://www.instagram.com/p/{mediaId}/";
    }

    private static string? ExtractMetaErrorMessage(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("error", out var errorObj) &&
                errorObj.TryGetProperty("message", out var messageProp))
            {
                return messageProp.GetString();
            }
        }
        catch
        {
            // Ignorar errores de parsing si no es JSON
        }

        return null;
    }
}
