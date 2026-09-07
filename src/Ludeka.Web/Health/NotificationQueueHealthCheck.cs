using Ludeka.Application.Contracts;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Ludeka.Web.Health;

public class NotificationQueueHealthCheck : IHealthCheck
{
    private readonly ICommunityNotificationQueue _queue;

    public NotificationQueueHealthCheck(ICommunityNotificationQueue queue)
    {
        _queue = queue;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            if (_queue == null)
            {
                return Task.FromResult(HealthCheckResult.Unhealthy("El servicio ICommunityNotificationQueue no está disponible."));
            }

            var data = new Dictionary<string, object>
            {
                { "queue_type", _queue.GetType().Name },
                { "operational", true }
            };

            return Task.FromResult(HealthCheckResult.Healthy("Cola de notificaciones comunitarias operativa.", data));
        }
        catch (Exception ex)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy("Error en la cola de notificaciones comunitarias.", ex));
        }
    }
}
