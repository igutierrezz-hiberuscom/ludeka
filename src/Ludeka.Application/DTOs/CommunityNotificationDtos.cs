using System;
using Ludeka.Core.Enums;

namespace Ludeka.Application.DTOs;

public record CommunityNotificationLogDto(
    Guid Id,
    NotificationEventType EventType,
    NotificationChannel Channel,
    string Title,
    string Summary,
    string? TargetUrl,
    string? ImageUrl,
    NotificationStatus Status,
    string? ErrorDetails,
    DateTimeOffset CreatedAt,
    DateTimeOffset? SentAt);

public record NotificationDispatchResult(
    bool Success,
    NotificationChannel Channel,
    NotificationStatus Status,
    string? ErrorMessage = null);

public record NotificationChannelStatusDto(
    NotificationChannel Channel,
    bool Enabled,
    bool Configured,
    bool DryRun,
    string DestinationPreview);
