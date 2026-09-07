namespace Ludeka.Application.Options;

public class CommunityNotificationOptions
{
    public const string SectionName = "CommunityNotifications";

    public bool Enabled { get; set; } = true;
    public bool DryRun { get; set; } = true;

    // Discord Webhook
    public string? DiscordWebhookUrl { get; set; }
    public bool DiscordEnabled { get; set; } = true;

    // Telegram Bot API
    public string? TelegramBotToken { get; set; }
    public string? TelegramChatId { get; set; }
    public bool TelegramEnabled { get; set; } = true;

    public bool IsDiscordConfigured => !string.IsNullOrWhiteSpace(DiscordWebhookUrl);
    public bool IsTelegramConfigured => !string.IsNullOrWhiteSpace(TelegramBotToken) && !string.IsNullOrWhiteSpace(TelegramChatId);
}
