using System;
using System.Threading.Tasks;
using Ludeka.Application.DTOs;
using Ludeka.Application.Features.Reports;
using Ludeka.Core.Entities;
using Ludeka.Core.Enums;
using Ludeka.Infrastructure.Data;
using Ludeka.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Ludeka.UnitTests.Application;

public class GameIssueReportServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly LudekaDbContext _dbContext;
    private readonly SqliteGameIssueReportRepository _repository;
    private readonly GameIssueReportService _service;

    public GameIssueReportServiceTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<LudekaDbContext>()
            .UseSqlite(_connection)
            .Options;

        _dbContext = new LudekaDbContext(options);
        _dbContext.Database.EnsureCreated();

        _repository = new SqliteGameIssueReportRepository(_dbContext);
        _service = new GameIssueReportService(_repository);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public async Task CreateReportAsync_WithValidCommand_PersistsAndReturnsDto()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var command = new CreateGameReportCommand(
            GameId: gameId,
            GameSlug: "brass-birmingham",
            GameTitle: "Brass: Birmingham",
            IssueType: GameIssueType.WrongImage,
            Details: "La imagen es de la primera versión",
            ReporterNameOrAlias: "Pedro",
            UserId: "user-42"
        );

        // Act
        var result = await _service.CreateReportAsync(command);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(gameId, result.GameId);
        Assert.Equal("brass-birmingham", result.GameSlug);
        Assert.Equal("Brass: Birmingham", result.GameTitle);
        Assert.Equal(GameIssueType.WrongImage, result.IssueType);
        Assert.Equal("Imagen incorrecta o de otra edición", result.IssueTypeName);
        Assert.Equal("Pendiente", result.StatusName);
        Assert.Equal("Pedro", result.ReporterNameOrAlias);
        Assert.Equal("user-42", result.ReportedByUserId);
    }

    [Fact]
    public async Task CreateReportAsync_WithOtherTypeAndEmptyDetails_ThrowsArgumentException()
    {
        var command = new CreateGameReportCommand(
            GameId: Guid.NewGuid(),
            GameSlug: "catan",
            GameTitle: "Catan",
            IssueType: GameIssueType.Other,
            Details: "   "
        );

        var ex = await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateReportAsync(command).AsTask());
        Assert.Contains("Debes detallar la incidencia si seleccionas 'Otro problema'", ex.Message);
    }

    [Fact]
    public async Task CreateReportAsync_TruncatesDetailsIfOver1000Chars()
    {
        var longDetails = new string('A', 1200);
        var command = new CreateGameReportCommand(
            GameId: Guid.NewGuid(),
            GameSlug: "catan",
            GameTitle: "Catan",
            IssueType: GameIssueType.ErroneousMetadata,
            Details: longDetails
        );

        var result = await _service.CreateReportAsync(command);

        Assert.Equal(1000, result.Details?.Length);
    }

    [Fact]
    public async Task ChangeStatusAsync_ToInReview_UpdatesCorrectly()
    {
        var created = await _service.CreateReportAsync(new CreateGameReportCommand(
            Guid.NewGuid(), "dune-imperium", "Dune: Imperium", GameIssueType.IncorrectPlayerCount
        ));

        var updated = await _service.ChangeStatusAsync(created.Id, new UpdateGameReportStatusCommand(
            NewStatus: GameReportStatus.InReview,
            ModeratorId: "mod-1",
            Notes: "Revisando modo solitario"
        ));

        Assert.NotNull(updated);
        Assert.Equal(GameReportStatus.InReview, updated.Status);
        Assert.Equal("En revisión", updated.StatusName);
        Assert.Equal("mod-1", updated.ResolvedByUserId);
        Assert.Equal("Revisando modo solitario", updated.ModeratorNotes);
    }

    [Fact]
    public async Task ChangeStatusAsync_ToResolved_SetsResolvedAt()
    {
        var created = await _service.CreateReportAsync(new CreateGameReportCommand(
            Guid.NewGuid(), "root", "Root", GameIssueType.BrokenPurchaseLink
        ));

        var updated = await _service.ChangeStatusAsync(created.Id, new UpdateGameReportStatusCommand(
            NewStatus: GameReportStatus.Resolved,
            ModeratorId: "mod-2",
            Notes: "Enlace corregido a tienda 2Tomatoes"
        ));

        Assert.NotNull(updated);
        Assert.Equal(GameReportStatus.Resolved, updated.Status);
        Assert.Equal("Resuelto", updated.StatusName);
        Assert.NotNull(updated.ResolvedAt);
    }

    [Fact]
    public async Task ChangeStatusAsync_ToDismissed_SetsDismissedAndNotes()
    {
        var created = await _service.CreateReportAsync(new CreateGameReportCommand(
            Guid.NewGuid(), "azul", "Azul", GameIssueType.IncorrectAge
        ));

        var updated = await _service.ChangeStatusAsync(created.Id, new UpdateGameReportStatusCommand(
            NewStatus: GameReportStatus.Dismissed,
            ModeratorId: "mod-3",
            Notes: "Edad 8+ es la legal oficial de Plan B Games"
        ));

        Assert.NotNull(updated);
        Assert.Equal(GameReportStatus.Dismissed, updated.Status);
        Assert.Equal("Descartado", updated.StatusName);
        Assert.Equal("Edad 8+ es la legal oficial de Plan B Games", updated.ModeratorNotes);
    }

    [Fact]
    public async Task ChangeStatusAsync_NonExistentId_ReturnsNull()
    {
        var updated = await _service.ChangeStatusAsync(Guid.NewGuid(), new UpdateGameReportStatusCommand(
            NewStatus: GameReportStatus.InReview,
            ModeratorId: "mod-1"
        ));

        Assert.Null(updated);
    }

    [Fact]
    public async Task GetSummaryAsync_ReturnsAccurateMetrics()
    {
        var r1 = await _service.CreateReportAsync(new CreateGameReportCommand(Guid.NewGuid(), "g1", "Juego 1", GameIssueType.WrongImage));
        var r2 = await _service.CreateReportAsync(new CreateGameReportCommand(Guid.NewGuid(), "g2", "Juego 2", GameIssueType.BrokenImage));
        var r3 = await _service.CreateReportAsync(new CreateGameReportCommand(Guid.NewGuid(), "g3", "Juego 3", GameIssueType.IncorrectAge));

        await _service.ChangeStatusAsync(r2.Id, new UpdateGameReportStatusCommand(GameReportStatus.InReview, "mod-1"));
        await _service.ChangeStatusAsync(r3.Id, new UpdateGameReportStatusCommand(GameReportStatus.Resolved, "mod-1"));

        var summary = await _service.GetSummaryAsync();

        Assert.Equal(3, summary.TotalCount);
        Assert.Equal(1, summary.PendingCount);
        Assert.Equal(1, summary.InReviewCount);
        Assert.Equal(1, summary.ResolvedCount);
        Assert.Equal(0, summary.DismissedCount);
    }
}
