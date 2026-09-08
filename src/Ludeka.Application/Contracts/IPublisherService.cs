using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ludeka.Application.DTOs;

namespace Ludeka.Application.Contracts;

public interface IPublisherService
{
    Task<IReadOnlyList<PublisherDto>> GetAllAsync(string? search = null, CancellationToken ct = default);
    Task<PublisherDetailDto?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<PublisherDetailDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PublisherDto> CreateAsync(CreatePublisherDto dto, CancellationToken ct = default);
    Task<PublisherDto> UpdateAsync(Guid id, UpdatePublisherDto dto, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
