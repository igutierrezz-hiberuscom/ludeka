using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ludeca.Application.DTOs;
using Ludeca.Core.Entities;

namespace Ludeca.Application.Contracts;

public interface IGameRepository
{
    Task<Game?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Game?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<Game?> GetByBggIdAsync(int bggId, CancellationToken ct = default);
    Task<(IReadOnlyList<Game> Items, int TotalCount)> SearchAsync(GameFilterCriteria criteria, int page = 1, int pageSize = 20, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<Game> games, CancellationToken ct = default);
    Task UpdateAsync(Game game, CancellationToken ct = default);
    Task<bool> HasAnyAsync(CancellationToken ct = default);
}
