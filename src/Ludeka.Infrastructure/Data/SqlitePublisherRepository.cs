using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ludeka.Application.Contracts;
using Ludeka.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ludeka.Infrastructure.Data;

public class SqlitePublisherRepository : IPublisherRepository
{
    private readonly LudekaDbContext _context;

    public SqlitePublisherRepository(LudekaDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<IReadOnlyList<Publisher>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.Publishers
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .ToListAsync(ct);
    }

    public async Task<Publisher?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Publishers
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<Publisher?> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(slug)) return null;
        var normalized = slug.Trim().ToLowerInvariant();

        return await _context.Publishers
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Slug == normalized, ct);
    }

    public async Task<Publisher?> GetByNameAsync(string name, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(name)) return null;
        var normalized = name.Trim();

        return await _context.Publishers
            .AsNoTracking()
            .FirstOrDefaultAsync(p => EF.Functions.Like(p.Name, normalized), ct);
    }

    public async Task AddAsync(Publisher publisher, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(publisher);
        await _context.Publishers.AddAsync(publisher, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Publisher publisher, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(publisher);

        var existing = await _context.Publishers.FirstOrDefaultAsync(p => p.Id == publisher.Id, ct);
        if (existing != null)
        {
            existing.UpdateDetails(
                publisher.Name,
                publisher.Country,
                publisher.City,
                publisher.Description,
                publisher.LogoUrl,
                publisher.WebsiteUrl
            );
            existing.SetSocialLinks(publisher.SocialLinks);
            await _context.SaveChangesAsync(ct);
        }
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var existing = await _context.Publishers.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (existing != null)
        {
            _context.Publishers.Remove(existing);
            await _context.SaveChangesAsync(ct);
        }
    }
}
