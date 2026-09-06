using System;
using Ludeka.Core.Enums;

namespace Ludeka.Core.ValueObjects;

public record UserPlayerCountVote
{
    public int PlayerCount { get; init; }
    public ScalabilityStatus Status { get; init; }

    // Constructor sin parámetros para deserialización y EF Core
    public UserPlayerCountVote() { }

    public UserPlayerCountVote(int playerCount, ScalabilityStatus status)
    {
        if (playerCount < 1)
            throw new ArgumentOutOfRangeException(nameof(playerCount), "El número de jugadores debe ser al menos 1.");

        PlayerCount = playerCount;
        Status = status;
    }

    public string DisplayText => PlayerCount >= 7 ? "7+J" : $"{PlayerCount}J";
}
