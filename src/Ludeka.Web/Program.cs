using Ludeka.Application.Contracts;
using Ludeka.Application.Features.Bgg;
using Ludeka.Application.Features.Catalog;
using Ludeka.Application.Features.Founding;
using Ludeka.Application.Features.Library;
using Ludeka.Application.Features.Media;
using Ludeka.Infrastructure.Bgg;
using Ludeka.Infrastructure.Data;
using Ludeka.Infrastructure.Seeding;
using Ludeka.Infrastructure.Services;
using Ludeka.Web.Components;
using Microsoft.EntityFrameworkCore;

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
builder.Services.AddScoped<ICatalogService, CatalogService>();
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

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
