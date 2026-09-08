using System.Threading.Tasks;
using Ludeka.Core.Entities;
using Ludeka.Infrastructure.Data;
using Ludeka.Infrastructure.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Ludeka.UnitTests.Application;

public class UserPreferenceServiceTests : IAsyncLifetime
{
    private SqliteConnection _connection = null!;
    private LudekaDbContext _context = null!;
    private SqliteUserPreferenceService _service = null!;

    public async Task InitializeAsync()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        await _connection.OpenAsync();

        var options = new DbContextOptionsBuilder<LudekaDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new LudekaDbContext(options);
        await _context.Database.EnsureCreatedAsync();

        _service = new SqliteUserPreferenceService(_context);
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
        await _connection.DisposeAsync();
    }

    [Fact]
    public async Task GetUserThemeAsync_ShouldReturnDefaultCharcoal_WhenUserHasNoPreference()
    {
        // Act
        var theme = await _service.GetUserThemeAsync("usuario-nuevo");

        // Assert
        Assert.Equal("charcoal", theme);
    }

    [Fact]
    public async Task GetUserThemeAsync_ShouldReturnDefaultCharcoal_WhenUserIdIsNullOrEmpty()
    {
        // Act
        var themeNull = await _service.GetUserThemeAsync(string.Empty);

        // Assert
        Assert.Equal("charcoal", themeNull);
    }

    [Fact]
    public async Task SetUserThemeAsync_ShouldCreatePreference_WhenUserDoesNotExist()
    {
        // Act
        await _service.SetUserThemeAsync("jugador-1", "wood");

        // Assert
        var theme = await _service.GetUserThemeAsync("jugador-1");
        Assert.Equal("wood", theme);

        var dto = await _service.GetUserPreferenceAsync("jugador-1");
        Assert.Equal("jugador-1", dto.UserId);
        Assert.Equal("wood", dto.PreferredTheme);
    }

    [Fact]
    public async Task SetUserThemeAsync_ShouldUpdateTheme_WhenUserAlreadyExists()
    {
        // Arrange
        await _service.SetUserThemeAsync("jugador-2", "editorial");

        // Act
        await _service.SetUserThemeAsync("jugador-2", "wood");

        // Assert
        var theme = await _service.GetUserThemeAsync("jugador-2");
        Assert.Equal("wood", theme);
    }

    [Fact]
    public async Task SetUserThemeAsync_ShouldNormalizeTheme_WhenThemeIsCaseVariantOrUnknown()
    {
        // Act
        await _service.SetUserThemeAsync("jugador-3", "WOOD");

        // Assert
        var theme = await _service.GetUserThemeAsync("jugador-3");
        Assert.Equal("wood", theme);

        // Act 2: Tema desconocido
        await _service.SetUserThemeAsync("jugador-3", "color-inexistente");
        var themeFallback = await _service.GetUserThemeAsync("jugador-3");
        Assert.Equal("charcoal", themeFallback);
    }
}
