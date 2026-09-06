using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.RateLimiting;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Xml.Linq;
using Microsoft.Extensions.Options;
using Ludeka.Application.Contracts;
using Ludeka.Application.DTOs;
using Ludeka.Core.Entities;

namespace Ludeka.Infrastructure.Bgg;

public class BggXmlApiClient : IBggClient, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly RateLimiter _rateLimiter;
    private readonly bool _ownsHttpClient;

    public BggXmlApiClient(HttpClient? httpClient = null, IOptions<BggOptions>? options = null)
    {
        _ownsHttpClient = httpClient == null;
        _httpClient = httpClient ?? new HttpClient();

        var bggOpts = options?.Value ?? new BggOptions();
        if (!string.IsNullOrWhiteSpace(bggOpts.ApiToken))
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bggOpts.ApiToken.Trim());
        }

        if (!_httpClient.DefaultRequestHeaders.UserAgent.Any())
        {
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(bggOpts.UserAgent);
        }

        // Limitar a máximo 2 peticiones por segundo para cortesía hacia los servidores de BGG
        _rateLimiter = new TokenBucketRateLimiter(new TokenBucketRateLimiterOptions
        {
            TokenLimit = 2,
            QueueLimit = 10,
            TokensPerPeriod = 2,
            ReplenishmentPeriod = TimeSpan.FromSeconds(1),
            AutoReplenishment = true
        });
    }

    public async Task<Game?> FetchGameByBggIdAsync(int bggId, CancellationToken ct = default)
    {
        if (bggId <= 0) return null;

        string url = $"https://boardgamegeek.com/xmlapi2/thing?id={bggId}&stats=1";

        int maxRetries = 3;
        int delayMs = 1500;

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            using var lease = await _rateLimiter.AcquireAsync(1, ct);
            if (!lease.IsAcquired)
            {
                await Task.Delay(500, ct);
                continue;
            }

            try
            {
                using var response = await _httpClient.GetAsync(url, ct);

                // Si BGG responde 202 (Accepted), la petición ha sido encolada para procesamiento interno
                if (response.StatusCode == HttpStatusCode.Accepted)
                {
                    if (attempt < maxRetries)
                    {
                        await Task.Delay(delayMs * attempt, ct);
                        continue;
                    }
                    return null;
                }

                if (response.StatusCode == (HttpStatusCode)429)
                {
                    if (attempt < maxRetries)
                    {
                        await Task.Delay(delayMs * 2 * attempt, ct);
                        continue;
                    }
                    return null;
                }

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new HttpRequestException("401 Unauthorized: BGG requiere autenticación mediante Application Token (Bearer). Consulta https://boardgamegeek.com/applications y configura 'Bgg:ApiToken'.", null, HttpStatusCode.Unauthorized);
                }

                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                string xmlContent = await response.Content.ReadAsStringAsync(ct);
                if (string.IsNullOrWhiteSpace(xmlContent)) return null;

                var doc = XDocument.Parse(xmlContent);
                var item = doc.Root?.Element("item");
                if (item == null) return null;

                return BggXmlParser.ParseItem(item);
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw;
            }
            catch (HttpRequestException) when (attempt < maxRetries)
            {
                await Task.Delay(delayMs, ct);
            }
            catch (Exception)
            {
                return null;
            }
        }

        return null;
    }

    public async Task<IReadOnlyList<BggCollectionItemDto>> FetchUserCollectionAsync(string username, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(username)) return [];

        string url = $"https://boardgamegeek.com/xmlapi2/collection?username={Uri.EscapeDataString(username)}&stats=1";

        int maxRetries = 4;
        int delayMs = 2000;

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            using var lease = await _rateLimiter.AcquireAsync(1, ct);
            if (!lease.IsAcquired)
            {
                await Task.Delay(500, ct);
                continue;
            }

            try
            {
                using var response = await _httpClient.GetAsync(url, ct);

                // Si BGG responde 202 (Accepted), la colección está encolada para generarse en caché
                if (response.StatusCode == HttpStatusCode.Accepted)
                {
                    if (attempt < maxRetries)
                    {
                        await Task.Delay(delayMs * attempt, ct);
                        continue;
                    }
                    return [];
                }

                if (response.StatusCode == (HttpStatusCode)429)
                {
                    if (attempt < maxRetries)
                    {
                        await Task.Delay(delayMs * 2 * attempt, ct);
                        continue;
                    }
                    return [];
                }

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new HttpRequestException("401 Unauthorized: BGG requiere autenticación mediante Application Token (Bearer). Consulta https://boardgamegeek.com/applications y configura 'Bgg:ApiToken'.", null, HttpStatusCode.Unauthorized);
                }

                if (!response.IsSuccessStatusCode)
                {
                    return [];
                }

                string xmlContent = await response.Content.ReadAsStringAsync(ct);
                if (string.IsNullOrWhiteSpace(xmlContent)) return [];

                var doc = XDocument.Parse(xmlContent);
                return BggXmlParser.ParseCollection(doc);
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw;
            }
            catch (HttpRequestException) when (attempt < maxRetries)
            {
                await Task.Delay(delayMs, ct);
            }
            catch (Exception)
            {
                return [];
            }
        }

        return [];
    }

    public async Task<IReadOnlyList<BggSearchResultDto>> SearchGamesAsync(string query, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Trim().Length < 2) return [];

        string url = $"https://boardgamegeek.com/xmlapi2/search?query={Uri.EscapeDataString(query.Trim())}&type=boardgame";

        int maxRetries = 3;
        int delayMs = 1000;

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            using var lease = await _rateLimiter.AcquireAsync(1, ct);
            if (!lease.IsAcquired)
            {
                await Task.Delay(500, ct);
                continue;
            }

            try
            {
                using var response = await _httpClient.GetAsync(url, ct);

                if (response.StatusCode == (HttpStatusCode)429)
                {
                    if (attempt < maxRetries)
                    {
                        await Task.Delay(delayMs * 2 * attempt, ct);
                        continue;
                    }
                    return [];
                }

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new HttpRequestException("401 Unauthorized: BGG requiere autenticación mediante Application Token (Bearer). Consulta https://boardgamegeek.com/applications y configura 'Bgg:ApiToken'.", null, HttpStatusCode.Unauthorized);
                }

                if (!response.IsSuccessStatusCode)
                {
                    return [];
                }

                string xmlContent = await response.Content.ReadAsStringAsync(ct);
                if (string.IsNullOrWhiteSpace(xmlContent)) return [];

                var doc = XDocument.Parse(xmlContent);
                return BggXmlParser.ParseSearchResults(doc);
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw;
            }
            catch (HttpRequestException) when (attempt < maxRetries)
            {
                await Task.Delay(delayMs, ct);
            }
            catch (Exception)
            {
                return [];
            }
        }

        return [];
    }

    public void Dispose()
    {
        _rateLimiter.Dispose();
        if (_ownsHttpClient)
        {
            _httpClient.Dispose();
        }
    }
}
