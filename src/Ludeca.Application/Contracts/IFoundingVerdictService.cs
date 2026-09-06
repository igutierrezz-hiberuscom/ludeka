using System;
using System.Threading;
using System.Threading.Tasks;
using Ludeca.Application.DTOs;

namespace Ludeca.Application.Contracts;

public interface IFoundingVerdictService
{
    Task<FoundingVerdictDto?> GetVerdictByGameIdAsync(Guid gameId, CancellationToken ct = default);
    Task<AiGameSummaryDto> GetAiSummaryForGameAsync(Guid gameId, CancellationToken ct = default);
    Task<FoundingVerdictDto> SaveVerdictAsync(SaveFoundingVerdictRequest request, CancellationToken ct = default);
    Task DeleteVerdictAsync(Guid gameId, CancellationToken ct = default);
}
