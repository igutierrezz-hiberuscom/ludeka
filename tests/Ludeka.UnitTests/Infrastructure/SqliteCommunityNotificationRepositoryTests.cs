using System;
using System.Threading.Tasks;
using Ludeka.Core.Entities;
using Ludeka.Core.Enums;
using Ludeka.Infrastructure.Data;
using Ludeka.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Ludeka.UnitTests.Infrastructure;

public class SqliteCommunityNotificationRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly LudekaDbContext _dbContext;
    private readonly SqliteCommunityNotificationRepository _repository;

    public SqliteCommunityNotificationRepositoryTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<LudekaDbContext>()
            .UseSqlite(_connection)
            .Options;

        _dbContext = new LudekaDbContext(options);
        _dbContext.Database.EnsureCreated();

        _repository = new SqliteCommunityNotificationRepository(_dbContext);
    }

    [Fact]
    public async Task AddLogAsync_And_GetByIdAsync_WorksCorrectly()
    {
        var log = new CommunityNotificationLog(
            NotificationEventType.FoundingVerdictPublished,
            NotificationChannel.Discord,
            "Nuevo Análisis",
            "Resumen del análisis",
            "https://ludeka.es/juegos/wingspan");

        await _repository.AddLogAsync(log);

        var retrieved = await _repository.GetByIdAsync(log.Id);

        Assert.NotNull(retrieved);
        Assert.Equal(log.Id, retrieved.Id);
        Assert.Equal("Nuevo Análisis", retrieved.Title);
        Assert.Equal(NotificationStatus.Queued, retrieved.Status);
    }

    [Fact]
    public async Task UpdateLogAsync_UpdatesStatusAndSentAt()
    {
        var log = new CommunityNotificationLog(
            NotificationEventType.FridayReleasesSummary,
            NotificationChannel.Telegram,
            "Novedades",
            "Resumen de novedades");

        await _repository.AddLogAsync(log);

        log.MarkAsSent();
        await _repository.UpdateLogAsync(log);

        var retrieved = await _repository.GetByIdAsync(log.Id);

        Assert.NotNull(retrieved);
        Assert.Equal(NotificationStatus.Sent, retrieved.Status);
        Assert.NotNull(retrieved.SentAt);
    }

    [Fact]
    public async Task GetRecentLogsAsync_ReturnsOrderedLogs()
    {
        var log1 = new CommunityNotificationLog(NotificationEventType.CustomTestPing, NotificationChannel.Discord, "Ping 1", "Desc 1");
        var log2 = new CommunityNotificationLog(NotificationEventType.CustomTestPing, NotificationChannel.Telegram, "Ping 2", "Desc 2");

        await _repository.AddLogAsync(log1);
        await _repository.AddLogAsync(log2);

        var recent = await _repository.GetRecentLogsAsync(take: 10);

        Assert.Equal(2, recent.Count);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _connection.Dispose();
    }
}
