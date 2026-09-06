using System;

namespace Ludeca.Core.Entities;

public class GameLoan
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string UserId { get; private set; } = string.Empty;
    public Guid GameId { get; private set; }
    public string BorrowerName { get; private set; } = string.Empty;
    public DateTimeOffset LoanDate { get; private set; }
    public string? Notes { get; private set; }
    public bool IsReturned { get; private set; }
    public DateTimeOffset? ReturnedDate { get; private set; }

    // Navigation property for EF Core
    public Game? Game { get; private set; }

    private GameLoan() { }

    public GameLoan(string userId, Guid gameId, string borrowerName, DateTimeOffset loanDate, string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("El identificador de usuario no puede estar vacío.", nameof(userId));
        if (gameId == Guid.Empty)
            throw new ArgumentException("El identificador del juego no puede ser un GUID vacío.", nameof(gameId));
        if (string.IsNullOrWhiteSpace(borrowerName))
            throw new ArgumentException("El nombre del prestatario o asociación no puede estar vacío.", nameof(borrowerName));

        UserId = userId.Trim();
        GameId = gameId;
        BorrowerName = borrowerName.Trim();
        LoanDate = loanDate;
        Notes = notes?.Trim();
        IsReturned = false;
        ReturnedDate = null;
    }

    public void MarkAsReturned(DateTimeOffset? returnDate = null)
    {
        IsReturned = true;
        ReturnedDate = returnDate ?? DateTimeOffset.UtcNow;
    }
}
