namespace Ludeca.Core.ValueObjects;

public record GameDuration(int MinMinutes, int MaxMinutes, int EstimatedPerPlayerMinutes)
{
    public string FormatForPlayerCount(int playerCount)
    {
        if (playerCount <= 0) return $"{MinMinutes}–{MaxMinutes} min";
        int calculated = EstimatedPerPlayerMinutes * playerCount;
        return $"~{calculated} min a {playerCount} jugadores";
    }

    public string SummaryText => MinMinutes == MaxMinutes
        ? $"{MinMinutes} min"
        : $"{MinMinutes}–{MaxMinutes} min (~{EstimatedPerPlayerMinutes} min/jugador)";
}
