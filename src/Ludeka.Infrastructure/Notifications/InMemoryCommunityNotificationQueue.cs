using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Ludeka.Application.Contracts;
using Ludeka.Core.ValueObjects;

namespace Ludeka.Infrastructure.Notifications;

public class InMemoryCommunityNotificationQueue : ICommunityNotificationQueue
{
    private readonly Channel<CommunityNotificationMessage> _channel;

    public InMemoryCommunityNotificationQueue(int capacity = 1000)
    {
        var options = new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false
        };

        _channel = Channel.CreateBounded<CommunityNotificationMessage>(options);
    }

    public ValueTask EnqueueAsync(CommunityNotificationMessage message, CancellationToken ct = default)
    {
        return _channel.Writer.WriteAsync(message, ct);
    }

    public IAsyncEnumerable<CommunityNotificationMessage> ReadAllAsync(CancellationToken ct = default)
    {
        return _channel.Reader.ReadAllAsync(ct);
    }
}
