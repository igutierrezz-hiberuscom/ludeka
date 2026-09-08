using System;
using System.Collections.Generic;
using System.Linq;
using Ludeka.Application.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Ludeka.Infrastructure.YouTube;

/// <summary>
/// Provee el padrón consolidado de canales oficiales hispanohablantes de Editoriales, Creadores y Tiendas.
/// Combina el padrón de referencia canónico con los canales dados de alta dinámicamente en el directorio (INC-14 e INC-19).
/// </summary>
public class ChannelFocusProvider : IChannelFocusProvider
{
    private readonly IServiceScopeFactory? _scopeFactory;
    private DateTimeOffset _lastDynamicFetch = DateTimeOffset.MinValue;
    private List<ChannelFocusEntry> _cachedDynamicChannels = [];
    private readonly object _lock = new();

    public ChannelFocusProvider(IServiceScopeFactory? scopeFactory = null)
    {
        _scopeFactory = scopeFactory;
    }

    private static readonly List<ChannelFocusEntry> Channels =
    [
        // --- 1. Editoriales (Publishers) ---
        new("Devir TV", ChannelCategory.Publisher, Handle: "@devirtv", Description: "Canal oficial de Devir Iberia (reglas, tutoriales y directos)", PriorityBonus: 60),
        new("Devir Iberia", ChannelCategory.Publisher, Handle: "@deviriberia", Description: "Canal oficial de Devir", PriorityBonus: 60),
        new("Tranjis Games", ChannelCategory.Publisher, Handle: "@TranjisGames", Description: "Editorial de Virus!, Dixit y juegos familiares", PriorityBonus: 50),
        new("Maldito Games", ChannelCategory.Publisher, Handle: "@MalditoGames", Description: "Editorial especializada en eurogames y títulos expertos", PriorityBonus: 55),
        new("Asmodee Ibérica", ChannelCategory.Publisher, Handle: "@AsmodeeIberica", Description: "Canal oficial de Asmodee en España", PriorityBonus: 55),
        new("2Tomatoes Games", ChannelCategory.Publisher, Handle: "@2tomatoesgames", Description: "Editorial de Root, Peak Oil y juegos temáticos", PriorityBonus: 50),
        new("TCG Factory", ChannelCategory.Publisher, Handle: "@tcgfactory", Description: "Editorial y distribuidora de juegos de mesa", PriorityBonus: 50),
        new("Arrakis Games", ChannelCategory.Publisher, Handle: "@ArrakisGames", Description: "Editorial de juegos de mesa y eurogames", PriorityBonus: 50),
        new("Mercurio Distribuciones", ChannelCategory.Publisher, Handle: "@MercurioDistribuciones", Description: "Editorial de juegos de mesa familiares y party", PriorityBonus: 45),
        new("SD Games", ChannelCategory.Publisher, Handle: "@SDGames", Description: "Editorial de Sagrada, Terraforming Mars Ares, etc.", PriorityBonus: 50),
        new("GDM Games", ChannelCategory.Publisher, Handle: "@GDMGames", Description: "Editorial nacional española de juegos de autor", PriorityBonus: 45),

        // --- 2. Creadores / Divulgadores (Creators) ---
        new("Análisis Parálisis", ChannelCategory.Creator, Handle: "@AnalisisParalisis", Description: "Referente absoluto en reseñas, partidas y directos lúdicos", PriorityBonus: 70),
        new("Meepletopía", ChannelCategory.Creator, Handle: "@meepletopia", Description: "Tutoriales detallados paso a paso y partidas completas", PriorityBonus: 65),
        new("El Agujero de Hobbit", ChannelCategory.Creator, Handle: "@elagujerodehobbit", Description: "Explicaciones de reglas rigurosas y ordenadas", PriorityBonus: 65),
        new("Mesa de Guerra", ChannelCategory.Creator, Handle: "@mesadeguerra", Description: "Especialistas en wargames, eurogames pesados y partidas tácticas", PriorityBonus: 60),
        new("Pareja de Ases", ChannelCategory.Creator, Handle: "@parejadeases", Description: "Partidas a 2 jugadores y reseñas de parejas", PriorityBonus: 65),
        new("Sentido Antihorario", ChannelCategory.Creator, Handle: "@SentidoAntihorario", Description: "Tutoriales y partidas en directo con enfoque de club", PriorityBonus: 60),
        new("Jugador Inicial", ChannelCategory.Creator, Handle: "@jugadorinicial", Description: "Divulgación cercana, tutoriales y opinión honesta", PriorityBonus: 60),
        new("La Mazmorra de Pacheco", ChannelCategory.Creator, Handle: "@LaMazmorradePacheco", Description: "Tutoriales animados y explicaciones dinámicas de cómo jugar", PriorityBonus: 65),
        new("Consola y Tablero", ChannelCategory.Creator, Handle: "@consolaytablero", Description: "Guías visuales, tutoriales rápidos y unboxings", PriorityBonus: 55),
        new("Océano de Juegos", ChannelCategory.Creator, Handle: "@oceanodejuegos", Description: "Reseñas sosegadas y partidas completas de juegos de mesa", PriorityBonus: 55),
        new("La Taberna de Dam", ChannelCategory.Creator, Handle: "@LaTabernadeDam", Description: "Divulgación lúdica, tops y partidas en mesa", PriorityBonus: 55),
        new("Rincón de Jugetes", ChannelCategory.Creator, Handle: "@rincondejugetes", Description: "Reseñas y cómo jugar en 2 minutos", PriorityBonus: 50),

        // --- 3. Tiendas Especializadas (Stores) ---
        new("Zacatrus!", ChannelCategory.Store, Handle: "@zacatrustv", Description: "Cadena de tiendas y editorial con canal didáctico referente", PriorityBonus: 65),
        new("Zacatrus TV", ChannelCategory.Store, Handle: "@zacatrustv", Description: "Tutoriales rápidos y partidas de Zacatrus", PriorityBonus: 65),
        new("Jugamos Otra", ChannelCategory.Store, Handle: "@jugamosotra", Description: "Tienda online y canal de partidas y tutoriales", PriorityBonus: 55),
        new("Cuarto de Juegos", ChannelCategory.Store, Handle: "@cuartodejuegos", Description: "Tienda mítica de Madrid con canal de recomendaciones", PriorityBonus: 50),
        new("Dungeon Marvels", ChannelCategory.Store, Handle: "@dungeonmarvels", Description: "Tienda especializada de Barcelona con novedades", PriorityBonus: 50),
        new("JugarXJugar", ChannelCategory.Store, Handle: "@jugarxjugar", Description: "Tienda y espacio de divulgación lúdica", PriorityBonus: 45)
    ];

