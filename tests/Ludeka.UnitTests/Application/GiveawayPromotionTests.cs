using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ludeka.Application.Contracts;
using Ludeka.Application.Features.Community;
using Ludeka.Core.Entities;
using Ludeka.Core.Enums;
using Xunit;

namespace Ludeka.UnitTests.Application;

public class GiveawayPromotionTests
{
    private class InMemoryGiveawayRepository : IGiveawayRepository
    {
        public List<Giveaway> Items { get; } = new();

        public Task<IReadOnlyList<Giveaway>> GetGiveawaysAsync(bool includeExpired = false, CancellationToken ct = default)
        {
            var query = Items.AsEnumerable();
            if (!includeExpired)
            {
                query = query.Where(g => !g.IsExpired);
            }
            return Task.FromResult<IReadOnlyList<Giveaway>>(query.ToList());
        }

        public Task<Giveaway?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return Task.FromResult(Items.FirstOrDefault(g => g.Id == id));
        }

        public Task<Giveaway?> FindDuplicateOrCollaborativeAsync(string title, string organizer, DateTimeOffset deadline, CancellationToken ct = default)
        {
            return Task.FromResult<Giveaway?>(null);
        }

        public Task AddAsync(Giveaway giveaway, CancellationToken ct = default)
        {
            Items.Add(giveaway);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Giveaway giveaway, CancellationToken ct = default)
        {
            var idx = Items.FindIndex(g => g.Id == giveaway.Id);
            if (idx >= 0) Items[idx] = giveaway;
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            Items.RemoveAll(g => g.Id == id);
            return Task.CompletedTask;
        }
    }

    private readonly InMemoryGiveawayRepository _repo;
    private readonly GiveawayService _service;

    public GiveawayPromotionTests()
    {
        _repo = new InMemoryGiveawayRepository();
        _service = new GiveawayService(_repo);
    }

    [Fact]
    public async Task SetPromotedAsync_UpdatesStateAndPersists()
    {
        // Arrange
        var giveaway = new Giveaway(
            "Sorteo Brass", "Maldito", "https://instagram.com/p/1",
            GiveawayPlatform.Instagram, DateTimeOffset.UtcNow.AddDays(5), isPromoted: false);

        await _repo.AddAsync(giveaway);

        // Act
        await _service.SetPromotedAsync(giveaway.Id, true);

        // Assert
        var saved = await _repo.GetByIdAsync(giveaway.Id);
        Assert.NotNull(saved);
        Assert.True(saved.IsPromoted);
    }

    [Fact]
    public async Task SetPromotedAsync_NonExistingId_ThrowsKeyNotFoundException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _service.SetPromotedAsync(Guid.NewGuid(), true));
    }

    [Fact]
    public async Task GetGiveawaysAsync_OrdersPromotedFirst_ThenByDeadline()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;

        var standardLate = new Giveaway("Standard 5d", "Org1", "https://url1", GiveawayPlatform.Instagram, now.AddDays(5), isPromoted: false);
        var standardSoon = new Giveaway("Standard 1d", "Org2", "https://url2", GiveawayPlatform.Instagram, now.AddDays(1), isPromoted: false);
        var promoLate = new Giveaway("Promo 4d", "Org3", "https://url3", GiveawayPlatform.Instagram, now.AddDays(4), isPromoted: true);
        var promoSoon = new Giveaway("Promo 2d", "Org4", "https://url4", GiveawayPlatform.Instagram, now.AddDays(2), isPromoted: true);

        await _repo.AddAsync(standardLate);
        await _repo.AddAsync(standardSoon);
        await _repo.AddAsync(promoLate);
        await _repo.AddAsync(promoSoon);

        // Act
        var result = await _service.GetGiveawaysAsync();

        // Assert
        Assert.Equal(4, result.Count);
        // Primero los promocionados ordenados por deadline
        Assert.Equal("Promo 2d", result[0].Title);
        Assert.True(result[0].IsPromoted);
        Assert.Equal("Promo 4d", result[1].Title);
        Assert.True(result[1].IsPromoted);
        // Luego los estándares ordenados por deadline
        Assert.Equal("Standard 1d", result[2].Title);
        Assert.False(result[2].IsPromoted);
        Assert.Equal("Standard 5d", result[3].Title);
        Assert.False(result[3].IsPromoted);
    }
}
