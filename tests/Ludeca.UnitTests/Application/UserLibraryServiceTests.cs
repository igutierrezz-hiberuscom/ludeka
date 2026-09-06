using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ludeca.Application.Contracts;
using Ludeca.Application.DTOs;
using Ludeca.Application.Features.Library;
using Ludeca.Core.Entities;
using Ludeca.Core.Enums;
using Ludeca.Core.ValueObjects;
using Xunit;

namespace Ludeca.UnitTests.Application;

public class UserLibraryServiceTests
{
    private class FakeCurrentUserService : ICurrentUserService
    {
        public string UserId { get; set; } = "test-user-1";
        public string UserName { get; set; } = "Test User";
        public IReadOnlyList<string> Roles { get; set; } = ["User"];
        public bool IsFoundingTeam => IsInRole("FoundingTeam");
        public bool IsInRole(string role) => Roles.Contains(role);
        public void SwitchRole(string role) => Roles = [role];
    }

    private class FakeCollectionRepo : IUserCollectionRepository
    {
        public List<UserCollectionItem> Items { get; } = [];

        public Task<UserCollectionItem?> GetByUserAndGameAsync(string userId, Guid gameId, CancellationToken ct = default) =>
            Task.FromResult(Items.FirstOrDefault(i => i.UserId == userId && i.GameId == gameId));

        public Task<List<UserCollectionItem>> GetByUserIdAsync(string userId, CollectionStatus? status = null, CancellationToken ct = default)
        {
            var res = Items.Where(i => i.UserId == userId);
            if (status.HasValue) res = res.Where(i => i.Status == status.Value);
            return Task.FromResult(res.ToList());
        }

        public Task<Dictionary<CollectionStatus, int>> GetCountsByStatusAsync(string userId, CancellationToken ct = default)
        {
            var dict = Items
                .Where(i => i.UserId == userId)
                .GroupBy(i => i.Status)
                .ToDictionary(g => g.Key, g => g.Count());
            return Task.FromResult(dict);
        }

        public Task AddAsync(UserCollectionItem item, CancellationToken ct = default)
        {
            Items.Add(item);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(UserCollectionItem item, CancellationToken ct = default)
        {
            var idx = Items.FindIndex(i => i.Id == item.Id);
            if (idx >= 0) Items[idx] = item;
            return Task.CompletedTask;
        }

        public Task RemoveAsync(UserCollectionItem item, CancellationToken ct = default)
        {
            Items.RemoveAll(i => i.Id == item.Id);
            return Task.CompletedTask;
        }
    }

    private class FakeLoanRepo : IGameLoanRepository
    {
        public List<GameLoan> Loans { get; } = [];

        public Task<GameLoan?> GetByIdAsync(Guid loanId, CancellationToken ct = default) =>
            Task.FromResult(Loans.FirstOrDefault(l => l.Id == loanId));

        public Task<GameLoan?> GetActiveLoanByUserAndGameAsync(string userId, Guid gameId, CancellationToken ct = default) =>
            Task.FromResult(Loans.FirstOrDefault(l => l.UserId == userId && l.GameId == gameId && !l.IsReturned));

        public Task<List<GameLoan>> GetActiveLoansByUserIdAsync(string userId, CancellationToken ct = default) =>
            Task.FromResult(Loans.Where(l => l.UserId == userId && !l.IsReturned).ToList());

        public Task<List<GameLoan>> GetLoanHistoryByUserIdAsync(string userId, CancellationToken ct = default) =>
            Task.FromResult(Loans.Where(l => l.UserId == userId).ToList());

        public Task<int> GetActiveLoansCountAsync(string userId, CancellationToken ct = default) =>
            Task.FromResult(Loans.Count(l => l.UserId == userId && !l.IsReturned));

        public Task AddAsync(GameLoan loan, CancellationToken ct = default)
        {
            Loans.Add(loan);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(GameLoan loan, CancellationToken ct = default)
        {
            var idx = Loans.FindIndex(l => l.Id == loan.Id);
            if (idx >= 0) Loans[idx] = loan;
            return Task.CompletedTask;
        }
    }

    private class FakeReviewRepo : IUserReviewRepository
    {
        public List<UserGameReview> Reviews { get; } = [];

        public Task<UserGameReview?> GetByUserAndGameAsync(string userId, Guid gameId, CancellationToken ct = default) =>
            Task.FromResult(Reviews.FirstOrDefault(r => r.UserId == userId && r.GameId == gameId));

