using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ludeka.Application.Contracts;
using Ludeka.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ludeka.Infrastructure.Data;

public class SqliteGamePlayLogRepository : IGamePlayLogRepository
{
    private readonly LudekaDbContext _context;

    public SqliteGamePlayLogRepository(LudekaDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(GamePlayLog play, CancellationToken cancellationToken = default)
    {
        await _context.GamePlayLogs.AddAsync(play, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var play = await _context.GamePlayLogs.FindAsync([id], cancellationToken);
        if (play != null)
        {
            _context.GamePlayLogs.Remove(play);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<GamePlayLog?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.GamePlayLogs
            .Include(p => p.Game)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<List<GamePlayLog>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        // EF Core SQLite no traduce ORDER BY sobre DateTimeOffset: se materializa primero y se ordena en memoria.
        var logs = await _context.GamePlayLogs
            .Include(p => p.Game)
            .Where(p => p.UserId == userId)
            .ToListAsync(cancellationToken);

        return logs
            .OrderByDescending(p => p.PlayDate)
            .ThenByDescending(p => p.CreatedAt)
            .ToList();
    }

    public async Task<List<GamePlayLog>> GetByUserAndGameAsync(string userId, Guid gameId, CancellationToken cancellationToken = default)
    {
        // EF Core SQLite no traduce ORDER BY sobre DateTimeOffset: se materializa primero y se ordena en memoria.
        var logs = await _context.GamePlayLogs
            .Include(p => p.Game)
            .Where(p => p.UserId == userId && p.GameId == gameId)
            .ToListAsync(cancellationToken);

        return logs
            .OrderByDescending(p => p.PlayDate)
            .ThenByDescending(p => p.CreatedAt)
            .ToList();
    }

    public async Task<int> GetCountByUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _context.GamePlayLogs
            .CountAsync(p => p.UserId == userId, cancellationToken);
    }

    public async Task<int> GetCountByUserAndGameAsync(string userId, Guid gameId, CancellationToken cancellationToken = default)
    {
        return await _context.GamePlayLogs
            .CountAsync(p => p.UserId == userId && p.GameId == gameId, cancellationToken);
    }
}
