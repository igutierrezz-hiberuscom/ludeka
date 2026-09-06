using System.Xml.Linq;
using Ludeka.Infrastructure.Bgg;
using Xunit;

namespace Ludeka.UnitTests.Infrastructure;

public class BggXmlCollectionParserTests
{
    private const string SampleCollectionXml = @"
<items totalitems=""3"" termsofuse=""https://boardgamegeek.com/xmlapi/termsofuse"" pubdate=""Sun, 06 Sep 2026 12:00:00 +0000"">
  <item objecttype=""thing"" objectid=""13"" subtype=""boardgame"" collid=""1001"">
    <name sortindex=""1"">Catan &amp; Navegantes</name>
    <yearpublished>1995</yearpublished>
    <image>https://example.com/catan.jpg</image>
    <thumbnail>https://example.com/catan_t.jpg</thumbnail>
    <status own=""1"" prevowned=""0"" fortrade=""0"" want=""0"" wanttoplay=""0"" wanttobuy=""0"" wishlist=""0""/>
    <numplays>15</numplays>
  </item>
  <item objecttype=""thing"" objectid=""342942"" subtype=""boardgame"" collid=""1002"">
    <name sortindex=""1"">Ark Nova</name>
    <yearpublished>2021</yearpublished>
    <image>https://example.com/arknova.jpg</image>
    <thumbnail>https://example.com/arknova_t.jpg</thumbnail>
    <status own=""0"" prevowned=""0"" fortrade=""0"" want=""0"" wanttoplay=""0"" wanttobuy=""1"" wishlist=""1"" wishlistpriority=""1""/>
    <numplays>0</numplays>
  </item>
  <item objecttype=""thing"" objectid=""999999"" subtype=""boardgameaccessory"" collid=""1003"">
    <name sortindex=""1"">Insert para Catan</name>
    <status own=""1"" wishlist=""0""/>
    <numplays>0</numplays>
  </item>
</items>";

    [Fact]
    public void ParseCollection_WithValidXml_ExtractsOnlyBoardgamesAndCorrectStatuses()
    {
        // Arrange
        var doc = XDocument.Parse(SampleCollectionXml);

        // Act
        var items = BggXmlParser.ParseCollection(doc);

        // Assert
        Assert.Equal(2, items.Count); // Ignora el subtipo boardgameaccessory

        var catan = items[0];
        Assert.Equal(13, catan.BggId);
        Assert.Equal("Catan & Navegantes", catan.Title); // WebUtility.HtmlDecode
        Assert.Equal(1995, catan.YearPublished);
        Assert.Equal("https://example.com/catan_t.jpg", catan.ThumbnailUrl);
        Assert.Equal("https://example.com/catan.jpg", catan.CoverImageUrl);
        Assert.True(catan.IsOwned);
        Assert.False(catan.IsWishlist);
        Assert.False(catan.IsWantToBuy);
        Assert.Equal(15, catan.NumPlays);

        var arkNova = items[1];
        Assert.Equal(342942, arkNova.BggId);
        Assert.Equal("Ark Nova", arkNova.Title);
        Assert.Equal(2021, arkNova.YearPublished);
        Assert.False(arkNova.IsOwned);
        Assert.True(arkNova.IsWishlist);
        Assert.True(arkNova.IsWantToBuy);
        Assert.Equal(0, arkNova.NumPlays);
    }

    [Fact]
    public void ParseCollection_WithEmptyDocument_ReturnsEmptyList()
    {
        // Arrange
        var doc = new XDocument();

        // Act
        var items = BggXmlParser.ParseCollection(doc);

        // Assert
        Assert.Empty(items);
    }
}
