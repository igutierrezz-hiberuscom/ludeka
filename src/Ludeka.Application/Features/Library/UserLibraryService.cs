using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ludeka.Application.Contracts;
using Ludeka.Application.DTOs;
using Ludeka.Core.Entities;
using Ludeka.Core.Enums;

namespace Ludeka.Application.Features.Library;

public class UserLibraryService : IUserLibraryService
{
    private readonly IUserCollectionRepository _collectionRepo;
    private readonly IGameLoanRepository _loanRepo;
    private readonly IUserReviewRepository _reviewRepo;
    private readonly IGameRepository _gameRepo;
    private readonly ICurrentUserService _currentUserService;

    public UserLibraryService(
        IUserCollectionRepository collectionRepo,
        IGameLoanRepository loanRepo,
        IUserReviewRepository reviewRepo,
        IGameRepository gameRepo,
        ICurrentUserService currentUserService)
    {
        _collectionRepo = collectionRepo;
        _loanRepo = loanRepo;
        _reviewRepo = reviewRepo;
        _gameRepo = gameRepo;
        _currentUserService = currentUserService;
    }

    public async Task<UserCollectionItemDto?> GetCollectionStateAsync(Guid gameId, CancellationToken ct = default)
    {
        string userId = _currentUserService.UserId;
        var item = await _collectionRepo.GetByUserAndGameAsync(userId, gameId, ct);
        if (item == null) return null;

        var activeLoan = await _loanRepo.GetActiveLoanByUserAndGameAsync(userId, gameId, ct);
        var game = item.Game ?? await _gameRepo.GetByIdAsync(gameId, ct);

        return new UserCollectionItemDto(
            item.Id,
            item.GameId,
            game?.SpanishTitle ?? "Juego",
            game?.CoverImageUrl,
            game?.Slug ?? string.Empty,
            item.Status,
            item.AddedAt,
            activeLoan != null
        );
    }

    public async Task<UserCollectionItemDto?> SetCollectionStateAsync(Guid gameId, CollectionStatus? status, CancellationToken ct = default)
    {
        string userId = _currentUserService.UserId;
        var existing = await _collectionRepo.GetByUserAndGameAsync(userId, gameId, ct);

        if (!status.HasValue)
        {
            if (existing != null)
            {
                await _collectionRepo.RemoveAsync(existing, ct);
            }
            return null;
        }

        if (existing != null)
        {
            // Toggle: si se vuelve a pulsar el mismo estado, se desmarca
            if (existing.Status == status.Value)
            {
                await _collectionRepo.RemoveAsync(existing, ct);
                return null;
            }

            existing.ChangeStatus(status.Value);
            await _collectionRepo.UpdateAsync(existing, ct);
            return await MapCollectionItemAsync(existing, ct);
        }

        var game = await _gameRepo.GetByIdAsync(gameId, ct);
        if (game == null)
        {
            throw new KeyNotFoundException($"No se encontró el juego con ID {gameId}.");
        }

        var newItem = new UserCollectionItem(userId, gameId, status.Value);
        await _collectionRepo.AddAsync(newItem, ct);

        return await MapCollectionItemAsync(newItem, ct);
    }

    public async Task<GameLoanDto?> GetActiveLoanAsync(Guid gameId, CancellationToken ct = default)
    {
        string userId = _currentUserService.UserId;
        var loan = await _loanRepo.GetActiveLoanByUserAndGameAsync(userId, gameId, ct);
        if (loan == null) return null;

        var game = loan.Game ?? await _gameRepo.GetByIdAsync(gameId, ct);

        return new GameLoanDto(
            loan.Id,
            loan.GameId,
            game?.SpanishTitle ?? "Juego",
            game?.CoverImageUrl,
            game?.Slug ?? string.Empty,
            loan.BorrowerName,
            loan.LoanDate,
            loan.Notes,
            loan.IsReturned,
            loan.ReturnedDate
        );
    }

    public async Task<GameLoanDto> CreateLoanAsync(CreateLoanRequest request, CancellationToken ct = default)
    {
        string userId = _currentUserService.UserId;

        // Invariante de negocio: solo se pueden prestar juegos que estén en la ludoteca propia (InCollection)
        var collectionItem = await _collectionRepo.GetByUserAndGameAsync(userId, request.GameId, ct);
        if (collectionItem == null || collectionItem.Status != CollectionStatus.InCollection)
        {
            throw new InvalidOperationException("Solo puedes registrar préstamos de juegos que estén en tu ludoteca física propia («En mi ludoteca»).");
        }

        var activeLoan = await _loanRepo.GetActiveLoanByUserAndGameAsync(userId, request.GameId, ct);
        if (activeLoan != null)
        {
            throw new InvalidOperationException("Este juego ya se encuentra registrado como prestado actualmente.");
        }

        var loan = new GameLoan(userId, request.GameId, request.BorrowerName, request.LoanDate, request.Notes);
        await _loanRepo.AddAsync(loan, ct);

        var game = await _gameRepo.GetByIdAsync(request.GameId, ct);

        return new GameLoanDto(
            loan.Id,
            loan.GameId,
            game?.SpanishTitle ?? "Juego",
            game?.CoverImageUrl,
            game?.Slug ?? string.Empty,
            loan.BorrowerName,
            loan.LoanDate,
            loan.Notes,
            loan.IsReturned,
            loan.ReturnedDate
        );
    }

