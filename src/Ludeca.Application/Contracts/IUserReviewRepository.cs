using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ludeca.Core.Entities;

namespace Ludeca.Application.Contracts;

public interface IUserReviewRepository
{
    Task<UserGameReview?> GetByUserAndGameAsync(string userId, Guid gameId, CancellationToken cancellationToken = default);
    Task<List<UserGameReview>> GetByGameIdAsync(Guid gameId, CancellationToken cancellationToken = default);
    Task<double?> GetAverageScoreByGameIdAsync(Guid gameId, CancellationToken cancellationToken = default);
    Task AddAsync(UserGameReview review, CancellationToken cancellationToken = default);
    Task UpdateAsync(UserGameReview review, CancellationToken cancellationToken = default);
}
