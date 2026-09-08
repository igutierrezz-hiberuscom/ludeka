using System.Linq;
using System.Threading.Tasks;
using Ludeka.Core.Enums;
using Ludeka.Infrastructure.Bgg;
using Xunit;

namespace Ludeka.UnitTests.Infrastructure;

public class SimulatedBggClientTests
{
    private readonly SimulatedBggClient _client = new();

    [Fact]
    public async Task SearchGamesAsync_WithSpanishTitle_ReturnsExpectedGame()
    {
        var results = await _client.SearchGamesAsync("Catán");

        Assert.NotEmpty(results);
        var match = results.FirstOrDefault(r => r.BggId == 13);
        Assert.NotNull(match);
        Assert.Equal("Catán", match.Title);
        Assert.Equal(1995, match.YearPublished);
    }

    [Theory]
    [InlineData("catan", 13)]
    [InlineData("agricola", 31260)]
    [InlineData("codigo secreto", 178900)]
    [InlineData("wingspan", 266192)]
    public async Task SearchGamesAsync_IsCaseAndDiacriticInsensitive(string query, int expectedBggId)
    {
        var results = await _client.SearchGamesAsync(query);

        Assert.Contains(results, r => r.BggId == expectedBggId);
    }

    [Fact]
    public async Task SearchGamesAsync_ByDesigner_ReturnsGamesByThatDesigner()
    {
        var results = await _client.SearchGamesAsync("Uwe Rosenberg");

        Assert.NotEmpty(results);
        Assert.Contains(results, r => r.BggId == 163412); // Patchwork
        Assert.Contains(results, r => r.BggId == 31260);  // Agrícola
    }

    [Fact]
    public async Task SearchGamesAsync_WhenQueryIsTooShortOrEmpty_ReturnsEmpty()
    {
        Assert.Empty(await _client.SearchGamesAsync(""));
        Assert.Empty(await _client.SearchGamesAsync("   "));
        Assert.Empty(await _client.SearchGamesAsync("a"));
    }

    [Fact]
    public async Task SearchGamesAsync_WhenNoMatch_ReturnsEmpty()
    {
        var results = await _client.SearchGamesAsync("JuegoInexistenteZyx12345");

        Assert.Empty(results);
    }

    [Fact]
    public async Task FetchGameByBggIdAsync_WhenGameExists_ReturnsPopulatedEntityWithAllValueObjects()
    {
        var game = await _client.FetchGameByBggIdAsync(316554); // Dune: Imperium

        Assert.NotNull(game);
        Assert.Equal(316554, game.BggId);
        Assert.Equal("Dune: Imperium", game.SpanishTitle);
        Assert.Equal("Paul Dennen", game.Designer);
        Assert.Equal(GameType.BaseGame, game.Type);
        Assert.NotEmpty(game.Scalability);
        Assert.NotEmpty(game.Sleeves);
        Assert.NotEmpty(game.PurchaseLinks);
        Assert.NotNull(game.Age);
        Assert.NotNull(game.Duration);
    }

    [Fact]
    public async Task FetchGameByBggIdAsync_WhenExpansionExists_ReturnsExpansionMetadata()
    {
        var expansion = await _client.FetchGameByBggIdAsync(290448); // Wingspan Europa

        Assert.NotNull(expansion);
        Assert.Equal(290448, expansion.BggId);
        Assert.Equal(GameType.Expansion, expansion.Type);
        Assert.Equal(ExpansionNecessity.MustHave, expansion.ExpansionNecessity);
        Assert.NotNull(expansion.ImpactTags);
        Assert.Contains(ExpansionImpactTag.FixesBalance, expansion.ImpactTags);
        Assert.False(string.IsNullOrWhiteSpace(expansion.WhatItBringsSummary));
    }

    [Fact]
    public async Task FetchGameByBggIdAsync_WhenGameDoesNotExist_ReturnsNull()
    {
        var game = await _client.FetchGameByBggIdAsync(9999999);

        Assert.Null(game);
    }

    [Fact]
    public async Task FetchGameByBggIdAsync_ReturnsFreshInstancesEachTime()
    {
        var game1 = await _client.FetchGameByBggIdAsync(266192);
        var game2 = await _client.FetchGameByBggIdAsync(266192);

        Assert.NotNull(game1);
        Assert.NotNull(game2);
        Assert.NotSame(game1, game2);
        Assert.NotEqual(game1.Id, game2.Id);
    }

    [Fact]
    public async Task FetchUserCollectionAsync_ForLudekaDemo_ReturnsBalancedCollection()
    {
        var collection = await _client.FetchUserCollectionAsync("ludeka_demo");

        Assert.Equal(12, collection.Count);
        Assert.Contains(collection, c => c.IsOwned);
        Assert.Contains(collection, c => c.IsWishlist);
        Assert.Contains(collection, c => c.IsWantToBuy);
        Assert.Contains(collection, c => c.BggId == 266192 && c.IsOwned); // Wingspan
        Assert.Contains(collection, c => c.BggId == 199792 && c.IsWishlist); // Everdell
    }

    [Fact]
    public async Task FetchUserCollectionAsync_ForParejaJugona_ReturnsTwoPlayerCollection()
    {
        var collection = await _client.FetchUserCollectionAsync("pareja_jugona");

        Assert.NotEmpty(collection);
        Assert.Contains(collection, c => c.BggId == 173346); // 7 Wonders Duel
        Assert.Contains(collection, c => c.BggId == 163412); // Patchwork
    }

    [Fact]
    public async Task FetchUserCollectionAsync_ForMaratonEuro_ReturnsHeavyEuroCollection()
    {
        var collection = await _client.FetchUserCollectionAsync("maraton_euro");

        Assert.NotEmpty(collection);
        Assert.Contains(collection, c => c.BggId == 167791); // Terraforming Mars
        Assert.Contains(collection, c => c.BggId == 224517); // Brass: Birmingham
    }

    [Fact]
    public async Task FetchUserCollectionAsync_ForUnknownUser_ReturnsDefaultCuratedCollection()
    {
        var collection = await _client.FetchUserCollectionAsync("usuario_aleatorio_99");

        Assert.NotEmpty(collection);
        Assert.Equal(6, collection.Count);
    }
}
