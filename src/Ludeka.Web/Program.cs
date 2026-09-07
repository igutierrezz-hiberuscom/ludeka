using Ludeka.Application.Contracts;
using Ludeka.Application.Features.Bgg;
using Ludeka.Application.Features.Catalog;
using Ludeka.Application.Features.Community;
using Ludeka.Application.Features.Expansions;
using Ludeka.Application.Features.Founding;
using Ludeka.Application.Features.Library;
using Ludeka.Application.Features.Media;
using Ludeka.Infrastructure.Bgg;
using Ludeka.Infrastructure.Data;
using Ludeka.Infrastructure.Repositories;
using Ludeka.Infrastructure.Seeding;
using Ludeka.Infrastructure.Services;
using Ludeka.Infrastructure.Notifications;
using Ludeka.Application.Options;
using Ludeka.Web.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using System.Text.Json;
using System.Text.RegularExpressions;
using Ludeka.Web.Health;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Configuración de persistencia SQLite y Clean Architecture
builder.Services.AddDbContext<LudekaDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=ludeka.db");
});

builder.Services.AddScoped<IGameRepository, SqliteGameRepository>();

// Caché Nivel 1 (Aplicación en Memoria) y Decorador del Catálogo
builder.Services.AddMemoryCache();
builder.Services.AddScoped<CatalogService>();
builder.Services.AddScoped<ICatalogService>(sp =>
    new CachedCatalogService(
        sp.GetRequiredService<CatalogService>(),
        sp.GetRequiredService<IMemoryCache>()));

// Caché Nivel 2 (HTTP / Output Caching con Tags de Invalidación)
builder.Services.AddOutputCache(options =>
{
    options.AddPolicy("CatalogCache", policy =>
        policy.Expire(TimeSpan.FromMinutes(10))
              .Tag("tag-catalog"));

    options.AddPolicy("RadarCache", policy =>
        policy.Expire(TimeSpan.FromMinutes(5))
              .Tag("tag-radar"));

    options.AddPolicy("StaticPages", policy =>
        policy.Expire(TimeSpan.FromMinutes(60))
              .Tag("tag-static"));
});

builder.Services.Configure<BggOptions>(builder.Configuration.GetSection(BggOptions.SectionName));
builder.Services.AddHttpClient<IBggClient, BggXmlApiClient>();

builder.Services.AddScoped<IUserCollectionRepository, SqliteUserCollectionRepository>();
builder.Services.AddScoped<IGameLoanRepository, SqliteGameLoanRepository>();
builder.Services.AddScoped<IUserReviewRepository, SqliteUserReviewRepository>();
builder.Services.AddScoped<IFoundingVerdictRepository, SqliteFoundingVerdictRepository>();
builder.Services.AddScoped<IFoundingVerdictService, FoundingVerdictService>();

builder.Services.AddScoped<IMediaRepository, SqliteMediaRepository>();
builder.Services.AddHttpClient<IBrokenLinkCheckerService, BrokenLinkCheckerService>();
builder.Services.AddScoped<IMediaService, MediaService>();

builder.Services.AddScoped<IPendingBggImportRepository, SqlitePendingBggImportRepository>();
builder.Services.AddScoped<IBggImportService, BggImportService>();
builder.Services.AddScoped<IBggCatalogQueueService, BggCatalogQueueService>();
builder.Services.AddScoped<IBggSearchAssistedService, BggSearchAssistedService>();

// Incremento 6: Sorteos, Novedades del Viernes, Q&A de Reglas y Tarjetas Sociales
builder.Services.AddScoped<IGiveawayRepository, SqliteGiveawayRepository>();
builder.Services.AddScoped<IGiveawayService, GiveawayService>();
builder.Services.AddScoped<IWeeklyReleaseRepository, SqliteWeeklyReleaseRepository>();
builder.Services.AddScoped<IWeeklyReleaseService, WeeklyReleaseService>();
builder.Services.AddScoped<IRuleQARepository, SqliteRuleQARepository>();
builder.Services.AddScoped<IRuleQAService, RuleQAService>();
builder.Services.AddScoped<ISocialCardService, SocialCardService>();

