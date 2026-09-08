using System;
using Ludeka.Core.Enums;

namespace Ludeka.Core.Entities;

/// <summary>
/// Entidad de dominio que representa un reporte de incidencia o errata en una ficha de catálogo.
/// </summary>
public class GameIssueReport
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid GameId { get; private set; }
    public string GameSlug { get; private set; } = string.Empty;
    public string GameTitle { get; private set; } = string.Empty;
    public GameIssueType IssueType { get; private set; }
    public string? Details { get; private set; }
    public string? ReportedByUserId { get; private set; }
    public string ReporterNameOrAlias { get; private set; } = "Comunidad anónima";
    public GameReportStatus Status { get; private set; } = GameReportStatus.Pending;
    public string? ModeratorNotes { get; private set; }
    public string? ResolvedByUserId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; private set; }
    public DateTimeOffset? ResolvedAt { get; private set; }

    // Requerido por EF Core
    private GameIssueReport() { }

    public GameIssueReport(
        Guid gameId,
        string gameSlug,
        string gameTitle,
        GameIssueType issueType,
        string? details = null,
        string? reporterNameOrAlias = null,
        string? reportedByUserId = null)
    {
        if (gameId == Guid.Empty)
            throw new ArgumentException("El identificador del juego no puede estar vacío.", nameof(gameId));
        if (string.IsNullOrWhiteSpace(gameSlug))
            throw new ArgumentException("El slug del juego no puede estar vacío.", nameof(gameSlug));
        if (string.IsNullOrWhiteSpace(gameTitle))
            throw new ArgumentException("El título del juego no puede estar vacío.", nameof(gameTitle));

        GameId = gameId;
        GameSlug = gameSlug.Trim();
        GameTitle = gameTitle.Trim();
        IssueType = issueType;
        Details = string.IsNullOrWhiteSpace(details) ? null : details.Trim();
        ReporterNameOrAlias = string.IsNullOrWhiteSpace(reporterNameOrAlias) ? "Comunidad anónima" : reporterNameOrAlias.Trim();
        ReportedByUserId = string.IsNullOrWhiteSpace(reportedByUserId) ? null : reportedByUserId.Trim();
        Status = GameReportStatus.Pending;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Marca el reporte como tomado en revisión activa por un moderador.
    /// </summary>
    public void MarkAsInReview(string moderatorId, string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(moderatorId))
            throw new ArgumentException("Debe especificarse el moderador que toma el reporte en revisión.", nameof(moderatorId));

        Status = GameReportStatus.InReview;
        ResolvedByUserId = moderatorId.Trim();
        if (!string.IsNullOrWhiteSpace(notes))
            ModeratorNotes = notes.Trim();
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Marca el reporte como resuelto y subsanado.
    /// </summary>
    public void Resolve(string moderatorId, string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(moderatorId))
            throw new ArgumentException("Debe especificarse el moderador que resuelve el reporte.", nameof(moderatorId));

        Status = GameReportStatus.Resolved;
        ResolvedByUserId = moderatorId.Trim();
        if (!string.IsNullOrWhiteSpace(notes))
            ModeratorNotes = notes.Trim();
        ResolvedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Descarta el reporte indicando un motivo justificativo.
    /// </summary>
    public void Dismiss(string moderatorId, string reason)
    {
        if (string.IsNullOrWhiteSpace(moderatorId))
            throw new ArgumentException("Debe especificarse el moderador que descarta el reporte.", nameof(moderatorId));
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Debe indicarse el motivo del descarte.", nameof(reason));

        Status = GameReportStatus.Dismissed;
        ResolvedByUserId = moderatorId.Trim();
        ModeratorNotes = reason.Trim();
        ResolvedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Reabre un reporte descartado o resuelto devolviéndolo a estado pendiente.
    /// </summary>
    public void Reopen()
    {
        Status = GameReportStatus.Pending;
        ResolvedAt = null;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
