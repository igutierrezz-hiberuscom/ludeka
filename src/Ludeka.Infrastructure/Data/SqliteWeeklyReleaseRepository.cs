using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ludeka.Application.Contracts;
using Ludeka.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ludeka.Infrastructure.Data;

public class SqliteWeeklyReleaseRepository : IWeeklyReleaseRepository
{
    private readonly LudekaDbContext _context;

    public SqliteWeeklyReleaseRepository(LudekaDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<IReadOnlyList<WeeklyRelease>> GetReleasesAsync(DateOnly? fromDate = null, CancellationToken ct = default)
    {
        var query = _context.WeeklyReleases
            .Include(r => r.Game)
            .AsQueryable();

        if (fromDate.HasValue)
        {
            query = query.Where(r => r.ReleaseDate >= fromDate.Value);
        }

        return await query
            .OrderBy(r => r.ReleaseDate)
            .ThenBy(r => r.Title)
            .ToListAsync(ct);
    }

    public async Task<WeeklyRelease?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.WeeklyReleases
            .Include(r => r.Game)
            .FirstOrDefaultAsync(r => r.Id == id, ct);
    }

    public async Task AddAsync(WeeklyRelease release, CancellationToken ct = default)
    {
        await _context.WeeklyReleases.AddAsync(release, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(WeeklyRelease release, CancellationToken ct = default)
    {
        _context.WeeklyReleases.Update(release);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var item = await _context.WeeklyReleases.FindAsync(new object[] { id }, ct);
        if (item != null)
        {
            _context.WeeklyReleases.Remove(item);
            await _context.SaveChangesAsync(ct);
        }
    }
}
