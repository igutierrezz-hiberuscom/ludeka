using Ludeca.Application.Contracts;
using Ludeca.Application.Features.Catalog;
using Ludeca.Application.Features.Founding;
using Ludeca.Application.Features.Library;
using Ludeca.Infrastructure.Bgg;
using Ludeca.Infrastructure.Data;
using Ludeca.Infrastructure.Seeding;
using Ludeca.Infrastructure.Services;
using Ludeca.Web.Components;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Configuración de persistencia SQLite y Clean Architecture
builder.Services.AddDbContext<LudecaDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=ludeca.db");
});

builder.Services.AddScoped<IGameRepository, SqliteGameRepository>();
builder.Services.AddScoped<ICatalogService, CatalogService>();
builder.Services.AddHttpClient<IBggClient, BggXmlApiClient>();

builder.Services.AddScoped<IUserCollectionRepository, SqliteUserCollectionRepository>();
builder.Services.AddScoped<IGameLoanRepository, SqliteGameLoanRepository>();
builder.Services.AddScoped<IUserReviewRepository, SqliteUserReviewRepository>();
builder.Services.AddScoped<IFoundingVerdictRepository, SqliteFoundingVerdictRepository>();
builder.Services.AddScoped<IFoundingVerdictService, FoundingVerdictService>();

// Servicio de identidad en demo (Singleton para permitir alternancia interactiva de roles en la sesión)
builder.Services.AddSingleton<ICurrentUserService, DefaultCurrentUserService>();
builder.Services.AddScoped<IUserLibraryService, UserLibraryService>();

var app = builder.Build();

// Inicialización automática y siembra del catálogo Offline-First
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<LudecaDbContext>();
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
