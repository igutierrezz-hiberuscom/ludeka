using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Ludeka.Application.Contracts;
using Ludeka.Application.DTOs;
using Ludeka.Application.Options;
using Ludeka.Core.Enums;
using Ludeka.Core.ValueObjects;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Ludeka.Infrastructure.Notifications;

public class DiscordWebhookClient : IDiscordWebhookClient
{
    private readonly HttpClient _httpClient;
    private readonly CommunityNotificationOptions _options;
    private readonly ILogger<DiscordWebhookClient> _logger;

    public DiscordWebhookClient(
        HttpClient httpClient,
        IOptions<CommunityNotificationOptions> options,
        ILogger<DiscordWebhookClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<NotificationDispatchResult> SendAsync(
        CommunityNotificationMessage message,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        if (!_options.IsDiscordConfigured)
        {
            return new NotificationDispatchResult(false, NotificationChannel.Discord, NotificationStatus.Failed, "URL de webhook de Discord no configurada.");
        }

        try
        {
            var color = message.EventType switch
            {
                NotificationEventType.GiveawayExpiring => 0xF59E0B, // Ámbar / Alerta
                NotificationEventType.FoundingVerdictPublished => 0x10B981, // Verde Sello
                _ => 0xE05A38 // Terracota Corporativo Ludeka
            };

            var embed = new DiscordEmbedDto
            {
                Title = message.Title,
                Description = message.Description,
                Url = message.TargetUrl,
                Color = color,
                Timestamp = DateTimeOffset.UtcNow.ToString("o"),
                Footer = new DiscordFooterDto { Text = "Ludeka • Comunidad de Juegos de Mesa en Español" },
                Thumbnail = !string.IsNullOrWhiteSpace(message.ImageUrl) ? new DiscordMediaDto { Url = message.ImageUrl } : null,
                Fields = message.Fields?.Select(f => new DiscordFieldDto
                {
                    Name = f.Key,
                    Value = f.Value,
                    Inline = true
                }).ToList() ?? []
            };

            var payload = new DiscordWebhookPayloadDto
            {
                Username = "Ludeka Bot",
                AvatarUrl = "https://ludeka.es/images/logo.png",
                Embeds = [embed]
            };

            using var response = await _httpClient.PostAsJsonAsync(_options.DiscordWebhookUrl, payload, ct);

            if (response.IsSuccessStatusCode)
            {
                return new NotificationDispatchResult(true, NotificationChannel.Discord, NotificationStatus.Sent);
            }

            var errorBody = await response.Content.ReadAsStringAsync(ct);
            var errorMessage = $"HTTP {(int)response.StatusCode} ({response.ReasonPhrase}): {errorBody}";
            _logger.LogWarning("Discord Webhook error: {Error}", errorMessage);

            return new NotificationDispatchResult(false, NotificationChannel.Discord, NotificationStatus.Failed, errorMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Excepción al invocar Discord Webhook: {Message}", ex.Message);
            return new NotificationDispatchResult(false, NotificationChannel.Discord, NotificationStatus.Failed, ex.Message);
        }
    }

    private class DiscordWebhookPayloadDto
    {
        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;

        [JsonPropertyName("avatar_url")]
        public string? AvatarUrl { get; set; }

        [JsonPropertyName("embeds")]
        public List<DiscordEmbedDto> Embeds { get; set; } = [];
    }

    private class DiscordEmbedDto
    {
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("url")]
        public string? Url { get; set; }

        [JsonPropertyName("color")]
        public int Color { get; set; }

        [JsonPropertyName("timestamp")]
        public string? Timestamp { get; set; }

        [JsonPropertyName("footer")]
        public DiscordFooterDto? Footer { get; set; }

        [JsonPropertyName("thumbnail")]
        public DiscordMediaDto? Thumbnail { get; set; }

        [JsonPropertyName("fields")]
        public List<DiscordFieldDto> Fields { get; set; } = [];
    }

    private class DiscordFooterDto
    {
        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;
    }

    private class DiscordMediaDto
    {
        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;
    }

    private class DiscordFieldDto
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("value")]
        public string Value { get; set; } = string.Empty;

        [JsonPropertyName("inline")]
        public bool Inline { get; set; }
    }
}
