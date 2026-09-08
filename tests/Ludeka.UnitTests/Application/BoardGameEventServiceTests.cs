using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ludeka.Application.Contracts;
using Ludeka.Application.DTOs;
using Ludeka.Application.Features.Events;
using Ludeka.Core.Entities;
using Xunit;

namespace Ludeka.UnitTests.Application;

public class BoardGameEventServiceTests
{
    private class InMemoryBoardGameEventRepository : IBoardGameEventRepository
    {
        public List<BoardGameEvent> Events { get; } = new();

        public Task<IReadOnlyList<BoardGameEvent>> GetUpcomingEventsAsync(int limit = 20, CancellationToken ct = default)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var result = Events.Where(e => e.EndDate >= today).OrderBy(e => e.StartDate).Take(limit).ToList();
            return Task.FromResult<IReadOnlyList<BoardGameEvent>>(result);
        }

        public Task<IReadOnlyList<BoardGameEvent>> GetAllEventsAsync(CancellationToken ct = default)
        {
            return Task.FromResult<IReadOnlyList<BoardGameEvent>>(Events.OrderBy(e => e.StartDate).ToList());
        }

        public Task<BoardGameEvent?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return Task.FromResult(Events.FirstOrDefault(e => e.Id == id));
        }

        public Task AddAsync(BoardGameEvent boardGameEvent, CancellationToken ct = default)
        {
            Events.Add(boardGameEvent);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(BoardGameEvent boardGameEvent, CancellationToken ct = default)
        {
            var index = Events.FindIndex(e => e.Id == boardGameEvent.Id);
            if (index >= 0)
            {
                Events[index] = boardGameEvent;
            }
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            Events.RemoveAll(e => e.Id == id);
            return Task.CompletedTask;
        }
    }

    private readonly InMemoryBoardGameEventRepository _repo;
    private readonly BoardGameEventService _service;

    public BoardGameEventServiceTests()
    {
        _repo = new InMemoryBoardGameEventRepository();
        _service = new BoardGameEventService(_repo);
    }

    [Fact]
    public async Task GetUpcomingEventsAsync_ReturnsFutureAndOngoingEvents_OrderedByStartDate()
    {
        // Arrange
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var pastEvent = new BoardGameEvent(
            "Essen Pasado", "Edición anterior", "/images/past.jpg",
            today.AddDays(-15), today.AddDays(-10), "Essen", "https://spiel.de", "SPIEL", true);

        var ongoingEvent = new BoardGameEvent(
            "Festival Córdoba", "Edición actual", "/images/cordoba.jpg",
            today.AddDays(-1), today.AddDays(2), "Córdoba", "https://cordoba.es", "Jugamos", true);

        var futureEvent = new BoardGameEvent(
            "InterOcio 2027", "Próxima edición", "/images/interocio.jpg",
            today.AddDays(30), today.AddDays(32), "Madrid", "https://interocio.es", "IFEMA", true);

        await _repo.AddAsync(pastEvent);
        await _repo.AddAsync(ongoingEvent);
        await _repo.AddAsync(futureEvent);

        // Act
        var upcoming = await _service.GetUpcomingEventsAsync();

        // Assert
        Assert.Equal(2, upcoming.Count);
        Assert.Equal("Festival Córdoba", upcoming[0].Title);
        Assert.Equal("¡En curso!", upcoming[0].RemainingDaysText);
        Assert.Equal("InterOcio 2027", upcoming[1].Title);
        Assert.StartsWith("En ", upcoming[1].RemainingDaysText);
    }

    [Fact]
    public async Task GetPastEventsAsync_ReturnsOnlyPastEvents_OrderedByEndDateDescending()
    {
        // Arrange
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var olderPast = new BoardGameEvent(
            "DAU 2024", "Hace dos años", "/images/dau24.jpg",
            today.AddDays(-60), today.AddDays(-58), "Barcelona", null, "DAU", true);

        var recentPast = new BoardGameEvent(
            "Gen Con 2025", "Hace un mes", "/images/gencon.jpg",
            today.AddDays(-35), today.AddDays(-30), "Indianapolis", null, "Gen Con", true);

        var future = new BoardGameEvent(
            "Essen 2026", "Futuro", "/images/essen.jpg",
            today.AddDays(10), today.AddDays(13), "Essen", null, "SPIEL", true);

        await _repo.AddAsync(olderPast);
        await _repo.AddAsync(recentPast);
        await _repo.AddAsync(future);

        // Act
        var past = await _service.GetPastEventsAsync();

        // Assert
        Assert.Equal(2, past.Count);
        Assert.Equal("Gen Con 2025", past[0].Title);
        Assert.Equal("DAU 2024", past[1].Title);
    }

    [Fact]
    public async Task CreateEventAsync_ValidRequest_PersistsAndReturnsDto()
    {
        // Arrange
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var request = new CreateBoardGameEventRequest(
            Title: "Festival Internacional de Córdoba",
            Description: "Encuentro lúdico en el Palacio de la Merced",
            ImageUrl: "/images/events/cordoba.jpg",
            StartDate: today.AddDays(14),
            EndDate: today.AddDays(16),
            Location: "Córdoba",
            WebsiteUrl: "https://festivaldejuegos.es",
            Organizer: "Jugamos Tod@s",
            IsOfficial: true);

        // Act
        var result = await _service.CreateEventAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("Festival Internacional de Córdoba", result.Title);
        Assert.Equal("Jugamos Tod@s", result.Organizer);
        Assert.Single(_repo.Events);
    }

    [Fact]
    public async Task UpdateEventAsync_ExistingEvent_UpdatesDetailsSuccessfully()
    {
        // Arrange
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var evt = new BoardGameEvent(
            "InterOcio", "Feria de ocio", "/images/interocio.jpg",
            today.AddDays(10), today.AddDays(12), "Madrid", null, "IFEMA", false);

        await _repo.AddAsync(evt);

        var updateReq = new UpdateBoardGameEventRequest(
            Title: "InterOcio 2026 Oficial",
            Description: "Feria internacional ampliada",
            ImageUrl: "/images/events/interocio_new.jpg",
            StartDate: today.AddDays(10),
            EndDate: today.AddDays(12),
            Location: "IFEMA Madrid Pabellón 9",
            WebsiteUrl: "https://interocio.es",
            Organizer: "IFEMA & Amigos",
            IsOfficial: true);

        // Act
        var updated = await _service.UpdateEventAsync(evt.Id, updateReq);

        // Assert
        Assert.Equal("InterOcio 2026 Oficial", updated.Title);
        Assert.Equal("IFEMA Madrid Pabellón 9", updated.Location);
        Assert.True(updated.IsOfficial);
    }

    [Fact]
    public async Task UpdateEventAsync_NonExistingId_ThrowsKeyNotFoundException()
    {
        // Arrange
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var updateReq = new UpdateBoardGameEventRequest(
            Title: "Inexistente",
            Description: "Desc",
            ImageUrl: "/images/none.jpg",
            StartDate: today,
            EndDate: today.AddDays(1),
            Location: "Nowhere");

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _service.UpdateEventAsync(Guid.NewGuid(), updateReq));
    }

    [Fact]
    public async Task DeleteEventAsync_RemovesEventFromRepository()
    {
        // Arrange
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var evt = new BoardGameEvent(
            "Evento Temporal", "Para borrar", "/images/tmp.jpg",
            today.AddDays(5), today.AddDays(6), "Online");

        await _repo.AddAsync(evt);
        Assert.Single(_repo.Events);

        // Act
        await _service.DeleteEventAsync(evt.Id);

        // Assert
        Assert.Empty(_repo.Events);
    }
}