        public Task<List<UserGameReview>> GetByGameIdAsync(Guid gameId, CancellationToken ct = default) =>
            Task.FromResult(Reviews.Where(r => r.GameId == gameId).ToList());

        public Task<double?> GetAverageScoreByGameIdAsync(Guid gameId, CancellationToken ct = default)
        {
            var gameReviews = Reviews.Where(r => r.GameId == gameId).ToList();
            if (gameReviews.Count == 0) return Task.FromResult<double?>(null);
            return Task.FromResult<double?>(gameReviews.Average(r => r.Score));
        }

        public Task AddAsync(UserGameReview review, CancellationToken ct = default)
        {
            Reviews.Add(review);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(UserGameReview review, CancellationToken ct = default)
        {
            var idx = Reviews.FindIndex(r => r.Id == review.Id);
            if (idx >= 0) Reviews[idx] = review;
            return Task.CompletedTask;
        }
    }

    private class FakeGameRepo : IGameRepository
    {
        public List<Game> Games { get; } = [];

        public Task<Game?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
            Task.FromResult(Games.FirstOrDefault(g => g.Id == id));

        public Task<Game?> GetBySlugAsync(string slug, CancellationToken ct = default) =>
            Task.FromResult(Games.FirstOrDefault(g => g.Slug == slug));

        public Task<Game?> GetByBggIdAsync(int bggId, CancellationToken ct = default) =>
            Task.FromResult(Games.FirstOrDefault(g => g.BggId == bggId));

        public Task<(IReadOnlyList<Game> Items, int TotalCount)> SearchAsync(GameFilterCriteria criteria, int page = 1, int pageSize = 20, CancellationToken ct = default) =>
            Task.FromResult(((IReadOnlyList<Game>)Games, Games.Count));

        public Task AddRangeAsync(IEnumerable<Game> games, CancellationToken ct = default)
        {
            Games.AddRange(games);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Game game, CancellationToken ct = default)
        {
            var idx = Games.FindIndex(g => g.Id == game.Id);
            if (idx >= 0) Games[idx] = game;
            return Task.CompletedTask;
        }

        public Task<bool> HasAnyAsync(CancellationToken ct = default) =>
            Task.FromResult(Games.Count > 0);
    }

    private static Game CreateTestGame()
    {
        return new Game(
            bggId: 1001,
            originalTitle: "Wingspan",
            spanishTitle: "Wingspan",
            designer: "Elizabeth Hargrave",
            publisher: "Maldito Games",
            yearPublished: 2019,
            coverImageUrl: "https://example.com/wingspan.jpg",
            thumbnailUrl: null,
            description: "Juego de aves",
            bggRating: 8.1,
            bggRank: 25,
            ludistRating: 0.0,
            confrontation: ConfrontationType.Competitive,
            style: GameStyle.Eurogame,
            isOfficialSolo: true,
            age: new AgeRating(10, 10),
            language: LanguageDependence.Low,
            footprint: TableFootprint.StandardTable,
            duration: new GameDuration(40, 70, 20)
        );
    }

    [Fact]
    public async Task SetCollectionStateAsync_ShouldAddState_WhenGameNotInCollection()
    {
        // Arrange
        var colRepo = new FakeCollectionRepo();
        var loanRepo = new FakeLoanRepo();
        var reviewRepo = new FakeReviewRepo();
        var gameRepo = new FakeGameRepo();
        var userSvc = new FakeCurrentUserService();
        var game = CreateTestGame();
        gameRepo.Games.Add(game);

        var svc = new UserLibraryService(colRepo, loanRepo, reviewRepo, gameRepo, userSvc);

        // Act
        var result = await svc.SetCollectionStateAsync(game.Id, CollectionStatus.InCollection);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(CollectionStatus.InCollection, result.Status);
        Assert.Single(colRepo.Items);
    }

    [Fact]
    public async Task SetCollectionStateAsync_ShouldToggleOff_WhenSameStatusClickedAgain()
    {
        // Arrange
        var colRepo = new FakeCollectionRepo();
        var loanRepo = new FakeLoanRepo();
        var reviewRepo = new FakeReviewRepo();
        var gameRepo = new FakeGameRepo();
        var userSvc = new FakeCurrentUserService();
        var game = CreateTestGame();
        gameRepo.Games.Add(game);

        var svc = new UserLibraryService(colRepo, loanRepo, reviewRepo, gameRepo, userSvc);
        await svc.SetCollectionStateAsync(game.Id, CollectionStatus.InCollection);

        // Act (toggle off)
        var result = await svc.SetCollectionStateAsync(game.Id, CollectionStatus.InCollection);

        // Assert
        Assert.Null(result);
        Assert.Empty(colRepo.Items);
    }