    public IReadOnlyList<ChannelFocusEntry> GetReferenceChannels()
    {
        EnsureDynamicChannelsLoaded();

        if (_cachedDynamicChannels.Count == 0)
        {
            return Channels;
        }

        var merged = new List<ChannelFocusEntry>(Channels);
        foreach (var dyn in _cachedDynamicChannels)
        {
            if (!merged.Any(m => string.Equals(m.ChannelName, dyn.ChannelName, StringComparison.OrdinalIgnoreCase) ||
                                 (!string.IsNullOrWhiteSpace(m.Handle) && !string.IsNullOrWhiteSpace(dyn.Handle) &&
                                  string.Equals(m.Handle, dyn.Handle, StringComparison.OrdinalIgnoreCase))))
            {
                merged.Add(dyn);
            }
        }

        return merged;
    }

    public bool IsReferenceChannel(string channelTitle, out ChannelCategory category, out int priorityBonus)
    {
        category = ChannelCategory.Creator;
        priorityBonus = 0;

        if (string.IsNullOrWhiteSpace(channelTitle)) return false;

        var allChannels = GetReferenceChannels();
        var normalizedTitle = channelTitle.Trim();

        var match = allChannels.FirstOrDefault(c =>
            string.Equals(c.ChannelName, normalizedTitle, StringComparison.OrdinalIgnoreCase) ||
            (!string.IsNullOrWhiteSpace(c.Handle) && string.Equals(c.Handle, normalizedTitle, StringComparison.OrdinalIgnoreCase)) ||
            normalizedTitle.Contains(c.ChannelName, StringComparison.OrdinalIgnoreCase));

        if (match != null)
        {
            category = match.Category;
            priorityBonus = match.PriorityBonus;
            return true;
        }

        return false;
    }

    private void EnsureDynamicChannelsLoaded()
    {
        if (_scopeFactory == null) return;
        if (DateTimeOffset.UtcNow - _lastDynamicFetch < TimeSpan.FromSeconds(30)) return;

        lock (_lock)
        {
            if (DateTimeOffset.UtcNow - _lastDynamicFetch < TimeSpan.FromSeconds(30)) return;

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var provider = scope.ServiceProvider.GetService<IChannelDirectoryProvider>();
                if (provider != null)
                {
                    var task = provider.GetDynamicReferenceChannelsAsync();
                    _cachedDynamicChannels = task.GetAwaiter().GetResult().ToList();
                    _lastDynamicFetch = DateTimeOffset.UtcNow;
                }
            }
            catch
            {
                // Salvaguarda defensiva
            }
        }
    }
}
