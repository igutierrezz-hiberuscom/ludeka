using System;
using System.Collections.Generic;
using System.Linq;
using Ludeka.Core.Enums;
using Ludeka.Core.ValueObjects;

namespace Ludeka.Core.Entities;

public class FoundingVerdict
{
    public const int MaxPhotosCount = 3;

    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid GameId { get; private set; }
    public string AuthorUserId { get; private set; } = string.Empty;
    public string AuthorName { get; private set; } = string.Empty;
    public FoundingRecommendation Recommendation { get; private set; }
    public string OverallVerdict { get; private set; } = string.Empty;
    public string TwoPlayerVerdict { get; private set; } = string.Empty;
    public string FamilyVerdict { get; private set; } = string.Empty;
    public List<FoundingPhoto> Photos { get; private set; } = [];
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; private set; } = DateTimeOffset.UtcNow;

    // Constructor privado para EF Core
    private FoundingVerdict() { }

    public FoundingVerdict(
        Guid gameId,
        string authorUserId,
        string authorName,
        FoundingRecommendation recommendation,
        string overallVerdict,
        string twoPlayerVerdict,
        string familyVerdict,
        IEnumerable<FoundingPhoto>? photos = null)
    {
        if (gameId == Guid.Empty)
            throw new ArgumentException("El GameId no puede ser vacío.", nameof(gameId));
        if (string.IsNullOrWhiteSpace(authorUserId))
            throw new ArgumentException("El ID del autor fundador es obligatorio.", nameof(authorUserId));
        if (string.IsNullOrWhiteSpace(authorName))
            throw new ArgumentException("El nombre del autor fundador es obligatorio.", nameof(authorName));

        GameId = gameId;
        AuthorUserId = authorUserId.Trim();
        AuthorName = authorName.Trim();
        Recommendation = recommendation;

        ValidateAndSetVerdicts(overallVerdict, twoPlayerVerdict, familyVerdict);

        if (photos != null)
        {
            foreach (var photo in photos)
            {
                AddPhoto(photo);
            }
        }
    }

    public void Update(
        FoundingRecommendation recommendation,
        string overallVerdict,
        string twoPlayerVerdict,
        string familyVerdict,
        IEnumerable<FoundingPhoto>? photos = null)
    {
        Recommendation = recommendation;
        ValidateAndSetVerdicts(overallVerdict, twoPlayerVerdict, familyVerdict);

        Photos.Clear();
        if (photos != null)
        {
            foreach (var photo in photos)
            {
                AddPhoto(photo);
            }
        }

        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void AddPhoto(FoundingPhoto photo)
    {
        ArgumentNullException.ThrowIfNull(photo);

        if (Photos.Count >= MaxPhotosCount)
            throw new InvalidOperationException($"No se pueden adjuntar más de {MaxPhotosCount} fotos reales de partida.");

        Photos.Add(photo);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void RemovePhoto(int index)
    {
        if (index >= 0 && index < Photos.Count)
        {
            Photos.RemoveAt(index);
            UpdatedAt = DateTimeOffset.UtcNow;
        }
    }

    public double GetEquivalentRating() => Recommendation switch
    {
        FoundingRecommendation.MustPlay => 10.0,
        FoundingRecommendation.RecommendedWithAdaptations => 7.5,
        FoundingRecommendation.Skippable => 4.0,
        _ => 7.0
    };

    private void ValidateAndSetVerdicts(string overallVerdict, string twoPlayerVerdict, string familyVerdict)
    {
        if (string.IsNullOrWhiteSpace(overallVerdict) || overallVerdict.Trim().Length < 10)
            throw new ArgumentException("El análisis general de la casa debe contener al menos 10 caracteres.", nameof(overallVerdict));

        if (string.IsNullOrWhiteSpace(twoPlayerVerdict) || twoPlayerVerdict.Trim().Length < 10)
            throw new ArgumentException("El análisis enfocado a 2 jugadores (pareja) debe contener al menos 10 caracteres.", nameof(twoPlayerVerdict));

        if (string.IsNullOrWhiteSpace(familyVerdict) || familyVerdict.Trim().Length < 10)
            throw new ArgumentException("El análisis enfocado a familias/niños debe contener al menos 10 caracteres.", nameof(familyVerdict));

        OverallVerdict = overallVerdict.Trim();
        TwoPlayerVerdict = twoPlayerVerdict.Trim();
        FamilyVerdict = familyVerdict.Trim();
    }
}
