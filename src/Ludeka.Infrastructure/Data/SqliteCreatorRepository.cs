using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ludeka.Application.Contracts;
using Ludeka.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ludeka.Infrastructure.Data;

public class SqliteCreatorRepository : ICreatorRepository
{
    private readonly LudekaDbContext _context;

    public SqliteCreatorRepository(LudekaDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<IReadOnlyList<Creator>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.Creators
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync(ct);
    }

    public async Task<Creator?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Creators
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task<Creator?> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(slug)) return null;
        var normalized = slug.Trim().ToLowerInvariant();

        return await _context.Creators
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Slug == normalized, ct);
    }

    public async Task<Creator?> GetByNameAsync(string name, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(name)) return null;
        var normalized = name.Trim();

        return await _context.Creators
            .AsNoTracking()
            .FirstOrDefaultAsync(c => EF.Functions.Like(c.Name, normalized), ct);
    }

    public async Task AddAsync(Creator creator, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(creator);
        await _context.Creators.AddAsync(creator, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Creator creator, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(creator);

        var existing = await _context.Creators.FirstOrDefaultAsync(c => c.Id == creator.Id, ct);
        if (existing != null)
        {
            existing.UpdateDetails(
                creator.Name,
                creator.Nationality,
                creator.Bio,
                creator.AvatarUrl,
                creator.BggPersonId,
                creator.WebsiteUrl
            );
            existing.SetSocialLinks(creator.SocialLinks);
            await _context.SaveChangesAsync(ct);
        }
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var existing = await _context.Creators.FirstOrDefaultAsync(c => c.Id == id, ct);
        if (existing != null)
        {
            _context.Creators.Remove(existing);
            await _context.SaveChangesAsync(ct);
        }
    }
}
