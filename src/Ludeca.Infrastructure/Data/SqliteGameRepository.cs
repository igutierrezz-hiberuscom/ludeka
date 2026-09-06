using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ludeca.Application.Contracts;
using Ludeca.Application.DTOs;
using Ludeca.Core.Entities;
using Ludeca.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace Ludeca.Infrastructure.Data;

public class SqliteGameRepository : IGameRepository
{
    private readonly LudecaDbContext _context;

    public SqliteGameRepository(LudecaDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Game?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Games
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Id == id, ct);
    }

    public async Task<Game?> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(slug)) return null;
        string normalized = slug.Trim().ToLowerInvariant();

        return await _context.Games
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Slug == normalized, ct);
    }

    public async Task<Game?> GetByBggIdAsync(int bggId, CancellationToken ct = default)
    {
        return await _context.Games
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.BggId == bggId, ct);
    }

    public async Task<(IReadOnlyList<Game> Items, int TotalCount)> SearchAsync(
        GameFilterCriteria criteria, int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;

        var query = _context.Games.AsNoTracking().AsQueryable();

        // Filtro por término de búsqueda (bilingüe: título español o título original)
        if (!string.IsNullOrWhiteSpace(criteria.SearchTerm))
        {
            string term = criteria.SearchTerm.Trim();
            query = query.Where(g =>
                EF.Functions.Like(g.SpanishTitle, $"%{term}%") ||
                EF.Functions.Like(g.OriginalTitle, $"%{term}%") ||
                EF.Functions.Like(g.Designer, $"%{term}%"));
        }

        // Filtro por estilo lúdico
        if (criteria.Style.HasValue)
        {
            query = query.Where(g => g.Style == criteria.Style.Value);
        }

        // Filtro por confrontación
        if (criteria.Confrontation.HasValue)
        {
            query = query.Where(g => g.Confrontation == criteria.Confrontation.Value);
        }

        // Filtro por duración máxima
        if (criteria.MaxDurationMinutes.HasValue)
        {
            query = query.Where(g => g.Duration.MaxMinutes <= criteria.MaxDurationMinutes.Value);
        }

        // Filtro "Mesa Familiar" (edad comunitaria <= 10 y dependencia de idioma nula o baja)
        if (criteria.MesaFamiliar)
        {
            query = query.Where(g =>
                g.Age.CommunityAge <= 10 &&
                g.Language != LanguageDependence.High);
        }

        // Filtro "Solo Top" (modo solitario oficial)
        if (criteria.SoloTop)
        {
            query = query.Where(g => g.IsOfficialSolo);
        }

        // Para filtros que evalúan elementos de colecciones JSON complejas en SQLite
        var list = await query.ToListAsync(ct);

        if (criteria.EspecialParejas)
        {
            list = list.Where(g => g.Scalability.Any(s => s.PlayerCount == 2 && s.Status == ScalabilityStatus.MustPlay)).ToList();
        }

        if (criteria.PlayerCount.HasValue)
        {
            int p = criteria.PlayerCount.Value;
            list = list.Where(g => g.Scalability.Any(s =>
                (p >= 7 ? s.PlayerCount >= 7 : s.PlayerCount == p) &&
                s.Status != ScalabilityStatus.NotRecommended)).ToList();
        }

        int totalCount = list.Count;

        // Ordenar por ranking BGG (con los rankeados primero) y luego rating
        var paged = list
            .OrderBy(g => g.BggRank.HasValue ? 0 : 1)
            .ThenBy(g => g.BggRank ?? int.MaxValue)
            .ThenByDescending(g => g.BggRating)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return (paged, totalCount);
    }

    public async Task AddRangeAsync(IEnumerable<Game> games, CancellationToken ct = default)
    {
        await _context.Games.AddRangeAsync(games, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Game game, CancellationToken ct = default)
    {
        var existing = await _context.Games.FirstOrDefaultAsync(g => g.Id == game.Id, ct);
        if (existing != null)
        {
            existing.UpdateLudistRating(game.LudistRating);
            await _context.SaveChangesAsync(ct);
        }
    }

    public async Task<bool> HasAnyAsync(CancellationToken ct = default)
    {
        return await _context.Games.AnyAsync(ct);
    }
}
