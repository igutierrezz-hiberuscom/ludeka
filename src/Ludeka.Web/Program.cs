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
using Ludeka.Web.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

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

// Servicio de identidad en demo (Singleton para permitir alternancia interactiva de roles en la sesión)
builder.Services.AddSingleton<ICurrentUserService, DefaultCurrentUserService>();
builder.Services.AddScoped<IUserLibraryService, UserLibraryService>();

var app = builder.Build();

// Inicialización automática y siembra del catálogo Offline-First
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<LudekaDbContext>();
    await db.Database.EnsureCreatedAsync();
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

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
