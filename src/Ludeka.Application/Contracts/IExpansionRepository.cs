using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ludeka.Core.Entities;

namespace Ludeka.Application.Contracts;

public interface IExpansionRepository
{
    Task<IReadOnlyList<Game>> GetExpansionsByBaseGameIdAsync(Guid baseGameId, CancellationToken ct = default);
    Task<Game?> GetExpansionWithBaseGameAsync(Guid expansionId, CancellationToken ct = default);
    Task<IReadOnlyList<ExpansionSynergy>> GetSynergiesByBaseGameIdAsync(Guid baseGameId, CancellationToken ct = default);
    Task<IReadOnlyList<ExpansionRecipe>> GetRecipesByBaseGameIdAsync(Guid baseGameId, CancellationToken ct = default);
    Task AddSynergyAsync(ExpansionSynergy synergy, CancellationToken ct = default);
    Task AddRecipeAsync(ExpansionRecipe recipe, CancellationToken ct = default);
}
