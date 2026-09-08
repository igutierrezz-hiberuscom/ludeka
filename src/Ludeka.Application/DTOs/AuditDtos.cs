using System;
using System.Collections.Generic;
using Ludeka.Core.Enums;

namespace Ludeka.Application.DTOs;

public record FieldChangeDto(
    string FieldName,
    string? OldValue,
    string? NewValue
);

public record AuditLogDto(
    Guid Id,
    string UserId,
    string UserName,
    DateTimeOffset Timestamp,
    AuditAction Action,
    string ActionDisplayName,
    AuditEntityType EntityType,
    string EntityTypeDisplayName,
    string EntityId,
    string EntityName,
    string Summary,
    IReadOnlyList<FieldChangeDto> Changes
);

public record AuditLogFilterDto(
    string? UserId = null,
    AuditEntityType? EntityType = null,
    AuditAction? Action = null,
    DateTimeOffset? FromDate = null,
    DateTimeOffset? ToDate = null,
    int Page = 1,
    int PageSize = 25
);

public record AuditLogPageDto(
    IReadOnlyList<AuditLogDto> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);

public record RecordAuditCommand(
    string UserId,
    string UserName,
    AuditAction Action,
    AuditEntityType EntityType,
    string EntityId,
    string EntityName,
    string Summary,
    IEnumerable<FieldChangeDto>? Changes = null
);
