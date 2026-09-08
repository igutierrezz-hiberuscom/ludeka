using System;
using System.Collections.Generic;
using System.Linq;

namespace Ludeka.Application.DTOs;

public record OfflineLibrarySnapshotDto(
    string UserId,
    DateTimeOffset Timestamp,
    int TotalInCollection,
    int TotalPlayed,
    int TotalWishlist,
    int TotalWantToBuy,
    int TotalActiveLoans,
    List<UserCollectionItemDto> Items,
    List<GameLoanDto> ActiveLoans
)
{
    public static OfflineLibrarySnapshotDto FromSummary(string userId, UserLibrarySummaryDto summary)
    {
        return new OfflineLibrarySnapshotDto(
            userId,
            DateTimeOffset.UtcNow,
            summary.TotalInCollection,
            summary.TotalPlayed,
            summary.TotalWishlist,
            summary.TotalWantToBuy,
            summary.TotalActiveLoans,
            summary.Items?.ToList() ?? [],
            summary.ActiveLoans?.ToList() ?? []
        );
    }

    public UserLibrarySummaryDto ToSummary()
    {
        return new UserLibrarySummaryDto(
            TotalInCollection,
            TotalPlayed,
            TotalWishlist,
            TotalWantToBuy,
            TotalActiveLoans,
            Items ?? [],
            ActiveLoans ?? []
        );
    }
}
