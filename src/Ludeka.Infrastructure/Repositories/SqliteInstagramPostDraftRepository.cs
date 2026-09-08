using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ludeka.Application.Contracts;
using Ludeka.Core.Entities;
using Ludeka.Core.Enums;
using Ludeka.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Ludeka.Infrastructure.Repositories;

public class SqliteInstagramPostDraftRepository : IInstagramPostDraftRepository
{
    private readonly LudekaDbContext _context;

    public SqliteInstagramPostDraftRepository(LudekaDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<IReadOnlyList<InstagramPostDraft>> GetDraftsAsync(
        InstagramPostDraftStatus? status = null,
        CancellationToken ct = default)
    {
        IQueryable<InstagramPostDraft> query = _context.InstagramPostDrafts.AsNoTracking();

        if (status.HasValue)
        {
            query = query.Where(d => d.Status == status.Value);
        }

        return await query
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<InstagramPostDraft?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.InstagramPostDrafts
            .FirstOrDefaultAsync(d => d.Id == id, ct);
    }

    public async Task<InstagramPostDraft?> GetBySourceAsync(
        InstagramPostSourceType sourceType,
        string sourceId,
        CancellationToken ct = default)
    {
        return await _context.InstagramPostDrafts
            .FirstOrDefaultAsync(d => d.SourceType == sourceType && d.SourceId == sourceId, ct);
    }

    public async Task AddAsync(InstagramPostDraft draft, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(draft);

        await _context.InstagramPostDrafts.AddAsync(draft, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(InstagramPostDraft draft, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(draft);

        _context.InstagramPostDrafts.Update(draft);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var draft = await _context.InstagramPostDrafts.FindAsync(new object[] { id }, ct);
        if (draft != null)
        {
            _context.InstagramPostDrafts.Remove(draft);
            await _context.SaveChangesAsync(ct);
        }
    }
}
