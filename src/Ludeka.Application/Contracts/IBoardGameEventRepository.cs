using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ludeka.Core.Entities;

namespace Ludeka.Application.Contracts;

public interface IBoardGameEventRepository
{
    Task<IReadOnlyList<BoardGameEvent>> GetUpcomingEventsAsync(int limit = 20, CancellationToken ct = default);
    Task<IReadOnlyList<BoardGameEvent>> GetAllEventsAsync(CancellationToken ct = default);
    Task<BoardGameEvent?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(BoardGameEvent boardGameEvent, CancellationToken ct = default);
    Task UpdateAsync(BoardGameEvent boardGameEvent, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
