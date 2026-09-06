using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ludeka.Application.DTOs;

namespace Ludeka.Application.Features.Catalog;

public record CatalogResult(IReadOnlyList<GameSummaryDto> Games, int TotalCount, int Page, int PageSize);

public interface ICatalogService
{
    Task<CatalogResult> GetCatalogAsync(GameFilterCriteria criteria, int page = 1, int pageSize = 20, CancellationToken ct = default);
    Task<GameDetailDto?> GetGameBySlugAsync(string slug, CancellationToken ct = default);
    Task<IReadOnlyList<GameSummaryDto>> GetQuickSearchAsync(string term, int limit = 5, CancellationToken ct = default);
}
