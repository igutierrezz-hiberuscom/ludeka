using System;
using Ludeca.Core.Enums;

namespace Ludeca.Core.Entities;

public class UserCollectionItem
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string UserId { get; private set; } = string.Empty;
    public Guid GameId { get; private set; }
    public CollectionStatus Status { get; private set; }
    public DateTimeOffset AddedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; private set; }

    // Navigation property for EF Core (opcional/lazy)
    public Game? Game { get; private set; }

    private UserCollectionItem() { }

    public UserCollectionItem(string userId, Guid gameId, CollectionStatus status)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("El identificador de usuario no puede estar vacío.", nameof(userId));
        if (gameId == Guid.Empty)
            throw new ArgumentException("El identificador del juego no puede ser un GUID vacío.", nameof(gameId));

        UserId = userId.Trim();
        GameId = gameId;
        Status = status;
        AddedAt = DateTimeOffset.UtcNow;
    }

    public void ChangeStatus(CollectionStatus newStatus)
    {
        if (Status == newStatus) return;

        Status = newStatus;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
