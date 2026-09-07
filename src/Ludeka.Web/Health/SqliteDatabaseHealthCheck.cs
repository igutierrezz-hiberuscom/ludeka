using Ludeka.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Ludeka.Web.Health;

public class SqliteDatabaseHealthCheck : IHealthCheck
{
    private readonly LudekaDbContext _dbContext;

    public SqliteDatabaseHealthCheck(LudekaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var canConnect = await _dbContext.Database.CanConnectAsync(cancellationToken);
            if (!canConnect)
            {
                return HealthCheckResult.Unhealthy("No se puede establecer conexión con la base de datos SQLite.");
            }

            // Comprobación de consulta básica
            await _dbContext.Database.ExecuteSqlRawAsync("SELECT 1;", cancellationToken);

            var gameCount = await _dbContext.Games.CountAsync(cancellationToken);
            var data = new Dictionary<string, object>
            {
                { "can_connect", true },
                { "game_count", gameCount },
                { "provider", "Microsoft.EntityFrameworkCore.Sqlite" }
            };

            return HealthCheckResult.Healthy("Base de datos SQLite operativa y respondiendo.", data);
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Fallo al verificar el estado de la base de datos SQLite.", ex);
        }
    }
}
