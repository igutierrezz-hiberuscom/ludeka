using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
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

public class TelegramBotClient : ITelegramBotClient
{
    private readonly HttpClient _httpClient;
    private readonly CommunityNotificationOptions _options;
    private readonly ILogger<TelegramBotClient> _logger;

    public TelegramBotClient(
        HttpClient httpClient,
        IOptions<CommunityNotificationOptions> options,
        ILogger<TelegramBotClient> logger)
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

        if (!_options.IsTelegramConfigured)
        {
            return new NotificationDispatchResult(false, NotificationChannel.Telegram, NotificationStatus.Failed, "Token de bot o ChatId de Telegram no configurados.");
        }

        try
        {
            var baseUrl = $"https://api.telegram.org/bot{_options.TelegramBotToken}";
            var htmlText = FormatHtmlMessage(message);

            HttpResponseMessage response;

            if (!string.IsNullOrWhiteSpace(message.ImageUrl))
            {
                // Enviar foto con pie en HTML
                var photoPayload = new TelegramPhotoPayloadDto
                {
                    ChatId = _options.TelegramChatId!,
                    PhotoUrl = message.ImageUrl,
                    Caption = htmlText,
                    ParseMode = "HTML"
                };

                response = await _httpClient.PostAsJsonAsync($"{baseUrl}/sendPhoto", photoPayload, ct);
            }
            else
            {
                // Enviar mensaje de texto en HTML
                var textPayload = new TelegramTextPayloadDto
                {
                    ChatId = _options.TelegramChatId!,
                    Text = htmlText,
                    ParseMode = "HTML",
                    DisableWebPagePreview = false
                };

                response = await _httpClient.PostAsJsonAsync($"{baseUrl}/sendMessage", textPayload, ct);
            }

            using (response)
            {
                if (response.IsSuccessStatusCode)
                {
                    return new NotificationDispatchResult(true, NotificationChannel.Telegram, NotificationStatus.Sent);
                }

                var errorBody = await response.Content.ReadAsStringAsync(ct);
                var errorMessage = $"HTTP {(int)response.StatusCode} ({response.ReasonPhrase}): {errorBody}";
                _logger.LogWarning("Telegram Bot API error: {Error}", errorMessage);

                return new NotificationDispatchResult(false, NotificationChannel.Telegram, NotificationStatus.Failed, errorMessage);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Excepción al invocar Telegram Bot API: {Message}", ex.Message);
            return new NotificationDispatchResult(false, NotificationChannel.Telegram, NotificationStatus.Failed, ex.Message);
        }
    }

    private static string FormatHtmlMessage(CommunityNotificationMessage message)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"<b>{EscapeHtml(message.Title)}</b>");
        sb.AppendLine();
        sb.AppendLine(EscapeHtml(message.Description));

        if (message.Fields != null && message.Fields.Count > 0)
        {
            sb.AppendLine();
            foreach (var kv in message.Fields)
            {
                sb.AppendLine($"• <b>{EscapeHtml(kv.Key)}:</b> {EscapeHtml(kv.Value)}");
            }
        }

        if (!string.IsNullOrWhiteSpace(message.TargetUrl))
        {
            sb.AppendLine();
            sb.AppendLine($"🔗 <a href=\"{message.TargetUrl}\">Abrir en Ludeka</a>");
        }

        return sb.ToString();
    }

    private static string EscapeHtml(string text)
    {
        if (string.IsNullOrEmpty(text)) return string.Empty;

        return text
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;");
    }

    private class TelegramTextPayloadDto
    {
        [JsonPropertyName("chat_id")]
        public string ChatId { get; set; } = string.Empty;

        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;

        [JsonPropertyName("parse_mode")]
        public string ParseMode { get; set; } = "HTML";

        [JsonPropertyName("disable_web_page_preview")]
        public bool DisableWebPagePreview { get; set; }
    }

    private class TelegramPhotoPayloadDto
    {
        [JsonPropertyName("chat_id")]
        public string ChatId { get; set; } = string.Empty;

        [JsonPropertyName("photo")]
        public string PhotoUrl { get; set; } = string.Empty;

        [JsonPropertyName("caption")]
        public string Caption { get; set; } = string.Empty;

        [JsonPropertyName("parse_mode")]
        public string ParseMode { get; set; } = "HTML";
    }
}
