using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ludeka.Core.Entities;

namespace Ludeka.Application.Contracts;

public interface IGiveawayRepository
{
    Task<IReadOnlyList<Giveaway>> GetGiveawaysAsync(bool includeExpired = false, CancellationToken ct = default);
    Task<Giveaway?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Giveaway?> FindDuplicateOrCollaborativeAsync(string title, string organizer, DateTimeOffset deadline, CancellationToken ct = default);
    Task AddAsync(Giveaway giveaway, CancellationToken ct = default);
    Task UpdateAsync(Giveaway giveaway, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
