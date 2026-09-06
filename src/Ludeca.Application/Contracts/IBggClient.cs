using System.Threading;
using System.Threading.Tasks;
using Ludeca.Core.Entities;

namespace Ludeca.Application.Contracts;

public interface IBggClient
{
    Task<Game?> FetchGameByBggIdAsync(int bggId, CancellationToken ct = default);
}
