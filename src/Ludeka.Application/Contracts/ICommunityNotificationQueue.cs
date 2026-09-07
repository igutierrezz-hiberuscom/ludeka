using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ludeka.Core.ValueObjects;

namespace Ludeka.Application.Contracts;

public interface ICommunityNotificationQueue
{
    ValueTask EnqueueAsync(CommunityNotificationMessage message, CancellationToken ct = default);
    IAsyncEnumerable<CommunityNotificationMessage> ReadAllAsync(CancellationToken ct = default);
}
