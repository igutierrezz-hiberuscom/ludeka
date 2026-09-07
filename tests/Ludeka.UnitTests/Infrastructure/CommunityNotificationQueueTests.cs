using System.Threading;
using System.Threading.Tasks;
using Ludeka.Core.Enums;
using Ludeka.Core.ValueObjects;
using Ludeka.Infrastructure.Notifications;
using Xunit;

namespace Ludeka.UnitTests.Infrastructure;

public class CommunityNotificationQueueTests
{
    [Fact]
    public async Task EnqueueAndReadAll_ProcessesMessagesInFIFOOrder()
    {
        var queue = new InMemoryCommunityNotificationQueue(capacity: 10);
        using var cts = new CancellationTokenSource();

        var msg1 = new CommunityNotificationMessage(NotificationEventType.GiveawayExpiring, "Mensaje 1", "Desc 1");
        var msg2 = new CommunityNotificationMessage(NotificationEventType.FridayReleasesSummary, "Mensaje 2", "Desc 2");

        await queue.EnqueueAsync(msg1);
        await queue.EnqueueAsync(msg2);

        var received = new System.Collections.Generic.List<CommunityNotificationMessage>();

        var readerTask = Task.Run(async () =>
        {
            await foreach (var message in queue.ReadAllAsync(cts.Token))
            {
                received.Add(message);
                if (received.Count == 2)
                {
                    cts.Cancel();
                    break;
                }
            }
        });

        await Task.WhenAny(readerTask, Task.Delay(2000));

        Assert.Equal(2, received.Count);
        Assert.Equal("Mensaje 1", received[0].Title);
        Assert.Equal("Mensaje 2", received[1].Title);
    }
}
