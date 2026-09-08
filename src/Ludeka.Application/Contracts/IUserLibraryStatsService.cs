using System.Threading;
using System.Threading.Tasks;
using Ludeka.Application.DTOs;

namespace Ludeka.Application.Contracts;

public interface IUserLibraryStatsService
{
    Task<UserLibraryStatsDto> GetUserStatsAsync(string? userId = null, CancellationToken ct = default);
    Task<PublicUserProfileDto?> GetPublicProfileAsync(string userId, CancellationToken ct = default);
}
