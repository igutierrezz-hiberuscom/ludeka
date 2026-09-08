using System;
using System.Threading;
using System.Threading.Tasks;
using Ludeka.Application.DTOs;

namespace Ludeka.Application.Contracts;

public interface IBggImportService
{
    Task<BggImportResultDto> ImportUserCollectionAsync(BggImportRequest request, CancellationToken ct = default)
        => ImportUserCollectionAsync(request, null, ct);

    Task<BggImportResultDto> ImportUserCollectionAsync(
        BggImportRequest request,
        IProgress<BggImportProgressReport>? progress,
        CancellationToken ct = default);
}
