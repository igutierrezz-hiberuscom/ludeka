using System;
using Ludeka.Core.Entities;
using Ludeka.Core.Enums;
using Xunit;

namespace Ludeka.UnitTests.Domain;

public class CommunityNotificationLogTests
{
    [Fact]
    public void Constructor_WithValidArguments_InitializesCorrectly()
    {
        var log = new CommunityNotificationLog(
            NotificationEventType.GiveawayExpiring,
            NotificationChannel.Discord,
            "Alerta de Sorteo",
            "Finaliza en 24h",
            "https://ludeka.es/radar",
            "https://ludeka.es/img.webp",
            NotificationStatus.Queued);

        Assert.NotEqual(Guid.Empty, log.Id);
        Assert.Equal(NotificationEventType.GiveawayExpiring, log.EventType);
        Assert.Equal(NotificationChannel.Discord, log.Channel);
        Assert.Equal("Alerta de Sorteo", log.Title);
        Assert.Equal("Finaliza en 24h", log.Summary);
        Assert.Equal("https://ludeka.es/radar", log.TargetUrl);
        Assert.Equal("https://ludeka.es/img.webp", log.ImageUrl);
        Assert.Equal(NotificationStatus.Queued, log.Status);
        Assert.Null(log.SentAt);
        Assert.Null(log.ErrorDetails);
    }

    [Theory]
    [InlineData("", "Resumen válido")]
    [InlineData("   ", "Resumen válido")]
    [InlineData(null, "Resumen válido")]
    [InlineData("Título válido", "")]
    [InlineData("Título válido", "   ")]
    [InlineData("Título válido", null)]
    public void Constructor_WithInvalidTitleOrSummary_ThrowsArgumentException(string? title, string? summary)
    {
        Assert.Throws<ArgumentException>(() => new CommunityNotificationLog(
            NotificationEventType.CustomTestPing,
            NotificationChannel.Telegram,
            title!,
            summary!));
    }

    [Fact]
    public void MarkAsSent_UpdatesStatusAndSentAt()
    {
        var log = new CommunityNotificationLog(
            NotificationEventType.FridayReleasesSummary,
            NotificationChannel.Telegram,
            "Novedades",
            "Resumen del viernes");

        log.MarkAsSent();

        Assert.Equal(NotificationStatus.Sent, log.Status);
        Assert.NotNull(log.SentAt);
        Assert.Null(log.ErrorDetails);
    }

    [Fact]
    public void MarkAsFailed_UpdatesStatusAndErrorDetails()
    {
        var log = new CommunityNotificationLog(
            NotificationEventType.FoundingVerdictPublished,
            NotificationChannel.Discord,
            "Nuevo Veredicto",
            "Análisis oficial");

        log.MarkAsFailed("HTTP 429 Rate Limit Exceeded");

        Assert.Equal(NotificationStatus.Failed, log.Status);
        Assert.Equal("HTTP 429 Rate Limit Exceeded", log.ErrorDetails);
    }

    [Fact]
    public void MarkAsDryRun_UpdatesStatusToDryRun()
    {
        var log = new CommunityNotificationLog(
            NotificationEventType.CustomTestPing,
            NotificationChannel.Discord,
            "Ping de prueba",
            "Simulación local");

        log.MarkAsDryRun();

        Assert.Equal(NotificationStatus.DryRun, log.Status);
        Assert.NotNull(log.SentAt);
        Assert.Null(log.ErrorDetails);
    }
}
