using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ludeka.Application.DTOs;
using Ludeka.Core.Enums;

namespace Ludeka.Application.Contracts;

public interface IStoreService
{
    Task<IReadOnlyList<StoreDto>> GetAllAsync(string? search = null, StoreType? type = null, string? country = null, CancellationToken ct = default);
    Task<StoreDetailDto?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<StoreDetailDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<StoreDto> CreateAsync(CreateStoreDto dto, CancellationToken ct = default);
    Task<StoreDto> UpdateAsync(Guid id, UpdateStoreDto dto, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
