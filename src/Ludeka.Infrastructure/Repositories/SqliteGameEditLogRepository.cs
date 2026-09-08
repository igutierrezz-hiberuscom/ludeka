using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ludeka.Application.Contracts;
using Ludeka.Core.Entities;
using Ludeka.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Ludeka.Infrastructure.Repositories;

public class SqliteGameEditLogRepository : IGameEditLogRepository
{
    private readonly LudekaDbContext _context;

    public SqliteGameEditLogRepository(LudekaDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task AddAsync(GameEditLog log, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(log);
        await _context.GameEditLogs.AddAsync(log, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<GameEditLog>> GetByGameIdAsync(Guid gameId, CancellationToken ct = default)
    {
        // EF Core SQLite no traduce ORDER BY sobre DateTimeOffset: se materializa primero y se ordena en memoria.
        var logs = await _context.GameEditLogs
            .AsNoTracking()
            .Where(l => l.GameId == gameId)
            .ToListAsync(ct);

        return logs
            .OrderByDescending(l => l.EditedAt)
            .ThenByDescending(l => l.Id)
            .ToList();
    }
}
