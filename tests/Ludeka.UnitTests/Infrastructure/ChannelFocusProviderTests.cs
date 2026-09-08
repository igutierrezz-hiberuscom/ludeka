using Ludeka.Application.Contracts;
using Ludeka.Infrastructure.YouTube;
using Xunit;

namespace Ludeka.UnitTests.Infrastructure;

public class ChannelFocusProviderTests
{
    private readonly ChannelFocusProvider _provider = new();

    [Fact]
    public void GetStaticCreators_ReturnsTwelveContentCreators()
    {
        var creators = ChannelFocusProvider.GetStaticCreators();

        Assert.Equal(12, creators.Count);
        Assert.All(creators, c => Assert.Equal(ChannelCategory.Creator, c.Category));
        Assert.Contains(creators, c => c.ChannelName == "Análisis Parálisis");
    }

    [Fact]
    public void GetReferenceChannels_ShouldContainPublishersCreatorsAndStores()
    {
        var channels = _provider.GetReferenceChannels();

        Assert.NotEmpty(channels);
        Assert.Contains(channels, c => c.Category == ChannelCategory.Publisher);
        Assert.Contains(channels, c => c.Category == ChannelCategory.Creator);
        Assert.Contains(channels, c => c.Category == ChannelCategory.Store);
    }

    [Theory]
    [InlineData("Devir TV", ChannelCategory.Publisher)]
    [InlineData("Devir Iberia", ChannelCategory.Publisher)]
    [InlineData("Maldito Games", ChannelCategory.Publisher)]
    [InlineData("Asmodee Ibérica", ChannelCategory.Publisher)]
    public void IsReferenceChannel_WithPublishers_ShouldReturnTrueAndCorrectCategory(string channelName, ChannelCategory expectedCategory)
    {
        var isRef = _provider.IsReferenceChannel(channelName, out var category, out var bonus);

        Assert.True(isRef);
        Assert.Equal(expectedCategory, category);
        Assert.True(bonus > 0);
    }

    [Theory]
    [InlineData("Análisis Parálisis", ChannelCategory.Creator)]
    [InlineData("Meepletopía", ChannelCategory.Creator)]
    [InlineData("El Agujero de Hobbit", ChannelCategory.Creator)]
    [InlineData("Mesa de Guerra", ChannelCategory.Creator)]
    public void IsReferenceChannel_WithCreators_ShouldReturnTrueAndCorrectCategory(string channelName, ChannelCategory expectedCategory)
    {
        var isRef = _provider.IsReferenceChannel(channelName, out var category, out var bonus);

        Assert.True(isRef);
        Assert.Equal(expectedCategory, category);
        Assert.True(bonus >= 60);
    }

    [Theory]
    [InlineData("Zacatrus!", ChannelCategory.Store)]
    [InlineData("Zacatrus TV", ChannelCategory.Store)]
    [InlineData("Jugamos Otra", ChannelCategory.Store)]
    public void IsReferenceChannel_WithStores_ShouldReturnTrueAndCorrectCategory(string channelName, ChannelCategory expectedCategory)
    {
        var isRef = _provider.IsReferenceChannel(channelName, out var category, out var bonus);

        Assert.True(isRef);
        Assert.Equal(expectedCategory, category);
        Assert.True(bonus >= 50);
    }

    [Theory]
    [InlineData("Canal Totalmente Desconocido 99")]
    [InlineData("Random Gamer 2026")]
    [InlineData("")]
    [InlineData(null)]
    public void IsReferenceChannel_WithNonReferenceOrEmpty_ShouldReturnFalse(string? channelName)
    {
        var isRef = _provider.IsReferenceChannel(channelName!, out _, out var bonus);

        Assert.False(isRef);
        Assert.Equal(0, bonus);
    }
}
