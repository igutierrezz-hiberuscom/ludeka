using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ludeka.Application.Contracts;
using Ludeka.Application.DTOs;
using Ludeka.Application.Features.Catalog;
using Ludeka.Core.Entities;
using Ludeka.Core.Enums;
using Ludeka.Core.ValueObjects;
using Xunit;

namespace Ludeka.UnitTests.Application;

public class CatalogServiceExpansionFilterTests
{
    private class InMemoryGameRepository : IGameRepository
    {
        public List<Game> Games { get; } = [];

        public Task<Game?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
            Task.FromResult(Games.FirstOrDefault(g => g.Id == id));

        public Task<Game?> GetBySlugAsync(string slug, CancellationToken ct = default) =>
            Task.FromResult(Games.FirstOrDefault(g => g.Slug == slug));

        public Task<Game?> GetByBggIdAsync(int bggId, CancellationToken ct = default) =>
            Task.FromResult(Games.FirstOrDefault(g => g.BggId == bggId));

        public Task<(IReadOnlyList<Game> Items, int TotalCount)> SearchAsync(
            GameFilterCriteria criteria, int page = 1, int pageSize = 20, CancellationToken ct = default)
        {
            var query = Games.AsEnumerable();

            if (criteria.TypeFilter.HasValue)
            {
                query = query.Where(g => g.Type == criteria.TypeFilter.Value);
            }

            if (!string.IsNullOrWhiteSpace(criteria.SearchTerm))
            {
                query = query.Where(g => g.SpanishTitle.Contains(criteria.SearchTerm, StringComparison.OrdinalIgnoreCase));
            }

            var list = query.ToList();
            var paged = list.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            return Task.FromResult(((IReadOnlyList<Game>)paged, list.Count));
        }

        public Task AddRangeAsync(IEnumerable<Game> games, CancellationToken ct = default)
        {
            Games.AddRange(games);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Game game, CancellationToken ct = default) => Task.CompletedTask;
        public Task<bool> HasAnyAsync(CancellationToken ct = default) => Task.FromResult(Games.Count > 0);
    }

    private static Game CreateSample(string title, GameType type) => new(
        bggId: Random.Shared.Next(1000, 99999),
        originalTitle: title,
        spanishTitle: title,
        designer: "Autor",
        publisher: "Editorial",
        yearPublished: 2020,
        coverImageUrl: null,
        thumbnailUrl: null,
        description: "Desc",
        bggRating: 8.0,
        bggRank: 50,
        ludistRating: 8.1,
        confrontation: ConfrontationType.Competitive,
        style: GameStyle.Eurogame,
        isOfficialSolo: true,
        age: new AgeRating(10, 10),
        language: LanguageDependence.None,
        footprint: TableFootprint.StandardTable,
        duration: new GameDuration(40, 70, 25),
        type: type
    );

    [Fact]
    public async Task SearchAsync_FiltersCorrectly_ByGameType()
    {
        var repo = new InMemoryGameRepository();
        repo.Games.Add(CreateSample("Wingspan", GameType.BaseGame));
        repo.Games.Add(CreateSample("Wingspan Europa", GameType.Expansion));
        repo.Games.Add(CreateSample("Wingspan Oceanía", GameType.Expansion));
        repo.Games.Add(CreateSample("Carcassonne", GameType.BaseGame));

        var service = new CatalogService(repo);

        // 1. Filtrar solo expansiones
        var expResult = await service.GetCatalogAsync(new GameFilterCriteria(TypeFilter: GameType.Expansion));
        Assert.Equal(2, expResult.TotalCount);
        Assert.All(expResult.Games, g => Assert.True(g.IsExpansion));

        // 2. Filtrar solo juegos base
        var baseResult = await service.GetCatalogAsync(new GameFilterCriteria(TypeFilter: GameType.BaseGame));
        Assert.Equal(2, baseResult.TotalCount);
        Assert.All(baseResult.Games, g => Assert.False(g.IsExpansion));

        // 3. Sin filtro de tipo
        var allResult = await service.GetCatalogAsync(new GameFilterCriteria());
        Assert.Equal(4, allResult.TotalCount);
    }
}
