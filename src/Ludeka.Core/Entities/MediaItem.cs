using System;
using Ludeka.Core.Enums;

namespace Ludeka.Core.Entities;

public class MediaItem
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid? GameId { get; private set; }
    public MediaType Type { get; private set; }
    public MediaPlatform Platform { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Url { get; private set; } = string.Empty;
    public string? EmbedUrl { get; private set; }
    public string ThumbnailUrl { get; private set; } = string.Empty;
    public string AuthorChannel { get; private set; } = string.Empty;
    public int? DurationSeconds { get; private set; }
    public string? PlayerCountBadge { get; private set; }
    public int? LikesCount { get; private set; }
    public string? Excerpt { get; private set; }
    public ModerationStatus Status { get; private set; } = ModerationStatus.PendingApproval;
    public bool IsBroken { get; private set; }
    public DateTimeOffset PublishedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;

    // Relación de navegación opcional para EF Core
    public virtual Game? Game { get; private set; }

    public bool IsOrphan => !GameId.HasValue;

    public string FormattedDuration => FormatDuration(DurationSeconds);

    // Constructor sin parámetros para EF Core
    private MediaItem() { }

    public MediaItem(
        MediaType type,
        MediaPlatform platform,
        string title,
        string url,
        string thumbnailUrl,
        string authorChannel,
        Guid? gameId = null,
        string? embedUrl = null,
        int? durationSeconds = null,
        string? playerCountBadge = null,
        int? likesCount = null,
        string? excerpt = null,
        ModerationStatus status = ModerationStatus.PendingApproval,
        DateTimeOffset? publishedAt = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("El título no puede estar vacío.", nameof(title));

        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("La URL no puede estar vacía.", nameof(url));

        if (!Uri.TryCreate(url, UriKind.Absolute, out _))
            throw new ArgumentException("La URL provista no tiene un formato válido.", nameof(url));

        if (string.IsNullOrWhiteSpace(thumbnailUrl))
            throw new ArgumentException("La URL de la miniatura no puede estar vacía.", nameof(thumbnailUrl));

        if (type == MediaType.Playthrough && string.IsNullOrWhiteSpace(playerCountBadge))
            throw new ArgumentException("El distintivo de número de jugadores (PlayerCountBadge) es obligatorio para partidas completas.", nameof(playerCountBadge));

        if (durationSeconds.HasValue && durationSeconds.Value < 0)
            throw new ArgumentOutOfRangeException(nameof(durationSeconds), "La duración en segundos no puede ser negativa.");

        if (likesCount.HasValue && likesCount.Value < 0)
            throw new ArgumentOutOfRangeException(nameof(likesCount), "El contador de likes no puede ser negativo.");

        Type = type;
        Platform = platform;
        Title = title.Trim();
        Url = url.Trim();
        ThumbnailUrl = thumbnailUrl.Trim();
        AuthorChannel = authorChannel?.Trim() ?? string.Empty;
        GameId = gameId;
        DurationSeconds = durationSeconds;
        PlayerCountBadge = string.IsNullOrWhiteSpace(playerCountBadge) ? null : playerCountBadge.Trim();
        LikesCount = likesCount;
        Excerpt = string.IsNullOrWhiteSpace(excerpt) ? null : excerpt.Trim();
        Status = status;
        PublishedAt = publishedAt ?? DateTimeOffset.UtcNow;

        EmbedUrl = !string.IsNullOrWhiteSpace(embedUrl)
            ? embedUrl.Trim()
            : GenerateDefaultEmbedUrl(platform, Url);

        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Approve()
    {
        Status = ModerationStatus.Approved;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Reject()
    {
        Status = ModerationStatus.Rejected;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void AssignToGame(Guid gameId)
    {
        if (gameId == Guid.Empty)
            throw new ArgumentException("El identificador del juego no puede estar vacío.", nameof(gameId));

        GameId = gameId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UnassignGame()
    {
        GameId = null;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkAsBroken(bool isBroken)
    {
        IsBroken = isBroken;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public static string FormatDuration(int? seconds)
    {
        if (!seconds.HasValue || seconds.Value <= 0) return string.Empty;

        var ts = TimeSpan.FromSeconds(seconds.Value);
        return ts.TotalHours >= 1
            ? $"{(int)ts.TotalHours}:{ts.Minutes:D2}:{ts.Seconds:D2}"
            : $"{ts.Minutes:D2}:{ts.Seconds:D2}";
    }

    private static string? GenerateDefaultEmbedUrl(MediaPlatform platform, string url)
    {
        if (platform != MediaPlatform.YouTube) return null;

        try
        {
            var uri = new Uri(url);
            // Formato estándar: youtube.com/watch?v=VIDEO_ID
            if (uri.Host.Contains("youtube.com", StringComparison.OrdinalIgnoreCase))
            {
                var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
                var v = query["v"];
                if (!string.IsNullOrWhiteSpace(v))
                {
                    return $"https://www.youtube-nocookie.com/embed/{v}";
                }

                // Formato shorts: youtube.com/shorts/VIDEO_ID
                if (uri.AbsolutePath.StartsWith("/shorts/", StringComparison.OrdinalIgnoreCase))
                {
                    var videoId = uri.AbsolutePath.Substring("/shorts/".Length).Trim('/');
                    if (!string.IsNullOrWhiteSpace(videoId))
                    {
                        return $"https://www.youtube-nocookie.com/embed/{videoId}";
                    }
                }
            }
            // Formato corto: youtu.be/VIDEO_ID
            else if (uri.Host.Contains("youtu.be", StringComparison.OrdinalIgnoreCase))
            {
                var videoId = uri.AbsolutePath.Trim('/');
                if (!string.IsNullOrWhiteSpace(videoId))
                {
                    return $"https://www.youtube-nocookie.com/embed/{videoId}";
                }
            }
        }
        catch
        {
            // Ignorar y devolver null si no es parseable
        }

        return null;
    }
}
