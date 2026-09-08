using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ludeka.Core.Entities;
using Ludeka.Core.Enums;

namespace Ludeka.Application.Contracts;

public interface IUserCollectionRepository
{
    Task<UserCollectionItem?> GetByUserAndGameAsync(string userId, Guid gameId, CancellationToken cancellationToken = default);
    Task<UserCollectionItem?> GetByUserAndBggIdAsync(string userId, int bggId, CancellationToken cancellationToken = default);
    Task<List<UserCollectionItem>> GetByUserIdAsync(string userId, CollectionStatus? status = null, CancellationToken cancellationToken = default);
    Task<List<UserCollectionItem>> GetPendingItemsByBggIdAsync(int bggId, CancellationToken cancellationToken = default);
    Task PromotePendingItemsAsync(int bggId, Guid gameId, CancellationToken cancellationToken = default);
    Task<Dictionary<CollectionStatus, int>> GetCountsByStatusAsync(string userId, CancellationToken cancellationToken = default);
    Task<List<UserCollectionItem>> GetPlayedByUserIdAsync(string userId, CancellationToken cancellationToken = default)
        => Task.FromResult(new List<UserCollectionItem>());
    Task<int> GetPlayedCountAsync(string userId, CancellationToken cancellationToken = default)
        => Task.FromResult(0);
    Task AddAsync(UserCollectionItem item, CancellationToken cancellationToken = default);
    Task UpdateAsync(UserCollectionItem item, CancellationToken cancellationToken = default);
    Task RemoveAsync(UserCollectionItem item, CancellationToken cancellationToken = default);
}
