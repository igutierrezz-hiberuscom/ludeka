using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ludeka.Application.DTOs;

namespace Ludeka.Application.Contracts;

public interface IBoardGameEventService
{
    Task<IReadOnlyList<BoardGameEventDto>> GetUpcomingEventsAsync(int limit = 50, string? country = null, CancellationToken ct = default);
    Task<IReadOnlyList<BoardGameEventDto>> GetPastEventsAsync(int limit = 50, string? country = null, CancellationToken ct = default);
    Task<BoardGameEventDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<BoardGameEventDto> CreateEventAsync(CreateBoardGameEventRequest request, CancellationToken ct = default);
    Task<BoardGameEventDto> UpdateEventAsync(Guid id, UpdateBoardGameEventRequest request, CancellationToken ct = default);
    Task DeleteEventAsync(Guid id, CancellationToken ct = default);
}