    public async Task<GameLoanDto> ReturnLoanAsync(Guid loanId, CancellationToken ct = default)
    {
        string userId = _currentUserService.UserId;
        var loan = await _loanRepo.GetByIdAsync(loanId, ct);
        if (loan == null || !string.Equals(loan.UserId, userId, StringComparison.OrdinalIgnoreCase))
        {
            throw new KeyNotFoundException("Préstamo no encontrado.");
        }

        loan.MarkAsReturned();
        await _loanRepo.UpdateAsync(loan, ct);

        var game = loan.Game ?? await _gameRepo.GetByIdAsync(loan.GameId, ct);

        return new GameLoanDto(
            loan.Id,
            loan.GameId,
            game?.SpanishTitle ?? "Juego",
            game?.CoverImageUrl,
            game?.Slug ?? string.Empty,
            loan.BorrowerName,
            loan.LoanDate,
            loan.Notes,
            loan.IsReturned,
            loan.ReturnedDate
        );
    }

    public async Task<UserReviewDto?> GetUserReviewAsync(Guid gameId, CancellationToken ct = default)
    {
        string userId = _currentUserService.UserId;
        var review = await _reviewRepo.GetByUserAndGameAsync(userId, gameId, ct);
        if (review == null) return null;

        return MapReview(review);
    }

    public async Task<UserReviewDto> SubmitReviewAsync(SubmitReviewRequest request, CancellationToken ct = default)
    {
        string userId = _currentUserService.UserId;
        var review = await _reviewRepo.GetByUserAndGameAsync(userId, request.GameId, ct);

        if (review != null)
        {
            review.Update(
                request.Score,
                request.MicroReview,
                request.PlayerCountVotes,
                request.FamilyExperience,
                request.PlayContext
            );
            await _reviewRepo.UpdateAsync(review, ct);
        }
        else
        {
            review = new UserGameReview(
                userId,
                request.GameId,
                request.Score,
                request.MicroReview,
                request.PlayerCountVotes,
                request.FamilyExperience,
                request.PlayContext
            );
            await _reviewRepo.AddAsync(review, ct);
        }

        // Recálculo dinámico del consenso local LudistRating
        var avgScore = await _reviewRepo.GetAverageScoreByGameIdAsync(request.GameId, ct);
        if (avgScore.HasValue)
        {
            var game = await _gameRepo.GetByIdAsync(request.GameId, ct);
            if (game != null)
            {
                game.UpdateLudistRating(avgScore.Value);
                await _gameRepo.UpdateAsync(game, ct);
            }
        }

        return MapReview(review);
    }

    public async Task<UserLibrarySummaryDto> GetLibrarySummaryAsync(CancellationToken ct = default)
    {
        string userId = _currentUserService.UserId;

        var counts = await _collectionRepo.GetCountsByStatusAsync(userId, ct);
        var activeLoansCount = await _loanRepo.GetActiveLoansCountAsync(userId, ct);

        var collectionItems = await _collectionRepo.GetByUserIdAsync(userId, null, ct);
        var activeLoans = await _loanRepo.GetActiveLoansByUserIdAsync(userId, ct);

        var activeLoanGameIds = activeLoans.Select(l => l.GameId).ToHashSet();

        var itemDtos = new List<UserCollectionItemDto>();
        foreach (var item in collectionItems)
        {
            var game = item.Game ?? await _gameRepo.GetByIdAsync(item.GameId, ct);
            itemDtos.Add(new UserCollectionItemDto(
                item.Id,
                item.GameId,
                game?.SpanishTitle ?? "Juego",
                game?.CoverImageUrl,
                game?.Slug ?? string.Empty,
                item.Status,
                item.AddedAt,
                activeLoanGameIds.Contains(item.GameId)
            ));
        }

        var loanDtos = new List<GameLoanDto>();
        foreach (var loan in activeLoans)
        {
            var game = loan.Game ?? await _gameRepo.GetByIdAsync(loan.GameId, ct);
            loanDtos.Add(new GameLoanDto(
                loan.Id,
                loan.GameId,
                game?.SpanishTitle ?? "Juego",
                game?.CoverImageUrl,
                game?.Slug ?? string.Empty,
                loan.BorrowerName,
                loan.LoanDate,
                loan.Notes,
                loan.IsReturned,
                loan.ReturnedDate
            ));
        }

        return new UserLibrarySummaryDto(
            counts.GetValueOrDefault(CollectionStatus.InCollection, 0),
            counts.GetValueOrDefault(CollectionStatus.Played, 0),
            counts.GetValueOrDefault(CollectionStatus.Wishlist, 0),
            counts.GetValueOrDefault(CollectionStatus.WantToBuy, 0),
            activeLoansCount,
            itemDtos,
            loanDtos
        );
    }

    private async Task<UserCollectionItemDto> MapCollectionItemAsync(UserCollectionItem item, CancellationToken ct)
    {
        var game = item.Game ?? await _gameRepo.GetByIdAsync(item.GameId, ct);
        var activeLoan = await _loanRepo.GetActiveLoanByUserAndGameAsync(item.UserId, item.GameId, ct);

        return new UserCollectionItemDto(
            item.Id,
            item.GameId,
            game?.SpanishTitle ?? "Juego",
            game?.CoverImageUrl,
            game?.Slug ?? string.Empty,
            item.Status,
            item.AddedAt,
            activeLoan != null
        );
    }

    private static UserReviewDto MapReview(UserGameReview review)
    {
        return new UserReviewDto(
            review.Id,
            review.GameId,
            review.Score,
            review.MicroReview,
            review.PlayerCountRatings.ToList(),
            review.FamilyExperience,
            review.PlayContext,
            review.CreatedAt,
            review.UpdatedAt
        );
    }
}
