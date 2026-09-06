using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ludeka.Application.Contracts;
using Ludeka.Application.DTOs;
using Ludeka.Core.Entities;
using Ludeka.Core.Enums;

namespace Ludeka.Application.Features.Media;

public class MediaService : IMediaService
{
    private readonly IMediaRepository _mediaRepository;
    private readonly IGameRepository _gameRepository;
    private readonly IBrokenLinkCheckerService _brokenLinkChecker;

    public MediaService(
        IMediaRepository mediaRepository,
        IGameRepository gameRepository,
        IBrokenLinkCheckerService brokenLinkChecker)
    {
        _mediaRepository = mediaRepository ?? throw new ArgumentNullException(nameof(mediaRepository));
        _gameRepository = gameRepository ?? throw new ArgumentNullException(nameof(gameRepository));
        _brokenLinkChecker = brokenLinkChecker ?? throw new ArgumentNullException(nameof(brokenLinkChecker));
    }

    public async Task<GameMediaHubDto> GetGameMediaAsync(Guid gameId, CancellationToken ct = default)
    {
        var items = await _mediaRepository.GetApprovedByGameIdAsync(gameId, ct);
        var activeItems = items.Where(x => !x.IsBroken).ToList();

        var tutorials = activeItems
            .Where(x => x.Type == MediaType.Tutorial)
            .OrderByDescending(x => x.PublishedAt)
            .Select(x => MediaItemDto.FromDomain(x))
            .ToList();

        var playthroughs = activeItems
            .Where(x => x.Type == MediaType.Playthrough)
            .OrderByDescending(x => x.PublishedAt)
            .Select(x => MediaItemDto.FromDomain(x))
            .ToList();

        var instagramPosts = activeItems
            .Where(x => x.Type == MediaType.InstagramPost)
            .OrderByDescending(x => x.PublishedAt)
            .Select(x => MediaItemDto.FromDomain(x))
            .ToList();

        var shortReels = activeItems
            .Where(x => x.Type == MediaType.ShortReel)
            .OrderByDescending(x => x.PublishedAt)
            .Select(x => MediaItemDto.FromDomain(x))
            .ToList();

        return new GameMediaHubDto(gameId, tutorials, playthroughs, instagramPosts, shortReels);
    }

    public async Task<IReadOnlyList<MediaItemDto>> GetPendingModerationAsync(CancellationToken ct = default)
    {
        var items = await _mediaRepository.GetPendingModerationAsync(ct);
        return await MapWithGameTitlesAsync(items, ct);
    }

    public async Task<IReadOnlyList<MediaItemDto>> GetOrphansAsync(CancellationToken ct = default)
    {
        var items = await _mediaRepository.GetOrphansAsync(ct);
        return items.Select(x => MediaItemDto.FromDomain(x)).ToList();
    }

    public async Task<IReadOnlyList<MediaItemDto>> GetApprovedAsync(CancellationToken ct = default)
    {
        var items = await _mediaRepository.GetApprovedAsync(ct);
        return await MapWithGameTitlesAsync(items, ct);
    }

    public async Task<MediaItemDto?> ApproveMediaAsync(Guid id, CancellationToken ct = default)
    {
        var item = await _mediaRepository.GetByIdAsync(id, ct);
        if (item == null) return null;

        item.Approve();
        await _mediaRepository.UpdateAsync(item, ct);

        string? gameTitle = null;
        if (item.GameId.HasValue)
        {
            var game = await _gameRepository.GetByIdAsync(item.GameId.Value, ct);
            gameTitle = game?.SpanishTitle;
        }

        return MediaItemDto.FromDomain(item, gameTitle);
    }

    public async Task<MediaItemDto?> RejectMediaAsync(Guid id, CancellationToken ct = default)
    {
        var item = await _mediaRepository.GetByIdAsync(id, ct);
        if (item == null) return null;

        item.Reject();
        await _mediaRepository.UpdateAsync(item, ct);

        string? gameTitle = null;
        if (item.GameId.HasValue)
        {
            var game = await _gameRepository.GetByIdAsync(item.GameId.Value, ct);
            gameTitle = game?.SpanishTitle;
        }

        return MediaItemDto.FromDomain(item, gameTitle);
    }

    public async Task<MediaItemDto?> AssignOrphanMediaAsync(Guid id, Guid gameId, CancellationToken ct = default)
    {
        var item = await _mediaRepository.GetByIdAsync(id, ct);
        if (item == null) return null;

        var game = await _gameRepository.GetByIdAsync(gameId, ct);
        if (game == null)
            throw new ArgumentException("El juego especificado no existe en el catálogo.", nameof(gameId));

        item.AssignToGame(gameId);
        await _mediaRepository.UpdateAsync(item, ct);

        return MediaItemDto.FromDomain(item, game.SpanishTitle);
    }

    public async Task<BrokenLinkReportDto> CheckBrokenLinksAsync(CancellationToken ct = default)
    {
        return await _brokenLinkChecker.CheckLinksAsync(ct);
    }

    public async Task<MediaItemDto> CreateMediaItemAsync(MediaItem item, CancellationToken ct = default)
    {
        await _mediaRepository.AddAsync(item, ct);

        string? gameTitle = null;
        if (item.GameId.HasValue)
        {
            var game = await _gameRepository.GetByIdAsync(item.GameId.Value, ct);
            gameTitle = game?.SpanishTitle;
        }

        return MediaItemDto.FromDomain(item, gameTitle);
    }

    private async Task<IReadOnlyList<MediaItemDto>> MapWithGameTitlesAsync(IEnumerable<MediaItem> items, CancellationToken ct)
    {
        var list = items.ToList();
        var gameIds = list.Where(x => x.GameId.HasValue).Select(x => x.GameId!.Value).Distinct().ToList();

        var titleDict = new Dictionary<Guid, string>();
        foreach (var gid in gameIds)
        {
            var game = await _gameRepository.GetByIdAsync(gid, ct);
            if (game != null)
            {
                titleDict[gid] = game.SpanishTitle;
            }
        }

        return list.Select(x =>
        {
            string? title = x.GameId.HasValue && titleDict.TryGetValue(x.GameId.Value, out var t) ? t : x.Game?.SpanishTitle;
            return MediaItemDto.FromDomain(x, title);
        }).ToList();
    }
}
