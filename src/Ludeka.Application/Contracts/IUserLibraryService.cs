using System;
using System.Threading;
using System.Threading.Tasks;
using Ludeka.Application.DTOs;
using Ludeka.Core.Enums;

namespace Ludeka.Application.Contracts;

public interface IUserLibraryService
{
    Task<UserCollectionItemDto?> GetCollectionStateAsync(Guid gameId, CancellationToken ct = default);
    Task<UserCollectionItemDto?> SetCollectionStateAsync(Guid gameId, CollectionStatus? status, CancellationToken ct = default);
    Task<GameLoanDto?> GetActiveLoanAsync(Guid gameId, CancellationToken ct = default);
    Task<GameLoanDto> CreateLoanAsync(CreateLoanRequest request, CancellationToken ct = default);
    Task<GameLoanDto> ReturnLoanAsync(Guid loanId, CancellationToken ct = default);
    Task<UserReviewDto?> GetUserReviewAsync(Guid gameId, CancellationToken ct = default);
    Task<UserReviewDto> SubmitReviewAsync(SubmitReviewRequest request, CancellationToken ct = default);
    Task<UserLibrarySummaryDto> GetLibrarySummaryAsync(CancellationToken ct = default);
}
