using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ludeka.Application.DTOs;

namespace Ludeka.Application.Contracts;

public interface IExpansionService
{
    Task<IReadOnlyList<ExpansionSummaryDto>> GetExpansionsForBaseGameAsync(Guid baseGameId, CancellationToken ct = default);
    Task<ParentGameSummaryDto?> GetParentGameForExpansionAsync(Guid expansionId, CancellationToken ct = default);
    Task<IReadOnlyList<ExpansionSynergyDto>> GetSynergiesForBaseGameAsync(Guid baseGameId, CancellationToken ct = default);
    Task<IReadOnlyList<ExpansionSynergyDto>> GetSynergiesForExpansionAsync(Guid expansionId, CancellationToken ct = default);
    Task<ExpansionMixerEvaluationDto> EvaluateMixerCombinationAsync(Guid baseGameId, IEnumerable<Guid> selectedExpansionIds, CancellationToken ct = default);
    Task<IReadOnlyList<ExpansionRecipeDto>> GetRecipesForBaseGameAsync(Guid baseGameId, CancellationToken ct = default);
}
