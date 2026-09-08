using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ludeka.Core.Entities;

namespace Ludeka.Application.Contracts;

public interface IGameEditLogRepository
{
    Task AddAsync(GameEditLog log, CancellationToken ct = default);
    Task<IReadOnlyList<GameEditLog>> GetByGameIdAsync(Guid gameId, CancellationToken ct = default);
}
