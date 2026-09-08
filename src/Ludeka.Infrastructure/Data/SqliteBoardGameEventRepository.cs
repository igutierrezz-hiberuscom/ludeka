using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ludeka.Application.Contracts;
using Ludeka.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ludeka.Infrastructure.Data;

public class SqliteBoardGameEventRepository : IBoardGameEventRepository
{
    private readonly LudekaDbContext _context;

    public SqliteBoardGameEventRepository(LudekaDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<IReadOnlyList<BoardGameEvent>> GetUpcomingEventsAsync(int limit = 20, CancellationToken ct = default)
    {
        if (limit < 1) limit = 20;

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // Primero eventos vigentes o futuros (EndDate >= hoy) ordenados por StartDate ascendente
        var upcoming = await _context.BoardGameEvents
            .AsNoTracking()
            .Where(e => e.EndDate >= today)
            .OrderBy(e => e.StartDate)
            .Take(limit)
            .ToListAsync(ct);

        if (upcoming.Count < limit)
        {
            // Si hay pocos eventos futuros, completar con los más recientes para no dejar el carril vacío
            int remaining = limit - upcoming.Count;
            var past = await _context.BoardGameEvents
                .AsNoTracking()
                .Where(e => e.EndDate < today)
                .OrderByDescending(e => e.EndDate)
                .Take(remaining)
                .ToListAsync(ct);

            upcoming.AddRange(past);
        }

        return upcoming;
    }

    public async Task<IReadOnlyList<BoardGameEvent>> GetAllEventsAsync(CancellationToken ct = default)
    {
        return await _context.BoardGameEvents
            .AsNoTracking()
            .OrderBy(e => e.StartDate)
            .ToListAsync(ct);
    }

    public async Task<BoardGameEvent?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.BoardGameEvents
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id, ct);
    }

    public async Task AddAsync(BoardGameEvent boardGameEvent, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(boardGameEvent);
        await _context.BoardGameEvents.AddAsync(boardGameEvent, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(BoardGameEvent boardGameEvent, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(boardGameEvent);
        _context.BoardGameEvents.Update(boardGameEvent);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _context.BoardGameEvents.FirstOrDefaultAsync(e => e.Id == id, ct);
        if (entity != null)
        {
            _context.BoardGameEvents.Remove(entity);
            await _context.SaveChangesAsync(ct);
        }
    }
}