    [Fact]
    public async Task CreateLoanAsync_ShouldSucceed_WhenGameIsInCollection()
    {
        // Arrange
        var colRepo = new FakeCollectionRepo();
        var loanRepo = new FakeLoanRepo();
        var reviewRepo = new FakeReviewRepo();
        var gameRepo = new FakeGameRepo();
        var userSvc = new FakeCurrentUserService();
        var game = CreateTestGame();
        gameRepo.Games.Add(game);

        var svc = new UserLibraryService(colRepo, loanRepo, reviewRepo, gameRepo, userSvc);
        await svc.SetCollectionStateAsync(game.Id, CollectionStatus.InCollection);

        // Act
        var loanDto = await svc.CreateLoanAsync(new CreateLoanRequest(game.Id, "Carlos", DateTimeOffset.UtcNow, "Préstamo de prueba"));

        // Assert
        Assert.NotNull(loanDto);
        Assert.Equal("Carlos", loanDto.BorrowerName);
        Assert.False(loanDto.IsReturned);
        Assert.Single(loanRepo.Loans);
    }

    [Fact]
    public async Task CreateLoanAsync_ShouldThrowInvalidOperation_WhenGameIsNotInCollection()
    {
        // Arrange
        var colRepo = new FakeCollectionRepo();
        var loanRepo = new FakeLoanRepo();
        var reviewRepo = new FakeReviewRepo();
        var gameRepo = new FakeGameRepo();
        var userSvc = new FakeCurrentUserService();
        var game = CreateTestGame();
        gameRepo.Games.Add(game);

        var svc = new UserLibraryService(colRepo, loanRepo, reviewRepo, gameRepo, userSvc);
        await svc.SetCollectionStateAsync(game.Id, CollectionStatus.Wishlist); // No está en InCollection

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            svc.CreateLoanAsync(new CreateLoanRequest(game.Id, "Carlos", DateTimeOffset.UtcNow)));
    }

    [Fact]
    public async Task ReturnLoanAsync_ShouldMarkAsReturned()
    {
        // Arrange
        var colRepo = new FakeCollectionRepo();
        var loanRepo = new FakeLoanRepo();
        var reviewRepo = new FakeReviewRepo();
        var gameRepo = new FakeGameRepo();
        var userSvc = new FakeCurrentUserService();
        var game = CreateTestGame();
        gameRepo.Games.Add(game);

        var svc = new UserLibraryService(colRepo, loanRepo, reviewRepo, gameRepo, userSvc);
        await svc.SetCollectionStateAsync(game.Id, CollectionStatus.InCollection);
        var loan = await svc.CreateLoanAsync(new CreateLoanRequest(game.Id, "Marta", DateTimeOffset.UtcNow));

        // Act
        var returned = await svc.ReturnLoanAsync(loan.Id);

        // Assert
        Assert.True(returned.IsReturned);
        Assert.NotNull(returned.ReturnedDate);
    }

    [Fact]
    public async Task SubmitReviewAsync_ShouldCreateReviewAndRecalculateLudistRating()
    {
        // Arrange
        var colRepo = new FakeCollectionRepo();
        var loanRepo = new FakeLoanRepo();
        var reviewRepo = new FakeReviewRepo();
        var gameRepo = new FakeGameRepo();
        var userSvc = new FakeCurrentUserService();
        var game = CreateTestGame();
        gameRepo.Games.Add(game);

        var svc = new UserLibraryService(colRepo, loanRepo, reviewRepo, gameRepo, userSvc);

        // Act
        var review = await svc.SubmitReviewAsync(new SubmitReviewRequest(
            game.Id,
            9.0,
            "Gran experiencia con el motor de cartas.",
            [new UserPlayerCountVote(2, ScalabilityStatus.MustPlay)],
            new UserFamilyExperienceVote(true, 10, false),
            PlayContextType.Owned
        ));

        // Assert
        Assert.NotNull(review);
        Assert.Equal(9.0, review.Score);
        Assert.Equal(9.0, game.LudistRating); // Consenso actualizado
    }
}
