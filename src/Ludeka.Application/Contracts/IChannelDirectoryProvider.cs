using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ludeka.Application.Contracts;

/// <summary>
/// Proveedor dinámico que recopila los canales de YouTube registrados en Editoriales, Creadores y Tiendas.
/// Alimenta el padrón algorítmico de IChannelFocusProvider (INC-14 e INC-19).
/// </summary>
public interface IChannelDirectoryProvider
{
    Task<IReadOnlyList<ChannelFocusEntry>> GetDynamicReferenceChannelsAsync(CancellationToken ct = default);
}
