using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ludeka.Application.Contracts;
using Ludeka.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ludeka.Infrastructure.Data;

/// <summary>
/// Repositorio SQLite para la persistencia y consulta de la bitácora de ejecuciones de catalogación nocturna.
/// </summary>
public class SqliteNightlyCatalogingLogRepository : INightlyCatalogingLogRepository
{
    private readonly LudekaDbContext _context;

    public SqliteNightlyCatalogingLogRepository(LudekaDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task AddAsync(NightlyCatalogingExecutionLog log, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(log);
        await _context.NightlyCatalogingExecutionLogs.AddAsync(log, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(NightlyCatalogingExecutionLog log, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(log);
        _context.NightlyCatalogingExecutionLogs.Update(log);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<NightlyCatalogingExecutionLog>> GetRecentLogsAsync(int limit = 20, CancellationToken ct = default)
    {
        // EF Core SQLite no traduce ORDER BY sobre DateTimeOffset: se materializa primero y se ordena en memoria.
        var logs = await _context.NightlyCatalogingExecutionLogs
            .AsNoTracking()
            .ToListAsync(ct);

        return logs
            .OrderByDescending(l => l.StartedAt)
            .ThenByDescending(l => l.Id)
            .Take(limit)
            .ToList();
    }

    public async Task<NightlyCatalogingExecutionLog?> GetLatestLogAsync(CancellationToken ct = default)
    {
        // EF Core SQLite no traduce ORDER BY sobre DateTimeOffset: se materializa primero y se ordena en memoria.
        var logs = await _context.NightlyCatalogingExecutionLogs
            .AsNoTracking()
            .ToListAsync(ct);

        return logs
            .OrderByDescending(l => l.StartedAt)
            .ThenByDescending(l => l.Id)
            .FirstOrDefault();
    }
}
