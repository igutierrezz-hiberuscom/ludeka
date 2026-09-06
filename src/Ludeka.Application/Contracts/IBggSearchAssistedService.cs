using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ludeka.Application.DTOs;
using Ludeka.Core.Enums;

namespace Ludeka.Application.Contracts;

public interface IBggSearchAssistedService
{
    Task<IReadOnlyList<BggSearchResultDto>> SearchGamesAsync(string query, CancellationToken ct = default);
    Task<Guid> AddGameToCollectionAsync(int bggId, CollectionStatus status, CancellationToken ct = default);
}
