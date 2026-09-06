using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ludeka.Application.Contracts;
using Ludeka.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ludeka.Infrastructure.Data;

public class SqliteGiveawayRepository : IGiveawayRepository
{
    private readonly LudekaDbContext _context;

    public SqliteGiveawayRepository(LudekaDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<IReadOnlyList<Giveaway>> GetGiveawaysAsync(bool includeExpired = false, CancellationToken ct = default)
    {
        // En SQLite cargamos en memoria para realizar el filtrado y ordenación por DateTimeOffset
        var list = await _context.Giveaways
            .Include(g => g.Game)
            .ToListAsync(ct);

        if (!includeExpired)
        {
            list = list.Where(g => !g.IsExpired).ToList();
        }

        return list.OrderBy(g => g.DeadlineAt).ToList();
    }

    public async Task<Giveaway?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Giveaways
            .Include(g => g.Game)
            .FirstOrDefaultAsync(g => g.Id == id, ct);
    }

    public async Task<Giveaway?> FindDuplicateOrCollaborativeAsync(string title, string organizer, DateTimeOffset deadline, CancellationToken ct = default)
    {
        var cleanTitle = title.Trim().ToLowerInvariant();
        var cleanOrganizer = organizer.Trim().ToLowerInvariant();

        var candidates = await _context.Giveaways.ToListAsync(ct);

        return candidates.FirstOrDefault(g =>
            !g.IsExpired &&
            g.Title.Trim().ToLowerInvariant().Equals(cleanTitle, StringComparison.OrdinalIgnoreCase) &&
            (g.Organizer.Trim().ToLowerInvariant().Contains(cleanOrganizer) ||
             cleanOrganizer.Contains(g.Organizer.Trim().ToLowerInvariant()) ||
             (g.Collaborator != null && g.Collaborator.Trim().ToLowerInvariant().Contains(cleanOrganizer))));
    }

    public async Task AddAsync(Giveaway giveaway, CancellationToken ct = default)
    {
        await _context.Giveaways.AddAsync(giveaway, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Giveaway giveaway, CancellationToken ct = default)
    {
        _context.Giveaways.Update(giveaway);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var item = await _context.Giveaways.FindAsync(new object[] { id }, ct);
        if (item != null)
        {
            _context.Giveaways.Remove(item);
            await _context.SaveChangesAsync(ct);
        }
    }
}
