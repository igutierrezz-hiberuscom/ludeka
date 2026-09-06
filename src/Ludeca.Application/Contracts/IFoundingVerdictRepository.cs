using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ludeca.Core.Entities;

namespace Ludeca.Application.Contracts;

public interface IFoundingVerdictRepository
{
    Task<FoundingVerdict?> GetByGameIdAsync(Guid gameId, CancellationToken ct = default);
    Task<FoundingVerdict?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(FoundingVerdict verdict, CancellationToken ct = default);
    Task UpdateAsync(FoundingVerdict verdict, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<FoundingVerdict>> GetAllAsync(CancellationToken ct = default);
}
