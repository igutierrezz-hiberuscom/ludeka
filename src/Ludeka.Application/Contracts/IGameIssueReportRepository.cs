using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ludeka.Application.DTOs;
using Ludeka.Core.Entities;

namespace Ludeka.Application.Contracts;

public interface IGameIssueReportRepository
{
    ValueTask<GameIssueReport?> GetByIdAsync(Guid id, CancellationToken ct = default);
    ValueTask<IReadOnlyList<GameIssueReport>> GetAllAsync(GameReportFilter filter, CancellationToken ct = default);
    ValueTask<GameIssueReportSummaryDto> GetSummaryAsync(CancellationToken ct = default);
    ValueTask AddAsync(GameIssueReport report, CancellationToken ct = default);
    ValueTask UpdateAsync(GameIssueReport report, CancellationToken ct = default);
}
