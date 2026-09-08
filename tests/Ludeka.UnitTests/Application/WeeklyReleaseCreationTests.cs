using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ludeka.Application.Contracts;
using Ludeka.Application.DTOs;
using Ludeka.Application.Features.Community;
using Ludeka.Core.Entities;
using Xunit;

namespace Ludeka.UnitTests.Application;

public class WeeklyReleaseCreationTests
{
    private class InMemoryWeeklyReleaseRepository : IWeeklyReleaseRepository
    {
        public List<WeeklyRelease> Items { get; } = new();

        public Task<IReadOnlyList<WeeklyRelease>> GetReleasesAsync(DateOnly? fromDate = null, CancellationToken ct = default)
        {
            var query = Items.AsEnumerable();
            if (fromDate.HasValue)
            {
                query = query.Where(r => r.ReleaseDate >= fromDate.Value);
            }
            return Task.FromResult<IReadOnlyList<WeeklyRelease>>(query.OrderByDescending(r => r.ReleaseDate).ToList());
        }

        public Task AddAsync(WeeklyRelease release, CancellationToken ct = default)
        {
            Items.Add(release);
            return Task.CompletedTask;
        }

        public Task<WeeklyRelease?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return Task.FromResult(Items.FirstOrDefault(r => r.Id == id));
        }

        public Task UpdateAsync(WeeklyRelease release, CancellationToken ct = default)
        {
            var idx = Items.FindIndex(r => r.Id == release.Id);
            if (idx >= 0) Items[idx] = release;
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            Items.RemoveAll(r => r.Id == id);
            return Task.CompletedTask;
        }
    }

    private readonly InMemoryWeeklyReleaseRepository _repo;
    private readonly WeeklyReleaseService _service;

    public WeeklyReleaseCreationTests()
    {
        _repo = new InMemoryWeeklyReleaseRepository();
        _service = new WeeklyReleaseService(_repo);
    }

    [Fact]
    public async Task CreateReleaseAsync_ValidRequest_AddsToRepositoryAndReturnsDto()
    {
        // Arrange
        var request = new CreateWeeklyReleaseRequest(
            Title: "Dune Imperium: Bloodlines",
            Publisher: "Dire Wolf / Asmodee",
            ReleaseDate: new DateOnly(2026, 10, 15),
            EstimatedPvp: 44.95m,
            IsReprint: false,
            Notes: "Expansión mayor para Dune Imperium.");

        // Act
        var result = await _service.CreateReleaseAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("Dune Imperium: Bloodlines", result.Title);
        Assert.Equal("Dire Wolf / Asmodee", result.Publisher);
        Assert.Equal(44.95m, result.EstimatedPvp);
        Assert.False(result.IsReprint);
        Assert.Single(_repo.Items);
    }

    [Fact]
    public async Task CreateReleaseAsync_NullRequest_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _service.CreateReleaseAsync(null!));
    }

    [Theory]
    [InlineData("", "Publisher")]
    [InlineData("   ", "Publisher")]
    [InlineData("Title", "")]
    [InlineData("Title", "   ")]
    public async Task CreateReleaseAsync_InvalidStrings_ThrowsArgumentException(string title, string publisher)
    {
        var req = new CreateWeeklyReleaseRequest(
            Title: title,
            Publisher: publisher,
            ReleaseDate: DateOnly.FromDateTime(DateTime.UtcNow));

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.CreateReleaseAsync(req));
    }
}
