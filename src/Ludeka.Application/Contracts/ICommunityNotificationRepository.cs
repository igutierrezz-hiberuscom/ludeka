using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ludeka.Core.Entities;

namespace Ludeka.Application.Contracts;

public interface ICommunityNotificationRepository
{
    Task<IReadOnlyList<CommunityNotificationLog>> GetRecentLogsAsync(int take = 50, CancellationToken ct = default);
    Task<CommunityNotificationLog?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddLogAsync(CommunityNotificationLog log, CancellationToken ct = default);
    Task UpdateLogAsync(CommunityNotificationLog log, CancellationToken ct = default);
}
