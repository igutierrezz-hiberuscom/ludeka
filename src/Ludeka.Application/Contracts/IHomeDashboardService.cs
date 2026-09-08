using System.Threading;
using System.Threading.Tasks;
using Ludeka.Application.DTOs;

namespace Ludeka.Application.Contracts;

public interface IHomeDashboardService
{
    Task<HomeDashboardDto> GetDashboardDataAsync(CancellationToken ct = default);
}
