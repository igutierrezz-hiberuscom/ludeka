using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ludeca.Core.Entities;
using Ludeca.Core.Enums;

namespace Ludeca.Application.Contracts;

public interface IUserCollectionRepository
{
    Task<UserCollectionItem?> GetByUserAndGameAsync(string userId, Guid gameId, CancellationToken cancellationToken = default);
    Task<List<UserCollectionItem>> GetByUserIdAsync(string userId, CollectionStatus? status = null, CancellationToken cancellationToken = default);
    Task<Dictionary<CollectionStatus, int>> GetCountsByStatusAsync(string userId, CancellationToken cancellationToken = default);
    Task AddAsync(UserCollectionItem item, CancellationToken cancellationToken = default);
    Task UpdateAsync(UserCollectionItem item, CancellationToken cancellationToken = default);
    Task RemoveAsync(UserCollectionItem item, CancellationToken cancellationToken = default);
}
