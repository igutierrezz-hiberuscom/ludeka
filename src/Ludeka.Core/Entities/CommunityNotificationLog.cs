using System;
using Ludeka.Core.Enums;

namespace Ludeka.Core.Entities;

public class CommunityNotificationLog
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public NotificationEventType EventType { get; private set; }
    public NotificationChannel Channel { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Summary { get; private set; } = string.Empty;
    public string? TargetUrl { get; private set; }
    public string? ImageUrl { get; private set; }
    public NotificationStatus Status { get; private set; }
    public string? ErrorDetails { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? SentAt { get; private set; }

    // Constructor privado para EF Core
    private CommunityNotificationLog() { }

    public CommunityNotificationLog(
        NotificationEventType eventType,
        NotificationChannel channel,
        string title,
        string summary,
        string? targetUrl = null,
        string? imageUrl = null,
        NotificationStatus initialStatus = NotificationStatus.Queued)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("El título de la notificación no puede estar vacío.", nameof(title));

        if (string.IsNullOrWhiteSpace(summary))
            throw new ArgumentException("El resumen de la notificación no puede estar vacío.", nameof(summary));

        EventType = eventType;
        Channel = channel;
        Title = title.Trim();
        Summary = summary.Trim();
        TargetUrl = string.IsNullOrWhiteSpace(targetUrl) ? null : targetUrl.Trim();
        ImageUrl = string.IsNullOrWhiteSpace(imageUrl) ? null : imageUrl.Trim();
        Status = initialStatus;
        CreatedAt = DateTimeOffset.UtcNow;

        if (initialStatus == NotificationStatus.Sent || initialStatus == NotificationStatus.DryRun)
        {
            SentAt = DateTimeOffset.UtcNow;
        }
    }

    public void MarkAsSent()
    {
        Status = NotificationStatus.Sent;
        SentAt = DateTimeOffset.UtcNow;
        ErrorDetails = null;
    }

    public void MarkAsFailed(string error)
    {
        if (string.IsNullOrWhiteSpace(error))
            error = "Error no especificado al emitir la notificación.";

        Status = NotificationStatus.Failed;
        ErrorDetails = error.Trim();
    }

    public void MarkAsDryRun()
    {
        Status = NotificationStatus.DryRun;
        SentAt = DateTimeOffset.UtcNow;
        ErrorDetails = null;
    }
}
