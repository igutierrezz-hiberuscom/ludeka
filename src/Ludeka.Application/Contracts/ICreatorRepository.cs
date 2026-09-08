using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ludeka.Core.Entities;

namespace Ludeka.Application.Contracts;

public interface ICreatorRepository
{
    Task<IReadOnlyList<Creator>> GetAllAsync(CancellationToken ct = default);
    Task<Creator?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Creator?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<Creator?> GetByNameAsync(string name, CancellationToken ct = default);
    Task AddAsync(Creator creator, CancellationToken ct = default);
    Task UpdateAsync(Creator creator, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
