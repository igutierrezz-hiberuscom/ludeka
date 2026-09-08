using System;
using System.Collections.Generic;
using Ludeka.Core.Enums;
using Ludeka.Core.ValueObjects;

namespace Ludeka.Application.DTOs;

public record UserCollectionItemDto(
    Guid Id,
    Guid? GameId,
    string GameTitle,
    string? GameCoverUrl,
    string GameSlug,
    CollectionStatus? Status,
    bool IsPlayed,
    DateTimeOffset AddedAt,
    bool IsCurrentlyLoaned,
    int? BggId = null,
    bool IsPendingCataloging = false,
    bool IsExpansion = false
);

public record GameLoanDto(
    Guid Id,
    Guid GameId,
    string GameTitle,
    string? GameCoverUrl,
    string GameSlug,
    string BorrowerName,
    DateTimeOffset LoanDate,
    string? Notes,
    bool IsReturned,
    DateTimeOffset? ReturnedDate
);

public record UserReviewDto(
    Guid Id,
    Guid GameId,
    double Score,
    string? MicroReview,
    List<UserPlayerCountVote> PlayerCountRatings,
    UserFamilyExperienceVote? FamilyExperience,
    PlayContextType? PlayContext,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
);

public record UserLibrarySummaryDto(
    int TotalInCollection,
    int TotalPlayed,
    int TotalWishlist,
    int TotalWantToBuy,
    int TotalActiveLoans,
    List<UserCollectionItemDto> Items,
    List<GameLoanDto> ActiveLoans
);

public record SetCollectionStatusRequest(
    Guid GameId,
    CollectionStatus? Status
);

public record CreateLoanRequest(
    Guid GameId,
    string BorrowerName,
    DateTimeOffset LoanDate,
    string? Notes = null
);

public record SubmitReviewRequest(
    Guid GameId,
    double Score,
    string? MicroReview,
    List<UserPlayerCountVote>? PlayerCountVotes = null,
    UserFamilyExperienceVote? FamilyExperience = null,
    PlayContextType? PlayContext = null
);
