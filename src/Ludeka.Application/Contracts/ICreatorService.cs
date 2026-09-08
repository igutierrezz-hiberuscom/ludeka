using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ludeka.Application.DTOs;

namespace Ludeka.Application.Contracts;

public interface ICreatorService
{
    Task<IReadOnlyList<CreatorDto>> GetAllAsync(string? search = null, CancellationToken ct = default);
    Task<CreatorDetailDto?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<CreatorDetailDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<CreatorDto> CreateAsync(CreateCreatorDto dto, CancellationToken ct = default);
    Task<CreatorDto> UpdateAsync(Guid id, UpdateCreatorDto dto, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
