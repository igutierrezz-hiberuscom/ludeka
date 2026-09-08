using System.Collections.Generic;

namespace Ludeka.Application.Contracts;

public enum ChannelCategory
{
    Publisher,
    Creator,
    Store
}

public record ChannelFocusEntry(
    string ChannelName,
    ChannelCategory Category,
    string? ChannelId = null,
    string? Handle = null,
    string? Description = null,
    int PriorityBonus = 50
);

/// <summary>
/// Proveedor central del foco de canales hispanos oficiales de Editoriales, Creadores y Tiendas.
/// Alimenta la priorización algorítmica del motor de búsqueda de YouTube.
/// </summary>
public interface IChannelFocusProvider
{
    IReadOnlyList<ChannelFocusEntry> GetReferenceChannels();
    bool IsReferenceChannel(string channelTitle, out ChannelCategory category, out int priorityBonus);
}
