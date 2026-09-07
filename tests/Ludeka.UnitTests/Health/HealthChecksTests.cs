using Ludeka.Application.Contracts;
using Ludeka.Infrastructure.Data;
using Ludeka.Infrastructure.Notifications;
using Ludeka.Web.Health;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Ludeka.UnitTests.Health;

public class HealthChecksTests
{
    [Fact]
    public async Task SqliteDatabaseHealthCheck_ConBaseDeDatosOperativa_DebeRetornarHealthy()
    {
        // Arrange
        using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<LudekaDbContext>()
            .UseSqlite(connection)
            .Options;

        using var dbContext = new LudekaDbContext(options);
        await dbContext.Database.EnsureCreatedAsync();

        var healthCheck = new SqliteDatabaseHealthCheck(dbContext);
        var context = new HealthCheckContext();

        // Act
        var result = await healthCheck.CheckHealthAsync(context);

        // Assert
        Assert.Equal(HealthStatus.Healthy, result.Status);
        Assert.Contains("operativa", result.Description, StringComparison.OrdinalIgnoreCase);
        Assert.True((bool)result.Data["can_connect"]);
    }

    [Fact]
    public async Task SqliteDatabaseHealthCheck_ConConexionCerrada_DebeRetornarUnhealthy()
    {
        // Arrange
        var connection = new SqliteConnection("Data Source=:memory:");
        var options = new DbContextOptionsBuilder<LudekaDbContext>()
            .UseSqlite(connection)
            .Options;

        var dbContext = new LudekaDbContext(options);
        // Desechar explícitamente para forzar fallo de conexión
        dbContext.Dispose();

        var healthCheck = new SqliteDatabaseHealthCheck(dbContext);
        var context = new HealthCheckContext();

        // Act
        var result = await healthCheck.CheckHealthAsync(context);

        // Assert
        Assert.Equal(HealthStatus.Unhealthy, result.Status);
        Assert.NotNull(result.Exception);
    }

    [Fact]
    public async Task StorageHealthCheck_ConDirectorioAccesible_DebeRetornarHealthy()
    {
        // Arrange
        var tempDbPath = Path.Combine(Path.GetTempPath(), "ludeka_test_storage", "test.db");
        var inMemorySettings = new Dictionary<string, string?>
        {
            { "ConnectionStrings:DefaultConnection", $"Data Source={tempDbPath}" }
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var storageCheck = new StorageHealthCheck(configuration);
        var context = new HealthCheckContext();

        // Act
        var result = await storageCheck.CheckHealthAsync(context);

        // Assert
        Assert.Equal(HealthStatus.Healthy, result.Status);
        Assert.True((bool)result.Data["writable"]);

        // Cleanup
        var dir = Path.GetDirectoryName(tempDbPath);
        if (dir != null && Directory.Exists(dir))
        {
            Directory.Delete(dir, true);
        }
    }

    [Fact]
    public async Task NotificationQueueHealthCheck_ConColaOperativa_DebeRetornarHealthy()
    {
        // Arrange
        var queue = new InMemoryCommunityNotificationQueue();
        var healthCheck = new NotificationQueueHealthCheck(queue);
        var context = new HealthCheckContext();

        // Act
        var result = await healthCheck.CheckHealthAsync(context);

        // Assert
        Assert.Equal(HealthStatus.Healthy, result.Status);
        Assert.True((bool)result.Data["operational"]);
        Assert.Equal("InMemoryCommunityNotificationQueue", result.Data["queue_type"]);
    }

    [Fact]
    public async Task NotificationQueueHealthCheck_ConColaNula_DebeRetornarUnhealthy()
    {
        // Arrange
        var healthCheck = new NotificationQueueHealthCheck(null!);
        var context = new HealthCheckContext();

        // Act
        var result = await healthCheck.CheckHealthAsync(context);

        // Assert
        Assert.Equal(HealthStatus.Unhealthy, result.Status);
    }

    [Fact]
    public void DependencyInjection_DebeRegistrarHealthChecks_ConEtiquetasReady()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<LudekaDbContext>(options => options.UseSqlite("Data Source=:memory:"));
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
        services.AddSingleton<ICommunityNotificationQueue, InMemoryCommunityNotificationQueue>();

        services.AddHealthChecks()
            .AddCheck<SqliteDatabaseHealthCheck>("sqlite_db", tags: ["ready"])
            .AddCheck<StorageHealthCheck>("storage", tags: ["ready"])
            .AddCheck<NotificationQueueHealthCheck>("notification_queue", tags: ["ready"]);

        var provider = services.BuildServiceProvider();

        // Act
        var healthCheckService = provider.GetService<HealthCheckService>();

        // Assert
        Assert.NotNull(healthCheckService);
    }
}
