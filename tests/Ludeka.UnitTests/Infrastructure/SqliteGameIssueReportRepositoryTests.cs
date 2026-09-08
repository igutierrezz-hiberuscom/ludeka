using System;
using System.Threading.Tasks;
using Ludeka.Application.DTOs;
using Ludeka.Core.Entities;
using Ludeka.Core.Enums;
using Ludeka.Infrastructure.Data;
using Ludeka.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Ludeka.UnitTests.Infrastructure;

public class SqliteGameIssueReportRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly LudekaDbContext _dbContext;
    private readonly SqliteGameIssueReportRepository _repository;

    public SqliteGameIssueReportRepositoryTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<LudekaDbContext>()
            .UseSqlite(_connection)
            .Options;

        _dbContext = new LudekaDbContext(options);
        _dbContext.Database.EnsureCreated();

        _repository = new SqliteGameIssueReportRepository(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public async Task AddAsync_And_GetByIdAsync_PersistsAndRetrievesReport()
    {
        var gameId = Guid.NewGuid();
        var report = new GameIssueReport(
            gameId,
            "wingspan",
            "Wingspan",
            GameIssueType.WrongImage,
            "La carátula es de la edición inglesa",
            "Marta Jugona",
            "user-1"
        );

        await _repository.AddAsync(report);

        var retrieved = await _repository.GetByIdAsync(report.Id);

        Assert.NotNull(retrieved);
        Assert.Equal(report.Id, retrieved.Id);
        Assert.Equal("wingspan", retrieved.GameSlug);
        Assert.Equal("Wingspan", retrieved.GameTitle);
        Assert.Equal(GameIssueType.WrongImage, retrieved.IssueType);
        Assert.Equal(GameReportStatus.Pending, retrieved.Status);
    }

    [Fact]
    public async Task GetAllAsync_WithStatusAndIssueTypeFilters_FiltersCorrectly()
    {
        var gameId = Guid.NewGuid();
        var r1 = new GameIssueReport(gameId, "catan", "Catan", GameIssueType.WrongImage, "img errónea");
        var r2 = new GameIssueReport(gameId, "catan", "Catan", GameIssueType.BrokenPurchaseLink, "link roto");
        var r3 = new GameIssueReport(gameId, "carcassonne", "Carcassonne", GameIssueType.WrongImage, "otra img");

        r2.Resolve("mod-1", "Link reparado");

        await _repository.AddAsync(r1);
        await _repository.AddAsync(r2);
        await _repository.AddAsync(r3);

        // Filter: Status Pending only
        var pendingReports = await _repository.GetAllAsync(new GameReportFilter(Status: GameReportStatus.Pending));
        Assert.Equal(2, pendingReports.Count);

        // Filter: Status Resolved only
        var resolvedReports = await _repository.GetAllAsync(new GameReportFilter(Status: GameReportStatus.Resolved));
        Assert.Single(resolvedReports);
        Assert.Equal(GameIssueType.BrokenPurchaseLink, resolvedReports[0].IssueType);

        // Filter: IssueType WrongImage only
        var wrongImgReports = await _repository.GetAllAsync(new GameReportFilter(IssueType: GameIssueType.WrongImage));
        Assert.Equal(2, wrongImgReports.Count);

        // SearchTerm filter
        var searchReports = await _repository.GetAllAsync(new GameReportFilter(SearchTerm: "Carcassonne"));
        Assert.Single(searchReports);
        Assert.Equal("carcassonne", searchReports[0].GameSlug);
    }

    [Fact]
    public async Task GetSummaryAsync_ComputesAccurateCounts()
    {
        var gameId = Guid.NewGuid();
        var r1 = new GameIssueReport(gameId, "catan", "Catan", GameIssueType.WrongImage);
        var r2 = new GameIssueReport(gameId, "catan", "Catan", GameIssueType.BrokenImage);
        var r3 = new GameIssueReport(gameId, "catan", "Catan", GameIssueType.IncorrectDuration);
        var r4 = new GameIssueReport(gameId, "catan", "Catan", GameIssueType.Other, "Texto");

        r2.MarkAsInReview("mod-1");
        r3.Resolve("mod-1", "Regla comprobada");
        r4.Dismiss("mod-1", "No aplica");

        await _repository.AddAsync(r1);
        await _repository.AddAsync(r2);
        await _repository.AddAsync(r3);
        await _repository.AddAsync(r4);

        var summary = await _repository.GetSummaryAsync();

        Assert.Equal(4, summary.TotalCount);
        Assert.Equal(1, summary.PendingCount);
        Assert.Equal(1, summary.InReviewCount);
        Assert.Equal(1, summary.ResolvedCount);
        Assert.Equal(1, summary.DismissedCount);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesReportInDatabase()
    {
        var report = new GameIssueReport(Guid.NewGuid(), "scythe", "Scythe", GameIssueType.IncorrectAge);
        await _repository.AddAsync(report);

        report.Resolve("mod-99", "Corregida edad a 14+");
        await _repository.UpdateAsync(report);

        var retrieved = await _repository.GetByIdAsync(report.Id);
        Assert.NotNull(retrieved);
        Assert.Equal(GameReportStatus.Resolved, retrieved.Status);
        Assert.Equal("mod-99", retrieved.ResolvedByUserId);
        Assert.Equal("Corregida edad a 14+", retrieved.ModeratorNotes);
    }

    [Fact]
    public async Task EnsureSchemaUpToDateAsync_CreatesGameIssueReportsTable_WhenMissing()
    {
        using var rawConnection = new SqliteConnection("DataSource=:memory:");
        await rawConnection.OpenAsync();

        // Creamos solo la tabla Games para simular una base de datos existente previa al incremento 17
        using (var cmd = rawConnection.CreateCommand())
        {
            cmd.CommandText = "CREATE TABLE \"Games\" (\"Id\" TEXT PRIMARY KEY, \"Slug\" TEXT NOT NULL);";
            await cmd.ExecuteNonQueryAsync();
        }

        var options = new DbContextOptionsBuilder<LudekaDbContext>()
            .UseSqlite(rawConnection)
            .Options;

        using var dbContext = new LudekaDbContext(options);

        // Ejecutar migrador defensivo
        await SqliteSchemaMigrator.EnsureSchemaUpToDateAsync(dbContext);

        // Verificar que la tabla GameIssueReports existe y se puede consultar/añadir sin errores
        var reportRepo = new SqliteGameIssueReportRepository(dbContext);
        var report = new GameIssueReport(Guid.NewGuid(), "dune", "Dune", GameIssueType.IncorrectPlayerCount);
        await reportRepo.AddAsync(report);

        var summary = await reportRepo.GetSummaryAsync();
        Assert.Equal(1, summary.TotalCount);
        Assert.Equal(1, summary.PendingCount);
    }
}