// Incremento 8: Expansiones, Sinergias y Mezclador de Mesa
builder.Services.AddScoped<IExpansionRepository, SqliteExpansionRepository>();
builder.Services.AddScoped<IExpansionService, ExpansionService>();

// Incremento 9: Notificaciones y Webhooks de Comunidad (Discord y Telegram)
builder.Services.Configure<CommunityNotificationOptions>(builder.Configuration.GetSection(CommunityNotificationOptions.SectionName));
builder.Services.AddHttpClient<IDiscordWebhookClient, DiscordWebhookClient>();
builder.Services.AddHttpClient<ITelegramBotClient, TelegramBotClient>();
builder.Services.AddSingleton<ICommunityNotificationQueue, InMemoryCommunityNotificationQueue>();
builder.Services.AddScoped<ICommunityNotificationRepository, SqliteCommunityNotificationRepository>();
builder.Services.AddScoped<ICommunityNotificationService, CommunityNotificationService>();
builder.Services.AddHostedService<CommunityNotificationDispatcherHostedService>();

// Servicio de identidad en demo (Singleton para permitir alternancia interactiva de roles en la sesión)
builder.Services.AddSingleton<ICurrentUserService, DefaultCurrentUserService>();
builder.Services.AddScoped<IUserLibraryService, UserLibraryService>();

// Incremento 10: Observabilidad con Health Checks Oficiales de ASP.NET Core
builder.Services.AddHealthChecks()
    .AddCheck<SqliteDatabaseHealthCheck>("sqlite_db", tags: ["ready"])
    .AddCheck<StorageHealthCheck>("storage", tags: ["ready"])
    .AddCheck<NotificationQueueHealthCheck>("notification_queue", tags: ["ready"]);

var app = builder.Build();

// Inicialización automática y siembra del catálogo Offline-First con resiliencia de directorios en Docker
using (var scope = app.Services.CreateScope())
{
    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    var connectionString = config.GetConnectionString("DefaultConnection") ?? "Data Source=ludeka.db";
    var match = Regex.Match(connectionString, @"Data Source=([^;]+)", RegexOptions.IgnoreCase);
    if (match.Success)
    {
        var rawPath = match.Groups[1].Value.Trim();
        var dir = Path.GetDirectoryName(rawPath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
    }

    var db = scope.ServiceProvider.GetRequiredService<LudekaDbContext>();
    await db.Database.EnsureCreatedAsync();
    await SqliteSchemaMigrator.EnsureSchemaUpToDateAsync(db);
    await CatalogSeeder.SeedAsync(db);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseAntiforgery();
app.UseOutputCache();

// Incremento 10: Endpoints de Diagnóstico y Salud
// Liveness probe (/healthz): confirma que el host está activo sin penalizar dependencias
app.MapHealthChecks("/healthz", new HealthCheckOptions
{
    Predicate = _ => false,
    ResponseWriter = async (context, _) =>
    {
        context.Response.ContentType = "application/json";
        var payload = new
        {
            status = "Healthy",
            timestamp = DateTimeOffset.UtcNow,
            mode = "liveness"
        };
        await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }
});

// Readiness probe (/ready): evalúa dependencias críticas (base de datos, almacenamiento y cola)
app.MapHealthChecks("/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var payload = new
        {
            status = report.Status.ToString(),
            totalDurationMs = report.TotalDuration.TotalMilliseconds,
            timestamp = DateTimeOffset.UtcNow,
            mode = "readiness",
            entries = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                durationMs = e.Value.Duration.TotalMilliseconds,
                data = e.Value.Data
            })
        };
        await context.Response.WriteAsync(JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true }));
    }
});

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
