using System;
using System.Collections.Generic;

namespace Ludeka.Core.Entities;

public class ExpansionRecipe
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid BaseGameId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string IdealFor { get; private set; } = string.Empty;
    public List<Guid> IncludedExpansionIds { get; private set; } = [];

    // Propiedad de navegación opcional para EF Core
    public Game? BaseGame { get; private set; }

    // Constructor privado para EF Core
    private ExpansionRecipe() { }

    public ExpansionRecipe(
        Guid baseGameId,
        string name,
        string description,
        string idealFor,
        IEnumerable<Guid>? includedExpansionIds = null)
    {
        if (baseGameId == Guid.Empty) throw new ArgumentException("El BaseGameId no puede estar vacío.", nameof(baseGameId));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("El nombre de la receta no puede estar vacío.", nameof(name));

        BaseGameId = baseGameId;
        Name = name.Trim();
        Description = description?.Trim() ?? string.Empty;
        IdealFor = idealFor?.Trim() ?? string.Empty;

        if (includedExpansionIds != null)
        {
            IncludedExpansionIds.AddRange(includedExpansionIds);
        }
    }
}
