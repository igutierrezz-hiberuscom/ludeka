using System;
using Ludeka.Core.Enums;

namespace Ludeka.Application.DTOs;

/// <summary>
/// Comando para reportar un problema o errata en la ficha de un juego.
/// </summary>
public record CreateGameReportCommand(
    Guid GameId,
    string GameSlug,
    string GameTitle,
    GameIssueType IssueType,
    string? Details = null,
    string? ReporterNameOrAlias = null,
    string? UserId = null
);

/// <summary>
/// Comando para que un moderador cambie el estado de un reporte.
/// </summary>
public record UpdateGameReportStatusCommand(
    GameReportStatus NewStatus,
    string ModeratorId,
    string? Notes = null
);

/// <summary>
/// Filtro para consultar y paginar reportes en la bandeja de moderación.
/// </summary>
public record GameReportFilter(
    GameReportStatus? Status = null,
    GameIssueType? IssueType = null,
    Guid? GameId = null,
    string? SearchTerm = null,
    int Page = 1,
    int PageSize = 50
);

/// <summary>
/// DTO de presentación de un reporte de incidencia de catálogo.
/// </summary>
public record GameIssueReportDto(
    Guid Id,
    Guid GameId,
    string GameSlug,
    string GameTitle,
    GameIssueType IssueType,
    string IssueTypeName,
    string? Details,
    string? ReportedByUserId,
    string ReporterNameOrAlias,
    GameReportStatus Status,
    string StatusName,
    string? ModeratorNotes,
    string? ResolvedByUserId,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    DateTimeOffset? ResolvedAt
);

/// <summary>
/// Resumen numérico para las tarjetas de estadísticas en cabecera de moderación.
/// </summary>
public record GameIssueReportSummaryDto(
    int TotalCount,
    int PendingCount,
    int InReviewCount,
    int ResolvedCount,
    int DismissedCount
);
