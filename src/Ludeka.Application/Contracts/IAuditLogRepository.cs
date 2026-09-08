using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ludeka.Core.Entities;
using Ludeka.Core.Enums;

namespace Ludeka.Application.Contracts;

/// <summary>
/// Repositorio inmutable para almacenar y consultar bitácoras de auditoría.
/// </summary>
public interface IAuditLogRepository
{
    Task AddAsync(AuditLogEntry entry, CancellationToken ct = default);

    Task<IReadOnlyList<AuditLogEntry>> GetLogsAsync(
        string? userId = null,
        AuditEntityType? entityType = null,
        AuditAction? action = null,
        DateTimeOffset? fromDate = null,
        DateTimeOffset? toDate = null,
        int skip = 0,
        int take = 50,
        CancellationToken ct = default);

    Task<int> CountLogsAsync(
        string? userId = null,
        AuditEntityType? entityType = null,
        AuditAction? action = null,
        DateTimeOffset? fromDate = null,
        DateTimeOffset? toDate = null,
        CancellationToken ct = default);
}
