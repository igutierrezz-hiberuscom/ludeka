using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ludeka.Application.Contracts;
using Ludeka.Core.Entities;
using Ludeka.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace Ludeka.Infrastructure.Data;

public class SqliteAuditLogRepository : IAuditLogRepository
{
    private readonly LudekaDbContext _db;

    public SqliteAuditLogRepository(LudekaDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async Task AddAsync(AuditLogEntry entry, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(entry);
        await _db.AuditLogs.AddAsync(entry, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<AuditLogEntry>> GetLogsAsync(
        string? userId = null,
        AuditEntityType? entityType = null,
        AuditAction? action = null,
        DateTimeOffset? fromDate = null,
        DateTimeOffset? toDate = null,
        int skip = 0,
        int take = 50,
        CancellationToken ct = default)
    {
        var query = BuildQuery(userId, entityType, action, fromDate, toDate);

        return await query
            .OrderByDescending(a => a.Timestamp)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);
    }

    public async Task<int> CountLogsAsync(
        string? userId = null,
        AuditEntityType? entityType = null,
        AuditAction? action = null,
        DateTimeOffset? fromDate = null,
        DateTimeOffset? toDate = null,
        CancellationToken ct = default)
    {
        var query = BuildQuery(userId, entityType, action, fromDate, toDate);
        return await query.CountAsync(ct);
    }

    private IQueryable<AuditLogEntry> BuildQuery(
        string? userId,
        AuditEntityType? entityType,
        AuditAction? action,
        DateTimeOffset? fromDate,
        DateTimeOffset? toDate)
    {
        var query = _db.AuditLogs.AsQueryable();

        if (!string.IsNullOrWhiteSpace(userId))
        {
            var cleanUserId = userId.Trim().ToLowerInvariant();
            query = query.Where(a => a.UserId == cleanUserId);
        }

        if (entityType.HasValue)
        {
            query = query.Where(a => a.EntityType == entityType.Value);
        }

        if (action.HasValue)
        {
            query = query.Where(a => a.Action == action.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(a => a.Timestamp >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(a => a.Timestamp <= toDate.Value);
        }

        return query;
    }
}
