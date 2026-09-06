using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ludeka.Application.DTOs;
using Ludeka.Core.Entities;

namespace Ludeka.Application.Contracts;

public interface IMediaService
{
    Task<GameMediaHubDto> GetGameMediaAsync(Guid gameId, CancellationToken ct = default);
    Task<IReadOnlyList<MediaItemDto>> GetPendingModerationAsync(CancellationToken ct = default);
    Task<IReadOnlyList<MediaItemDto>> GetOrphansAsync(CancellationToken ct = default);
    Task<IReadOnlyList<MediaItemDto>> GetApprovedAsync(CancellationToken ct = default);
    Task<MediaItemDto?> ApproveMediaAsync(Guid id, CancellationToken ct = default);
    Task<MediaItemDto?> RejectMediaAsync(Guid id, CancellationToken ct = default);
    Task<MediaItemDto?> AssignOrphanMediaAsync(Guid id, Guid gameId, CancellationToken ct = default);
    Task<BrokenLinkReportDto> CheckBrokenLinksAsync(CancellationToken ct = default);
    Task<MediaItemDto> CreateMediaItemAsync(MediaItem item, CancellationToken ct = default);
}
