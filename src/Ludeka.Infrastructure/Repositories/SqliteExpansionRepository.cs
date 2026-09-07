using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ludeka.Application.Contracts;
using Ludeka.Core.Entities;
using Ludeka.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Ludeka.Infrastructure.Repositories;

public class SqliteExpansionRepository : IExpansionRepository
{
    private readonly LudekaDbContext _db;

    public SqliteExpansionRepository(LudekaDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async Task<IReadOnlyList<Game>> GetExpansionsByBaseGameIdAsync(Guid baseGameId, CancellationToken ct = default)
    {
        var items = await _db.Games
            .AsNoTracking()
            .Where(g => g.BaseGameId == baseGameId)
            .OrderBy(g => g.YearPublished)
            .ThenBy(g => g.SpanishTitle)
            .ToListAsync(ct);

        return items.AsReadOnly();
    }

    public async Task<Game?> GetExpansionWithBaseGameAsync(Guid expansionId, CancellationToken ct = default)
    {
        return await _db.Games
            .AsNoTracking()
            .Include(g => g.BaseGame)
            .FirstOrDefaultAsync(g => g.Id == expansionId, ct);
    }

    public async Task<IReadOnlyList<ExpansionSynergy>> GetSynergiesByBaseGameIdAsync(Guid baseGameId, CancellationToken ct = default)
    {
        var items = await _db.ExpansionSynergies
            .AsNoTracking()
            .Where(s => s.BaseGameId == baseGameId)
            .ToListAsync(ct);

        return items.AsReadOnly();
    }

    public async Task<IReadOnlyList<ExpansionRecipe>> GetRecipesByBaseGameIdAsync(Guid baseGameId, CancellationToken ct = default)
    {
        var items = await _db.ExpansionRecipes
            .AsNoTracking()
            .Where(r => r.BaseGameId == baseGameId)
            .ToListAsync(ct);

        return items.AsReadOnly();
    }

    public async Task AddSynergyAsync(ExpansionSynergy synergy, CancellationToken ct = default)
    {
        await _db.ExpansionSynergies.AddAsync(synergy, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task AddRecipeAsync(ExpansionRecipe recipe, CancellationToken ct = default)
    {
        await _db.ExpansionRecipes.AddAsync(recipe, ct);
        await _db.SaveChangesAsync(ct);
    }
}
