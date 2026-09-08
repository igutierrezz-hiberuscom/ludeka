using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace Ludeka.Infrastructure.Bgg;

/// <summary>
/// DelegatingHandler para el pipeline de HttpClient de BGG.
/// Inyecta cabeceras oficiales de identificación (User-Agent), autenticación (Bearer Token)
/// y claves API (X-BGG-API-KEY), además de exponer utilidades de inspección de cabeceras de resiliencia.
/// </summary>
public class BggResilienceAndAuthHandler : DelegatingHandler
{
    private readonly BggOptions _options;

    public BggResilienceAndAuthHandler(IOptions<BggOptions>? options = null)
    {
        _options = options?.Value ?? new BggOptions();
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // 1. Inyectar User-Agent oficial si la solicitud no contiene uno
        if (!request.Headers.UserAgent.Any())
        {
            var userAgent = !string.IsNullOrWhiteSpace(_options.UserAgent)
                ? _options.UserAgent
                : "LudekaApp/1.0 (https://ludeka.es; contacto@ludeka.es)";
            request.Headers.UserAgent.ParseAdd(userAgent);
        }

        // 2. Inyectar Authorization: Bearer si existe token configurado y la solicitud no tiene autorización
        var bearerToken = _options.BearerToken;
        if (!string.IsNullOrWhiteSpace(bearerToken) && request.Headers.Authorization == null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken.Trim());
        }

        // 3. Inyectar X-BGG-API-KEY si existe ApiKey configurada
        if (!string.IsNullOrWhiteSpace(_options.ApiKey) && !request.Headers.Contains("X-BGG-API-KEY"))
        {
            request.Headers.Add("X-BGG-API-KEY", _options.ApiKey.Trim());
        }

        return await base.SendAsync(request, cancellationToken);
    }

    /// <summary>
    /// Extrae los segundos de espera especificados en la cabecera Retry-After de la respuesta HTTP, si existe.
    /// Soporta tanto valores numéricos enteros como fechas en formato RFC 1123.
    /// </summary>
    public static int? ExtractRetryAfterSeconds(HttpResponseMessage response)
    {
        if (response.Headers.RetryAfter != null)
        {
            if (response.Headers.RetryAfter.Delta.HasValue)
            {
                var seconds = (int)Math.Ceiling(response.Headers.RetryAfter.Delta.Value.TotalSeconds);
                return Math.Max(1, seconds);
            }

            if (response.Headers.RetryAfter.Date.HasValue)
            {
                var diff = (int)Math.Ceiling((response.Headers.RetryAfter.Date.Value - DateTimeOffset.UtcNow).TotalSeconds);
                return Math.Max(1, diff);
            }
        }

        if (response.Headers.TryGetValues("Retry-After", out var rawValues))
        {
            var raw = rawValues.FirstOrDefault();
            if (int.TryParse(raw, out int parsedSeconds))
            {
                return Math.Max(1, parsedSeconds);
            }
        }

        return null;
    }
}
