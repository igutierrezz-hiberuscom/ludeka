using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ludeka.Application.DTOs;

namespace Ludeka.Application.Contracts;

public interface IGamePlayLogService
{
    Task<GamePlayLogDto> RecordPlayAsync(RecordPlayRequest request, CancellationToken cancellationToken = default);
    Task<List<GamePlayLogDto>> GetUserPlaysAsync(string? userId = null, CancellationToken cancellationToken = default);
    Task<List<GamePlayLogDto>> GetGamePlaysAsync(Guid gameId, string? userId = null, CancellationToken cancellationToken = default);
    Task<UserPlaysStatsDto> GetUserPlaysStatsAsync(string? userId = null, CancellationToken cancellationToken = default);
    Task DeletePlayAsync(Guid playId, CancellationToken cancellationToken = default);
}
