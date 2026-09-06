using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ludeka.Application.DTOs;
using Ludeka.Core.Entities;

namespace Ludeka.Application.Contracts;

public interface IBggClient
{
    Task<Game?> FetchGameByBggIdAsync(int bggId, CancellationToken ct = default);
    Task<IReadOnlyList<BggCollectionItemDto>> FetchUserCollectionAsync(string username, CancellationToken ct = default);
    Task<IReadOnlyList<BggSearchResultDto>> SearchGamesAsync(string query, CancellationToken ct = default);
}
