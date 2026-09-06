using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ludeca.Application.Contracts;
using Ludeca.Core.Entities;
using Ludeca.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace Ludeca.Infrastructure.Data;

public class SqliteUserCollectionRepository : IUserCollectionRepository
{
    private readonly LudecaDbContext _context;

    public SqliteUserCollectionRepository(LudecaDbContext context)
    {
        _context = context;
    }

    public async Task<UserCollectionItem?> GetByUserAndGameAsync(string userId, Guid gameId, CancellationToken cancellationToken = default)
    {
        return await _context.CollectionItems
            .Include(c => c.Game)
            .FirstOrDefaultAsync(c => c.UserId == userId && c.GameId == gameId, cancellationToken);
    }

    public async Task<List<UserCollectionItem>> GetByUserIdAsync(string userId, CollectionStatus? status = null, CancellationToken cancellationToken = default)
    {
        var query = _context.CollectionItems
            .Include(c => c.Game)
            .Where(c => c.UserId == userId);

        if (status.HasValue)
        {
            query = query.Where(c => c.Status == status.Value);
        }

        var list = await query.ToListAsync(cancellationToken);
        return list.OrderByDescending(c => c.AddedAt).ToList();
    }

    public async Task<Dictionary<CollectionStatus, int>> GetCountsByStatusAsync(string userId, CancellationToken cancellationToken = default)
    {
        var counts = await _context.CollectionItems
            .Where(c => c.UserId == userId)
            .GroupBy(c => c.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count, cancellationToken);

        return counts;
    }

    public async Task AddAsync(UserCollectionItem item, CancellationToken cancellationToken = default)
    {
        await _context.CollectionItems.AddAsync(item, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(UserCollectionItem item, CancellationToken cancellationToken = default)
    {
        _context.CollectionItems.Update(item);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveAsync(UserCollectionItem item, CancellationToken cancellationToken = default)
    {
        _context.CollectionItems.Remove(item);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
