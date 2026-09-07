using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ludeka.Application.DTOs;
using Ludeka.Core.Enums;
using Ludeka.Core.ValueObjects;

namespace Ludeka.Application.Contracts;

public interface ICommunityNotificationService
{
    Task<IReadOnlyList<NotificationDispatchResult>> BroadcastAsync(CommunityNotificationMessage message, CancellationToken ct = default);
    Task<NotificationDispatchResult> SendToDiscordAsync(CommunityNotificationMessage message, CancellationToken ct = default);
    Task<NotificationDispatchResult> SendToTelegramAsync(CommunityNotificationMessage message, CancellationToken ct = default);
    Task<IReadOnlyList<CommunityNotificationLogDto>> GetHistoryAsync(int limit = 50, CancellationToken ct = default);
    Task<IReadOnlyList<NotificationChannelStatusDto>> GetChannelStatusesAsync(CancellationToken ct = default);
    Task<NotificationDispatchResult> RetryFailedNotificationAsync(Guid logId, CancellationToken ct = default);
    Task TriggerExpiringGiveawaysScanAsync(CancellationToken ct = default);
    Task TriggerFridayReleasesBulletinAsync(CancellationToken ct = default);
    Task SendTestPingAsync(NotificationChannel? channel = null, CancellationToken ct = default);
}
