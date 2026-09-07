using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ludeka.Application.Contracts;
using Ludeka.Core.Entities;
using Ludeka.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Ludeka.Infrastructure.Repositories;

public class SqliteCommunityNotificationRepository : ICommunityNotificationRepository
{
    private readonly LudekaDbContext _dbContext;

    public SqliteCommunityNotificationRepository(LudekaDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<CommunityNotificationLog>> GetRecentLogsAsync(
        int take = 50,
        CancellationToken ct = default)
    {
        var logs = await _dbContext.NotificationLogs
            .AsNoTracking()
            .ToListAsync(ct);

        return logs
            .OrderByDescending(l => l.CreatedAt)
            .Take(take)
            .ToList();
    }

    public async Task<CommunityNotificationLog?> GetByIdAsync(
        Guid id,
        CancellationToken ct = default)
    {
        return await _dbContext.NotificationLogs
            .FirstOrDefaultAsync(l => l.Id == id, ct);
    }

    public async Task AddLogAsync(
        CommunityNotificationLog log,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(log);
        await _dbContext.NotificationLogs.AddAsync(log, ct);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task UpdateLogAsync(
        CommunityNotificationLog log,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(log);
        _dbContext.NotificationLogs.Update(log);
        await _dbContext.SaveChangesAsync(ct);
    }
}
