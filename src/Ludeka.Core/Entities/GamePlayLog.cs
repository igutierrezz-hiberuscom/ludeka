using System;

namespace Ludeka.Core.Entities;

/// <summary>
/// Registra una sesión de partida jugada por un usuario para un juego determinado.
/// </summary>
public class GamePlayLog
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string UserId { get; private set; } = string.Empty;
    public Guid GameId { get; private set; }
    public DateTimeOffset PlayDate { get; private set; }
    public string Location { get; private set; } = string.Empty;
    public int PlayerCount { get; private set; }
    public int? DurationMinutes { get; private set; }
    public string? Comment { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;

    // Propiedad de navegación opcional para EF Core
    public Game? Game { get; private set; }

    private GamePlayLog() { }

    public GamePlayLog(
        string userId,
        Guid gameId,
        DateTimeOffset playDate,
        string location,
        int playerCount,
        int? durationMinutes = null,
        string? comment = null)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("El identificador de usuario no puede estar vacío.", nameof(userId));
        if (gameId == Guid.Empty)
            throw new ArgumentException("El identificador del juego no puede ser un GUID vacío.", nameof(gameId));
        if (playerCount < 1)
            throw new ArgumentException("El número de jugadores debe ser al menos 1.", nameof(playerCount));

        UserId = userId.Trim();
        GameId = gameId;
        PlayDate = playDate;
        Location = string.IsNullOrWhiteSpace(location) ? "En casa" : location.Trim();
        PlayerCount = playerCount;
        DurationMinutes = durationMinutes;
        Comment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim();
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public void Update(DateTimeOffset playDate, string location, int playerCount, int? durationMinutes, string? comment)
    {
        if (playerCount < 1)
            throw new ArgumentException("El número de jugadores debe ser al menos 1.", nameof(playerCount));

        PlayDate = playDate;
        Location = string.IsNullOrWhiteSpace(location) ? "En casa" : location.Trim();
        PlayerCount = playerCount;
        DurationMinutes = durationMinutes;
        Comment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim();
    }
}
