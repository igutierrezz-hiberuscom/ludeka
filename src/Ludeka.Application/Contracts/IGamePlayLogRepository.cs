using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ludeka.Core.Entities;

namespace Ludeka.Application.Contracts;

public interface IGamePlayLogRepository
{
    Task AddAsync(GamePlayLog play, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<GamePlayLog?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<GamePlayLog>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<List<GamePlayLog>> GetByUserAndGameAsync(string userId, Guid gameId, CancellationToken cancellationToken = default);
    Task<int> GetCountByUserAsync(string userId, CancellationToken cancellationToken = default);
    Task<int> GetCountByUserAndGameAsync(string userId, Guid gameId, CancellationToken cancellationToken = default);
}
