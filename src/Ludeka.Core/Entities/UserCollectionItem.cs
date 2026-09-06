using System;
using Ludeka.Core.Enums;

namespace Ludeka.Core.Entities;

public class UserCollectionItem
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string UserId { get; private set; } = string.Empty;
    public Guid? GameId { get; private set; }
    public CollectionStatus Status { get; private set; }
    public DateTimeOffset AddedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; private set; }

    // Propiedades provisionales para títulos en cola de catalogación
    public int? BggId { get; private set; }
    public string? PendingTitle { get; private set; }
    public string? PendingThumbnailUrl { get; private set; }
    public int? PendingYearPublished { get; private set; }

    public bool IsPendingCataloging => GameId == null;

    // Navigation property for EF Core (opcional/lazy)
    public Game? Game { get; private set; }

    private UserCollectionItem() { }

    /// <summary>
    /// Constructor para juegos ya catalogados en Ludeka.
    /// </summary>
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

    /// <summary>
    /// Constructor para juegos importados de BGG que aún están en cola de auto-catalogación.
    /// </summary>
    public UserCollectionItem(
        string userId,
        int bggId,
        string pendingTitle,
        CollectionStatus status,
        string? thumbnailUrl = null,
        int? yearPublished = null)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("El identificador de usuario no puede estar vacío.", nameof(userId));
        if (bggId <= 0)
            throw new ArgumentException("El identificador BGG debe ser mayor a cero.", nameof(bggId));
        if (string.IsNullOrWhiteSpace(pendingTitle))
            throw new ArgumentException("El título provisional del juego no puede estar vacío.", nameof(pendingTitle));

        UserId = userId.Trim();
        GameId = null;
        BggId = bggId;
        PendingTitle = pendingTitle.Trim();
        Status = status;
        PendingThumbnailUrl = thumbnailUrl?.Trim();
        PendingYearPublished = yearPublished;
        AddedAt = DateTimeOffset.UtcNow;
    }

    public void ChangeStatus(CollectionStatus newStatus)
    {
        if (Status == newStatus) return;

        Status = newStatus;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Promociona el ítem a catalogado una vez que se ha creado la entidad Game en el catálogo.
    /// </summary>
    public void PromoteToCataloged(Guid gameId)
    {
        if (gameId == Guid.Empty)
            throw new ArgumentException("El identificador del juego catalogado no puede ser un GUID vacío.", nameof(gameId));

        GameId = gameId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
