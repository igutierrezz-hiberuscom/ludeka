using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ludeca.Application.Contracts;
using Ludeca.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ludeca.Infrastructure.Data;

public class SqliteFoundingVerdictRepository : IFoundingVerdictRepository
{
    private readonly LudecaDbContext _context;

    public SqliteFoundingVerdictRepository(LudecaDbContext context)
    {
        _context = context;
    }

    public async Task<FoundingVerdict?> GetByGameIdAsync(Guid gameId, CancellationToken ct = default)
    {
        return await _context.FoundingVerdicts
            .FirstOrDefaultAsync(v => v.GameId == gameId, ct);
    }

    public async Task<FoundingVerdict?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.FoundingVerdicts
            .FirstOrDefaultAsync(v => v.Id == id, ct);
    }

    public async Task AddAsync(FoundingVerdict verdict, CancellationToken ct = default)
    {
        await _context.FoundingVerdicts.AddAsync(verdict, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(FoundingVerdict verdict, CancellationToken ct = default)
    {
        _context.FoundingVerdicts.Update(verdict);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var item = await _context.FoundingVerdicts.FindAsync([id], ct);
        if (item != null)
        {
            _context.FoundingVerdicts.Remove(item);
            await _context.SaveChangesAsync(ct);
        }
    }

    public async Task<IReadOnlyList<FoundingVerdict>> GetAllAsync(CancellationToken ct = default)
    {
        // En SQLite, el ordenamiento por DateTimeOffset debe realizarse en memoria
        var list = await _context.FoundingVerdicts.ToListAsync(ct);
        return list.OrderByDescending(v => v.CreatedAt).ToList().AsReadOnly();
    }
}
