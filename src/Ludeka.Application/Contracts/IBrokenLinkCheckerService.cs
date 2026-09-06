using System.Threading;
using System.Threading.Tasks;
using Ludeka.Application.DTOs;

namespace Ludeka.Application.Contracts;

public interface IBrokenLinkCheckerService
{
    Task<BrokenLinkReportDto> CheckLinksAsync(CancellationToken ct = default);
}
