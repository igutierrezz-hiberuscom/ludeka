using System;
using System.Collections.Generic;
using Ludeka.Core.Entities;
using Ludeka.Core.Enums;
using Ludeka.Core.ValueObjects;
using Xunit;

namespace Ludeka.UnitTests.Domain;

public class GameExpansionDomainTests
{
    private static Game CreateBaseGame(string title = "Wingspan") => new(
        bggId: 266192,
        originalTitle: title,
        spanishTitle: title,
        designer: "Elizabeth Hargrave",
        publisher: "Maldito Games",
        yearPublished: 2019,
        coverImageUrl: "/images/wingspan.png",
        thumbnailUrl: null,
        description: "Juego de aves",
        bggRating: 8.1,
        bggRank: 25,
        ludistRating: 8.3,
        confrontation: ConfrontationType.Competitive,
        style: GameStyle.Eurogame,
        isOfficialSolo: true,
        age: new AgeRating(10, 10),
        language: LanguageDependence.Low,
        footprint: TableFootprint.StandardTable,
        duration: new GameDuration(40, 70, 25),
        type: GameType.BaseGame
    );

    [Fact]
    public void BaseGame_HasDefaultGameTypeAndIsNotExpansion()
    {
        var baseGame = CreateBaseGame();

        Assert.Equal(GameType.BaseGame, baseGame.Type);
        Assert.False(baseGame.IsExpansion);
        Assert.Null(baseGame.BaseGameId);
        Assert.Empty(baseGame.Expansions);
        Assert.Empty(baseGame.ImpactTags);
    }

    [Fact]
    public void ExpansionGame_InitializedCorrectly_WithImpactAttributes()
    {
        var baseGame = CreateBaseGame();
        var expansion = new Game(
            bggId: 300580,
            originalTitle: "Wingspan: Oceania Expansion",
            spanishTitle: "Wingspan: Expansión Oceanía",
            designer: "Elizabeth Hargrave",
            publisher: "Maldito Games",
            yearPublished: 2020,
            coverImageUrl: "/images/oceania.png",
            thumbnailUrl: null,
            description: "Añade aves de Oceanía y néctar",
            bggRating: 8.4,
            bggRank: 110,
            ludistRating: 8.7,
            confrontation: ConfrontationType.Competitive,
            style: GameStyle.Eurogame,
            isOfficialSolo: true,
            age: new AgeRating(10, 10),
            language: LanguageDependence.Low,
            footprint: TableFootprint.StandardTable,
            duration: new GameDuration(40, 70, 25),
            type: GameType.Expansion,
            baseGameId: baseGame.Id,
            expansionNecessity: ExpansionNecessity.MustHave,
            impactTags: [ExpansionImpactTag.FixesBalance, ExpansionImpactTag.ModularContent],
            whatItBringsSummary: "Introduce el néctar y rebalancea la acción de poner huevos con nuevos tableros de jugador.",
            extraPlayerCount: 0,
            extraDurationMinutes: 10
        );

        Assert.Equal(GameType.Expansion, expansion.Type);
        Assert.True(expansion.IsExpansion);
        Assert.Equal(baseGame.Id, expansion.BaseGameId);
        Assert.Equal(ExpansionNecessity.MustHave, expansion.ExpansionNecessity);
        Assert.Contains(ExpansionImpactTag.FixesBalance, expansion.ImpactTags);
        Assert.Contains(ExpansionImpactTag.ModularContent, expansion.ImpactTags);
        Assert.Equal("Introduce el néctar y rebalancea la acción de poner huevos con nuevos tableros de jugador.", expansion.WhatItBringsSummary);
        Assert.Equal(0, expansion.ExtraPlayerCount);
        Assert.Equal(10, expansion.ExtraDurationMinutes);
    }

    [Fact]
    public void ConfigureExpansion_UpdatesStateCorrectly()
    {
        var game = CreateBaseGame("Carcassonne: Posadas y Catedrales");
        var baseId = Guid.NewGuid();

        game.ConfigureExpansion(
            baseGameId: baseId,
            necessity: ExpansionNecessity.MustHave,
            impactTags: [ExpansionImpactTag.AddsPlayers, ExpansionImpactTag.ModularContent],
            whatItBringsSummary: "Añade el 6º jugador, meeple grande y losetas de riesgo.",
            extraPlayerCount: 1,
            extraDurationMinutes: 15
        );

        Assert.Equal(GameType.Expansion, game.Type);
        Assert.True(game.IsExpansion);
        Assert.Equal(baseId, game.BaseGameId);
        Assert.Equal(ExpansionNecessity.MustHave, game.ExpansionNecessity);
        Assert.Equal(1, game.ExtraPlayerCount);
        Assert.Equal(15, game.ExtraDurationMinutes);
    }

    [Fact]
    public void ExpansionSynergy_ThrowsOnInvalidArguments()
    {
        var baseId = Guid.NewGuid();
        var expA = Guid.NewGuid();

        Assert.Throws<ArgumentException>(() => new ExpansionSynergy(Guid.Empty, expA, Guid.NewGuid(), ExpansionSynergyLevel.PerfectCombo, "test"));
        Assert.Throws<ArgumentException>(() => new ExpansionSynergy(baseId, Guid.Empty, Guid.NewGuid(), ExpansionSynergyLevel.PerfectCombo, "test"));
        Assert.Throws<ArgumentException>(() => new ExpansionSynergy(baseId, expA, expA, ExpansionSynergyLevel.PerfectCombo, "same"));
    }

    [Fact]
    public void ExpansionSynergy_MatchesPairSymmetrically()
    {
        var baseId = Guid.NewGuid();
        var expA = Guid.NewGuid();
        var expB = Guid.NewGuid();
        var expC = Guid.NewGuid();

        var synergy = new ExpansionSynergy(baseId, expA, expB, ExpansionSynergyLevel.PerfectCombo, "Excelente combo");

        Assert.True(synergy.MatchesPair(expA, expB));
        Assert.True(synergy.MatchesPair(expB, expA));
        Assert.False(synergy.MatchesPair(expA, expC));
    }

    [Fact]
    public void ExpansionRecipe_CreatesSuccessfully()
    {
        var baseId = Guid.NewGuid();
        var exp1 = Guid.NewGuid();
        var exp2 = Guid.NewGuid();

        var recipe = new ExpansionRecipe(
            baseGameId: baseId,
            name: "Setup Torneo",
            description: "La combinación más equilibrada para juego competitivo",
            idealFor: "3-4 jugadores en 60 min",
            includedExpansionIds: [exp1, exp2]
        );

        Assert.Equal("Setup Torneo", recipe.Name);
        Assert.Equal(2, recipe.IncludedExpansionIds.Count);
        Assert.Contains(exp1, recipe.IncludedExpansionIds);
        Assert.Contains(exp2, recipe.IncludedExpansionIds);
    }
}
