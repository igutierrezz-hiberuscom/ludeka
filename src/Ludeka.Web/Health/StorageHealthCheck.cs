using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Ludeka.Web.Health;

public class StorageHealthCheck : IHealthCheck
{
    private readonly IConfiguration _configuration;

    public StorageHealthCheck(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection") ?? "Data Source=ludeka.db";
            var match = Regex.Match(connectionString, @"Data Source=([^;]+)", RegexOptions.IgnoreCase);
            var rawPath = match.Success ? match.Groups[1].Value.Trim() : "ludeka.db";

            var targetDirectory = Path.GetDirectoryName(rawPath);
            if (string.IsNullOrWhiteSpace(targetDirectory))
            {
                targetDirectory = AppContext.BaseDirectory;
            }

            if (!Directory.Exists(targetDirectory))
            {
                Directory.CreateDirectory(targetDirectory);
            }

            // Probar escritura y lectura temporal para certificar permisos de volumen Docker
            var probeFile = Path.Combine(targetDirectory, $".healthcheck_probe_{Guid.NewGuid():N}.tmp");
            File.WriteAllText(probeFile, "ludeka-health-probe");
            var content = File.ReadAllText(probeFile);
            File.Delete(probeFile);

            if (content != "ludeka-health-probe")
            {
                return Task.FromResult(HealthCheckResult.Unhealthy("Error de verificación en la sonda de almacenamiento: contenido inconsistente."));
            }

            var data = new Dictionary<string, object>
            {
                { "directory", targetDirectory },
                { "writable", true }
            };

            return Task.FromResult(HealthCheckResult.Healthy("Almacenamiento accesible con permisos de lectura y escritura.", data));
        }
        catch (Exception ex)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy("Error de permisos o acceso al sistema de archivos de datos.", ex));
        }
    }
}
