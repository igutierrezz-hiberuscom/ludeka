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

public class SqliteMediaRepository : IMediaRepository
{
    private readonly LudekaDbContext _context;

    public SqliteMediaRepository(LudekaDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<MediaItem?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.MediaItems
            .Include(m => m.Game)
            .FirstOrDefaultAsync(m => m.Id == id, ct);
    }

    public async Task<IReadOnlyList<MediaItem>> GetApprovedByGameIdAsync(Guid gameId, CancellationToken ct = default)
    {
        var items = await _context.MediaItems
            .Include(m => m.Game)
            .Where(m => m.GameId == gameId && m.Status == ModerationStatus.Approved)
            .ToListAsync(ct);

        // En SQLite el ordenamiento por DateTimeOffset se ejecuta en memoria
        return items.OrderByDescending(m => m.PublishedAt).ToList();
    }

    public async Task<IReadOnlyList<MediaItem>> GetPendingModerationAsync(CancellationToken ct = default)
    {
        var items = await _context.MediaItems
            .Include(m => m.Game)
            .Where(m => m.Status == ModerationStatus.PendingApproval)
            .ToListAsync(ct);

        return items.OrderByDescending(m => m.CreatedAt).ToList();
    }

    public async Task<IReadOnlyList<MediaItem>> GetOrphansAsync(CancellationToken ct = default)
    {
        var items = await _context.MediaItems
            .Where(m => m.GameId == null)
            .ToListAsync(ct);

        return items.OrderByDescending(m => m.CreatedAt).ToList();
    }

    public async Task<IReadOnlyList<MediaItem>> GetApprovedAsync(CancellationToken ct = default)
    {
        var items = await _context.MediaItems
            .Include(m => m.Game)
            .Where(m => m.Status == ModerationStatus.Approved)
            .ToListAsync(ct);

        return items.OrderByDescending(m => m.PublishedAt).ToList();
    }

    public async Task<IReadOnlyList<MediaItem>> GetAllAsync(CancellationToken ct = default)
    {
        var items = await _context.MediaItems
            .Include(m => m.Game)
            .ToListAsync(ct);

        return items.OrderByDescending(m => m.CreatedAt).ToList();
    }

    public async Task AddAsync(MediaItem item, CancellationToken ct = default)
    {
        await _context.MediaItems.AddAsync(item, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(MediaItem item, CancellationToken ct = default)
    {
        _context.MediaItems.Update(item);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var item = await _context.MediaItems.FindAsync([id], ct);
        if (item != null)
        {
            _context.MediaItems.Remove(item);
            await _context.SaveChangesAsync(ct);
        }
    }
}
