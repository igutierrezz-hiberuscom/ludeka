using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ludeka.Core.Entities;

namespace Ludeka.Application.Contracts;

public interface IMediaRepository
{
    Task<MediaItem?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<MediaItem>> GetApprovedByGameIdAsync(Guid gameId, CancellationToken ct = default);
    Task<IReadOnlyList<MediaItem>> GetPendingModerationAsync(CancellationToken ct = default);
    Task<IReadOnlyList<MediaItem>> GetOrphansAsync(CancellationToken ct = default);
    Task<IReadOnlyList<MediaItem>> GetApprovedAsync(CancellationToken ct = default);
    Task<IReadOnlyList<MediaItem>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(MediaItem item, CancellationToken ct = default);
    Task UpdateAsync(MediaItem item, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
