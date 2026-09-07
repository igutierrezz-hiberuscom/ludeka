using System.Threading;
using System.Threading.Tasks;
using Ludeka.Application.DTOs;
using Ludeka.Core.ValueObjects;

namespace Ludeka.Application.Contracts;

public interface ITelegramBotClient
{
    Task<NotificationDispatchResult> SendAsync(CommunityNotificationMessage message, CancellationToken ct = default);
}
