using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ludeka.Application.Contracts;
using Ludeka.Core.Entities;
using Ludeka.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace Ludeka.Infrastructure.Data;

public class SqlitePendingBggImportRepository : IPendingBggImportRepository
{
    private readonly LudekaDbContext _context;

    public SqlitePendingBggImportRepository(LudekaDbContext context)
    {
        _context = context;
    }

    public async Task<PendingBggImport?> GetByBggIdAsync(int bggId, CancellationToken ct = default)
    {
        return await _context.PendingBggImports
            .FirstOrDefaultAsync(p => p.BggId == bggId, ct);
    }

    public async Task<IReadOnlyList<PendingBggImport>> GetTopPendingAsync(int limit = 50, CancellationToken ct = default)
    {
        return await _context.PendingBggImports
            .Where(p => p.Status == CatalogQueueStatus.Pending)
            .OrderByDescending(p => p.RequestedCount)
            .Take(limit)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<PendingBggImport>> GetAllAsync(CatalogQueueStatus? status = null, CancellationToken ct = default)
    {
        var query = _context.PendingBggImports.AsQueryable();
        if (status.HasValue)
        {
            query = query.Where(p => p.Status == status.Value);
        }

        return await query
            .OrderByDescending(p => p.RequestedCount)
            .ToListAsync(ct);
    }

    public async Task<int> GetTotalPendingCountAsync(CancellationToken ct = default)
    {
        return await _context.PendingBggImports
            .CountAsync(p => p.Status == CatalogQueueStatus.Pending, ct);
    }

    public async Task AddAsync(PendingBggImport item, CancellationToken ct = default)
    {
        await _context.PendingBggImports.AddAsync(item, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(PendingBggImport item, CancellationToken ct = default)
    {
        _context.PendingBggImports.Update(item);
        await _context.SaveChangesAsync(ct);
    }
}
