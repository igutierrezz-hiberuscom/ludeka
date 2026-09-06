using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ludeka.Application.Contracts;
using Ludeka.Application.DTOs;
using Ludeka.Core.Entities;

namespace Ludeka.Application.Features.Community;

public class WeeklyReleaseService : IWeeklyReleaseService
{
    private readonly IWeeklyReleaseRepository _repository;

    public WeeklyReleaseService(IWeeklyReleaseRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<IReadOnlyList<WeeklyReleaseDto>> GetReleasesAsync(DateOnly? fromDate = null, CancellationToken ct = default)
    {
        var releases = await _repository.GetReleasesAsync(fromDate, ct);
        return releases.Select(MapToDto).ToList();
    }

    public async Task<WeeklyReleaseDto> CreateReleaseAsync(CreateWeeklyReleaseRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var release = new WeeklyRelease(
            title: request.Title,
            publisher: request.Publisher,
            releaseDate: request.ReleaseDate,
            gameId: request.GameId,
            coverImageUrl: request.CoverImageUrl,
            estimatedPvp: request.EstimatedPvp,
            isReprint: request.IsReprint,
            notes: request.Notes);

        await _repository.AddAsync(release, ct);
        return MapToDto(release);
    }

    private static WeeklyReleaseDto MapToDto(WeeklyRelease r)
    {
        return new WeeklyReleaseDto(
            r.Id,
            r.Title,
            r.Publisher,
            r.ReleaseDate,
            r.GameId,
            r.CoverImageUrl,
            r.EstimatedPvp,
            r.IsReprint,
            r.Notes);
    }
}
