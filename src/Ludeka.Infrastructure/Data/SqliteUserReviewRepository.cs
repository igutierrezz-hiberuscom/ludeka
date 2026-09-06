using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ludeka.Application.Contracts;
using Ludeka.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ludeka.Infrastructure.Data;

public class SqliteUserReviewRepository : IUserReviewRepository
{
    private readonly LudekaDbContext _context;

    public SqliteUserReviewRepository(LudekaDbContext context)
    {
        _context = context;
    }

    public async Task<UserGameReview?> GetByUserAndGameAsync(string userId, Guid gameId, CancellationToken cancellationToken = default)
    {
        return await _context.Reviews
            .Include(r => r.Game)
            .FirstOrDefaultAsync(r => r.UserId == userId && r.GameId == gameId, cancellationToken);
    }

    public async Task<List<UserGameReview>> GetByGameIdAsync(Guid gameId, CancellationToken cancellationToken = default)
    {
        var list = await _context.Reviews
            .Include(r => r.Game)
            .Where(r => r.GameId == gameId)
            .ToListAsync(cancellationToken);

        return list.OrderByDescending(r => r.CreatedAt).ToList();
    }

    public async Task<double?> GetAverageScoreByGameIdAsync(Guid gameId, CancellationToken cancellationToken = default)
    {
        var scores = await _context.Reviews
            .Where(r => r.GameId == gameId)
            .Select(r => r.Score)
            .ToListAsync(cancellationToken);

        if (scores.Count == 0) return null;

        return scores.Average();
    }

    public async Task AddAsync(UserGameReview review, CancellationToken cancellationToken = default)
    {
        await _context.Reviews.AddAsync(review, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(UserGameReview review, CancellationToken cancellationToken = default)
    {
        _context.Reviews.Update(review);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
