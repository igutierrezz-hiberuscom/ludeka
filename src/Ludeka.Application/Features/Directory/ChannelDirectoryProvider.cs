using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ludeka.Application.Contracts;
using Ludeka.Core.Enums;

namespace Ludeka.Application.Features.Directory;

public class ChannelDirectoryProvider : IChannelDirectoryProvider
{
    private readonly IPublisherRepository _publisherRepository;
    private readonly ICreatorRepository _creatorRepository;
    private readonly IStoreRepository _storeRepository;

    public ChannelDirectoryProvider(
        IPublisherRepository publisherRepository,
        ICreatorRepository creatorRepository,
        IStoreRepository storeRepository)
    {
        _publisherRepository = publisherRepository ?? throw new ArgumentNullException(nameof(publisherRepository));
        _creatorRepository = creatorRepository ?? throw new ArgumentNullException(nameof(creatorRepository));
        _storeRepository = storeRepository ?? throw new ArgumentNullException(nameof(storeRepository));
    }

    public async Task<IReadOnlyList<ChannelFocusEntry>> GetDynamicReferenceChannelsAsync(CancellationToken ct = default)
    {
        var entries = new List<ChannelFocusEntry>();

        // 1. Editoriales
        var publishers = await _publisherRepository.GetAllAsync(ct);
        foreach (var pub in publishers)
        {
            var ytLink = pub.SocialLinks.FirstOrDefault(l => l.Platform == SocialPlatform.YouTube);
            if (ytLink != null)
            {
                var handle = ytLink.Handle ?? ExtractHandleFromUrl(ytLink.Url);
                entries.Add(new ChannelFocusEntry(
                    ChannelName: pub.Name,
                    Category: ChannelCategory.Publisher,
                    Handle: handle,
                    Description: $"Canal oficial de la editorial {pub.Name}",
                    PriorityBonus: 55
                ));
            }
        }

        // 2. Creadores
        var creators = await _creatorRepository.GetAllAsync(ct);
        foreach (var cr in creators)
        {
            var ytLink = cr.SocialLinks.FirstOrDefault(l => l.Platform == SocialPlatform.YouTube);
            if (ytLink != null)
            {
                var handle = ytLink.Handle ?? ExtractHandleFromUrl(ytLink.Url);
                entries.Add(new ChannelFocusEntry(
                    ChannelName: cr.Name,
                    Category: ChannelCategory.Creator,
                    Handle: handle,
                    Description: $"Canal de divulgación / autor: {cr.Name}",
                    PriorityBonus: 65
                ));
            }
        }

        // 3. Tiendas
        var stores = await _storeRepository.GetAllAsync(ct);
        foreach (var store in stores)
        {
            var ytLink = store.SocialLinks.FirstOrDefault(l => l.Platform == SocialPlatform.YouTube);
            if (ytLink != null)
            {
                var handle = ytLink.Handle ?? ExtractHandleFromUrl(ytLink.Url);
                entries.Add(new ChannelFocusEntry(
                    ChannelName: store.Name,
                    Category: ChannelCategory.Store,
                    Handle: handle,
                    Description: $"Canal de tienda especializada {store.Name}",
                    PriorityBonus: 60
                ));
            }
        }

        return entries;
    }

    private static string? ExtractHandleFromUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url)) return null;

        var atIndex = url.IndexOf('@');
        if (atIndex >= 0)
        {
            var handlePart = url[atIndex..].Split('/', '?', '&')[0];
            return handlePart;
        }

        return null;
    }
}
