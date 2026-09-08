using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ludeka.Core.Entities;

namespace Ludeka.Application.Contracts;

public interface IPublisherRepository
{
    Task<IReadOnlyList<Publisher>> GetAllAsync(CancellationToken ct = default);
    Task<Publisher?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Publisher?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<Publisher?> GetByNameAsync(string name, CancellationToken ct = default);
    Task AddAsync(Publisher publisher, CancellationToken ct = default);
    Task UpdateAsync(Publisher publisher, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
