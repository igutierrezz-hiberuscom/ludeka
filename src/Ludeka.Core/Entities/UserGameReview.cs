using System;
using System.Collections.Generic;
using Ludeka.Core.Enums;
using Ludeka.Core.ValueObjects;

namespace Ludeka.Core.Entities;

public class UserGameReview
{
    public const int MaxMicroReviewLength = 280;

    public Guid Id { get; private set; } = Guid.NewGuid();
    public string UserId { get; private set; } = string.Empty;
    public Guid GameId { get; private set; }
    public double Score { get; private set; }
    public string? MicroReview { get; private set; }
    public List<UserPlayerCountVote> PlayerCountRatings { get; private set; } = [];
    public UserFamilyExperienceVote? FamilyExperience { get; private set; }
    public PlayContextType? PlayContext { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; private set; }

    // Navigation property for EF Core
    public Game? Game { get; private set; }

    private UserGameReview() { }

    public UserGameReview(
        string userId,
        Guid gameId,
        double score,
        string? microReview = null,
        IEnumerable<UserPlayerCountVote>? playerCountRatings = null,
        UserFamilyExperienceVote? familyExperience = null,
        PlayContextType? playContext = null)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("El identificador de usuario no puede estar vacío.", nameof(userId));
        if (gameId == Guid.Empty)
            throw new ArgumentException("El identificador del juego no puede ser un GUID vacío.", nameof(gameId));
        if (score < 1.0 || score > 10.0)
            throw new ArgumentOutOfRangeException(nameof(score), "La puntuación debe estar comprendida entre 1.0 y 10.0.");
        if (microReview != null && microReview.Length > MaxMicroReviewLength)
            throw new ArgumentException($"La micro-reseña no puede superar los {MaxMicroReviewLength} caracteres.", nameof(microReview));

        UserId = userId.Trim();
        GameId = gameId;
        Score = Math.Round(score, 1);
        MicroReview = string.IsNullOrWhiteSpace(microReview) ? null : microReview.Trim();
        FamilyExperience = familyExperience;
        PlayContext = playContext;
        CreatedAt = DateTimeOffset.UtcNow;

        if (playerCountRatings != null)
        {
            PlayerCountRatings.AddRange(playerCountRatings);
        }
    }

    public void Update(
        double score,
        string? microReview,
        IEnumerable<UserPlayerCountVote>? playerCountRatings,
        UserFamilyExperienceVote? familyExperience,
        PlayContextType? playContext)
    {
        if (score < 1.0 || score > 10.0)
            throw new ArgumentOutOfRangeException(nameof(score), "La puntuación debe estar comprendida entre 1.0 y 10.0.");
        if (microReview != null && microReview.Length > MaxMicroReviewLength)
            throw new ArgumentException($"La micro-reseña no puede superar los {MaxMicroReviewLength} caracteres.", nameof(microReview));

        Score = Math.Round(score, 1);
        MicroReview = string.IsNullOrWhiteSpace(microReview) ? null : microReview.Trim();
        FamilyExperience = familyExperience;
        PlayContext = playContext;
        UpdatedAt = DateTimeOffset.UtcNow;

        PlayerCountRatings.Clear();
        if (playerCountRatings != null)
        {
            PlayerCountRatings.AddRange(playerCountRatings);
        }
    }
}
