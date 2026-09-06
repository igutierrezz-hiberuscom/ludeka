using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ludeka.Application.DTOs;

namespace Ludeka.Application.Contracts;

public interface IGiveawayService
{
    Task<IReadOnlyList<GiveawayDto>> GetGiveawaysAsync(bool includeExpired = false, CancellationToken ct = default);
    Task<GiveawayDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<GiveawayDto> CreateOrMergeGiveawayAsync(CreateGiveawayRequest request, CancellationToken ct = default);
}
