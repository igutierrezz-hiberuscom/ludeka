using System;
using System.Linq;
using System.Threading.Tasks;
using Ludeka.Core.Entities;
using Ludeka.Core.Enums;
using Ludeka.Core.ValueObjects;
using Ludeka.Infrastructure.Data;
using Ludeka.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Ludeka.UnitTests.Infrastructure;

public class SqliteExpansionRepositoryTests : IAsyncDisposable
{
    private readonly SqliteConnection _connection;
    private readonly LudekaDbContext _context;
    private readonly SqliteExpansionRepository _repository;

    public SqliteExpansionRepositoryTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<LudekaDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new LudekaDbContext(options);
        _context.Database.EnsureCreated();

        _repository = new SqliteExpansionRepository(_context);
    }

    public async ValueTask DisposeAsync()
    {
        await _context.DisposeAsync();
        await _connection.DisposeAsync();
    }

    private static Game CreateGame(string title, GameType type = GameType.BaseGame, Guid? baseGameId = null) => new(
        bggId: Random.Shared.Next(1000, 999999),
        originalTitle: title,
        spanishTitle: title,
        designer: "Autor",
        publisher: "Editorial",
        yearPublished: 2021,
        coverImageUrl: null,
        thumbnailUrl: null,
        description: "Desc",
        bggRating: 8.0,
        bggRank: 50,
        ludistRating: 8.2,
        confrontation: ConfrontationType.Competitive,
        style: GameStyle.Eurogame,
        isOfficialSolo: true,
        age: new AgeRating(10, 10),
        language: LanguageDependence.None,
        footprint: TableFootprint.StandardTable,
        duration: new GameDuration(40, 70, 25),
        type: type,
        baseGameId: baseGameId
    );

    [Fact]
    public async Task GetExpansionsByBaseGameIdAsync_ReturnsOnlyRelatedExpansions()
    {
        var base1 = CreateGame("Base 1");
        var base2 = CreateGame("Base 2");
        await _context.Games.AddRangeAsync([base1, base2]);
        await _context.SaveChangesAsync();

        var exp1 = CreateGame("Expansión 1A", GameType.Expansion, base1.Id);
        var exp2 = CreateGame("Expansión 1B", GameType.Expansion, base1.Id);
        var expOther = CreateGame("Expansión 2A", GameType.Expansion, base2.Id);

        await _context.Games.AddRangeAsync([exp1, exp2, expOther]);
        await _context.SaveChangesAsync();

        var result = await _repository.GetExpansionsByBaseGameIdAsync(base1.Id);

        Assert.Equal(2, result.Count);
        Assert.All(result, e => Assert.Equal(base1.Id, e.BaseGameId));
    }

    [Fact]
    public async Task GetExpansionWithBaseGameAsync_IncludesBaseGameNavigation()
    {
        var baseGame = CreateGame("Wingspan");
        await _context.Games.AddAsync(baseGame);
        await _context.SaveChangesAsync();

        var exp = CreateGame("Wingspan Europa", GameType.Expansion, baseGame.Id);
        await _context.Games.AddAsync(exp);
        await _context.SaveChangesAsync();

        var fetched = await _repository.GetExpansionWithBaseGameAsync(exp.Id);

        Assert.NotNull(fetched);
        Assert.NotNull(fetched.BaseGame);
        Assert.Equal("Wingspan", fetched.BaseGame.SpanishTitle);
    }

    [Fact]
    public async Task AddAndGetSynergiesAndRecipes_PersistSuccessfully()
    {
        var baseGame = CreateGame("Terraforming Mars");
        await _context.Games.AddAsync(baseGame);
        await _context.SaveChangesAsync();

        var expA = CreateGame("Preludio", GameType.Expansion, baseGame.Id);
        var expB = CreateGame("Hellas", GameType.Expansion, baseGame.Id);
        await _context.Games.AddRangeAsync([expA, expB]);
        await _context.SaveChangesAsync();

        var synergy = new ExpansionSynergy(
            baseGame.Id, expA.Id, expB.Id, ExpansionSynergyLevel.PerfectCombo, "Combo de torneo"
        );
        await _repository.AddSynergyAsync(synergy);

        var recipe = new ExpansionRecipe(
            baseGame.Id, "Torneo Ágil", "Excelente para 3J", "3-4 jugadores", [expA.Id, expB.Id]
        );
        await _repository.AddRecipeAsync(recipe);

        var synergies = await _repository.GetSynergiesByBaseGameIdAsync(baseGame.Id);
        var recipes = await _repository.GetRecipesByBaseGameIdAsync(baseGame.Id);

        Assert.Single(synergies);
        Assert.Equal("Combo de torneo", synergies[0].Reason);
        Assert.Equal(ExpansionSynergyLevel.PerfectCombo, synergies[0].Level);

        Assert.Single(recipes);
        Assert.Equal("Torneo Ágil", recipes[0].Name);
        Assert.Equal(2, recipes[0].IncludedExpansionIds.Count);
    }
}
