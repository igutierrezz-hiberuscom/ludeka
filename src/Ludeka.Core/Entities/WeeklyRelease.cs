using System;

namespace Ludeka.Core.Entities;

public class WeeklyRelease
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Title { get; private set; } = string.Empty;
    public string Publisher { get; private set; } = string.Empty;
    public DateOnly ReleaseDate { get; private set; }
    public Guid? GameId { get; private set; }
    public string? CoverImageUrl { get; private set; }
    public decimal? EstimatedPvp { get; private set; }
    public bool IsReprint { get; private set; }
    public string? Notes { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;

    public virtual Game? Game { get; private set; }

    private WeeklyRelease() { }

    public WeeklyRelease(
        string title,
        string publisher,
        DateOnly releaseDate,
        Guid? gameId = null,
        string? coverImageUrl = null,
        decimal? estimatedPvp = null,
        bool isReprint = false,
        string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("El título del lanzamiento no puede estar vacío.", nameof(title));

        if (string.IsNullOrWhiteSpace(publisher))
            throw new ArgumentException("La editorial del lanzamiento no puede estar vacía.", nameof(publisher));

        Title = title.Trim();
        Publisher = publisher.Trim();
        ReleaseDate = releaseDate;
        GameId = gameId;
        CoverImageUrl = string.IsNullOrWhiteSpace(coverImageUrl) ? null : coverImageUrl.Trim();
        EstimatedPvp = estimatedPvp;
        IsReprint = isReprint;
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        CreatedAt = DateTimeOffset.UtcNow;
    }
}
