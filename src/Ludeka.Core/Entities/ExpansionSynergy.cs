using System;
using Ludeka.Core.Enums;

namespace Ludeka.Core.Entities;

public class ExpansionSynergy
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid BaseGameId { get; private set; }
    public Guid ExpansionAId { get; private set; }
    public Guid ExpansionBId { get; private set; }
    public ExpansionSynergyLevel Level { get; private set; }
    public string Reason { get; private set; } = string.Empty;

    // Propiedades de navegación opcionales para EF Core
    public Game? BaseGame { get; private set; }
    public Game? ExpansionA { get; private set; }
    public Game? ExpansionB { get; private set; }

    // Constructor privado para EF Core
    private ExpansionSynergy() { }

    public ExpansionSynergy(
        Guid baseGameId,
        Guid expansionAId,
        Guid expansionBId,
        ExpansionSynergyLevel level,
        string reason)
    {
        if (baseGameId == Guid.Empty) throw new ArgumentException("El BaseGameId no puede estar vacío.", nameof(baseGameId));
        if (expansionAId == Guid.Empty || expansionBId == Guid.Empty) throw new ArgumentException("Los identificadores de expansión no pueden estar vacíos.");
        if (expansionAId == expansionBId) throw new ArgumentException("Una expansión no puede tener sinergia consigo misma.");

        BaseGameId = baseGameId;
        ExpansionAId = expansionAId;
        ExpansionBId = expansionBId;
        Level = level;
        Reason = reason?.Trim() ?? string.Empty;
    }

    public bool MatchesPair(Guid id1, Guid id2) =>
        (ExpansionAId == id1 && ExpansionBId == id2) || (ExpansionAId == id2 && ExpansionBId == id1);
}
