using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ludeka.Application.Contracts;
using Ludeka.Infrastructure.YouTube;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Ludeka.UnitTests.Infrastructure;

public class DynamicChannelFocusProviderTests
{
    private class FakeChannelDirectoryProvider : IChannelDirectoryProvider
    {
        public Task<IReadOnlyList<ChannelFocusEntry>> GetDynamicReferenceChannelsAsync(CancellationToken ct = default)
        {
            var list = new List<ChannelFocusEntry>
            {
                new("Editorial Rara", ChannelCategory.Publisher, Handle: "@editorialraratv", Description: "Canal dinámico", PriorityBonus: 55),
                new("Tienda Del Barrio", ChannelCategory.Store, Handle: "@tiendadelbarriotv", Description: "Canal de tienda", PriorityBonus: 60)
            };
            return Task.FromResult<IReadOnlyList<ChannelFocusEntry>>(list);
        }
    }

    [Fact]
    public void ChannelFocusProvider_WithScopeFactory_ShouldRecognizeDynamicChannels()
    {
        var services = new ServiceCollection();
        services.AddScoped<IChannelDirectoryProvider, FakeChannelDirectoryProvider>();
        var serviceProvider = services.BuildServiceProvider();
        var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

        var focusProvider = new ChannelFocusProvider(scopeFactory);

        // Canales dinámicos registrados en base de datos / directorio
        var isRefPublisher = focusProvider.IsReferenceChannel("@editorialraratv", out var categoryPub, out var bonusPub);
        Assert.True(isRefPublisher);
        Assert.Equal(ChannelCategory.Publisher, categoryPub);
        Assert.Equal(55, bonusPub);

        var isRefStore = focusProvider.IsReferenceChannel("Tienda Del Barrio", out var categoryStore, out var bonusStore);
        Assert.True(isRefStore);
        Assert.Equal(ChannelCategory.Store, categoryStore);
        Assert.Equal(60, bonusStore);

        // Canales estáticos tradicionales siguen funcionando
        var isRefStatic = focusProvider.IsReferenceChannel("Devir TV", out var categoryDevir, out var bonusDevir);
        Assert.True(isRefStatic);
        Assert.Equal(ChannelCategory.Publisher, categoryDevir);
        Assert.Equal(60, bonusDevir);

        // Canales desconocidos
        var isUnknown = focusProvider.IsReferenceChannel("Canal Desconocido XYZ", out _, out _);
        Assert.False(isUnknown);
    }
}
