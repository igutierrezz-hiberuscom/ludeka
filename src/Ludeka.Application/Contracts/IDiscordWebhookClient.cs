using System.Threading;
using System.Threading.Tasks;
using Ludeka.Application.DTOs;
using Ludeka.Core.ValueObjects;

namespace Ludeka.Application.Contracts;

public interface IDiscordWebhookClient
{
    Task<NotificationDispatchResult> SendAsync(CommunityNotificationMessage message, CancellationToken ct = default);
}
