using System.Collections.Generic;
using Ludeka.Core.Enums;

namespace Ludeka.Core.ValueObjects;

public record CommunityNotificationMessage(
    NotificationEventType EventType,
    string Title,
    string Description,
    string? TargetUrl = null,
    string? ImageUrl = null,
    Dictionary<string, string>? Fields = null,
    NotificationChannel? TargetChannel = null);
