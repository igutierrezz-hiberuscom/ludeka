using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ludeka.Application.DTOs;

namespace Ludeka.Application.Contracts;

public interface IBggCatalogQueueService
{
    Task<IReadOnlyList<CatalogQueueItemDto>> GetTopPendingQueueAsync(int limit = 50, CancellationToken ct = default);
    Task<ProcessQueueResultDto> ProcessPendingQueueBatchAsync(int batchSize = 20, CancellationToken ct = default);
    Task<int> GetTotalPendingCountAsync(CancellationToken ct = default);
    Task ResetFailedItemsAsync(CancellationToken ct = default);
}
