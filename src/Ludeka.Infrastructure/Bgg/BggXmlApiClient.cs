using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.RateLimiting;
using System.Threading.Tasks;
using System.Xml.Linq;
using Ludeka.Application.Contracts;
using Ludeka.Core.Entities;

namespace Ludeka.Infrastructure.Bgg;

public class BggXmlApiClient : IBggClient, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly RateLimiter _rateLimiter;
    private readonly bool _ownsHttpClient;

    public BggXmlApiClient(HttpClient? httpClient = null)
    {
        _ownsHttpClient = httpClient == null;
        _httpClient = httpClient ?? new HttpClient();

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

    public void Dispose()
    {
        _rateLimiter.Dispose();
        if (_ownsHttpClient)
        {
            _httpClient.Dispose();
        }
    }
}
