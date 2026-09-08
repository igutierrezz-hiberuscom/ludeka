using System;

namespace Ludeka.Core.Entities;

/// <summary>
/// Registra la auditoría editorial de las modificaciones realizadas en una ficha de juego.
/// Permite trazabilidad completa de quién modificó qué ficha, en qué fecha y qué reporte comunitario se subsanó.
/// </summary>
public class GameEditLog
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid GameId { get; private set; }
    public string EditorUserId { get; private set; } = string.Empty;
    public string EditorName { get; private set; } = string.Empty;
    public string SummaryOfChanges { get; private set; } = string.Empty;
    public Guid? AssociatedReportId { get; private set; }
    public DateTimeOffset EditedAt { get; private set; } = DateTimeOffset.UtcNow;

    // Constructor privado para EF Core
    private GameEditLog() { }

    public GameEditLog(
        Guid gameId,
        string editorUserId,
        string editorName,
        string summaryOfChanges,
        Guid? associatedReportId = null)
    {
        if (gameId == Guid.Empty) throw new ArgumentException("El GameId no puede estar vacío.", nameof(gameId));
        if (string.IsNullOrWhiteSpace(editorUserId)) throw new ArgumentException("El EditorUserId no puede estar vacío.", nameof(editorUserId));

        GameId = gameId;
        EditorUserId = editorUserId.Trim();
        EditorName = string.IsNullOrWhiteSpace(editorName) ? EditorUserId : editorName.Trim();
        SummaryOfChanges = string.IsNullOrWhiteSpace(summaryOfChanges) ? "Edición editorial de ficha" : summaryOfChanges.Trim();
        AssociatedReportId = associatedReportId;
        EditedAt = DateTimeOffset.UtcNow;
    }
}
