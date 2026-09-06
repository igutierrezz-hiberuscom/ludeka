using System.Xml.Linq;
using Ludeka.Infrastructure.Bgg;
using Xunit;

namespace Ludeka.UnitTests.Infrastructure;

public class BggXmlSearchParserTests
{
    private const string SampleSearchXml = @"
<items total=""2"" termsofuse=""https://boardgamegeek.com/xmlapi/termsofuse"">
  <item type=""boardgame"" id=""13"">
    <name type=""primary"" value=""Catan""/>
    <name type=""alternate"" value=""Los Colonos de Catan""/>
    <yearpublished value=""1995""/>
  </item>
  <item type=""boardgame"" id=""322477"">
    <name type=""primary"" value=""Catan: 3D Edition &amp; Deluxe""/>
    <yearpublished value=""2021""/>
  </item>
</items>";

    [Fact]
    public void ParseSearchResults_WithValidXml_ExtractsPrimaryNamesAndIds()
    {
        // Arrange
        var doc = XDocument.Parse(SampleSearchXml);

        // Act
        var results = BggXmlParser.ParseSearchResults(doc);

        // Assert
        Assert.Equal(2, results.Count);

        var first = results[0];
        Assert.Equal(13, first.BggId);
        Assert.Equal("Catan", first.Title);
        Assert.Equal(1995, first.YearPublished);

        var second = results[1];
        Assert.Equal(322477, second.BggId);
        Assert.Equal("Catan: 3D Edition & Deluxe", second.Title); // WebUtility.HtmlDecode
        Assert.Equal(2021, second.YearPublished);
    }

    [Fact]
    public void ParseSearchResults_WithEmptyDocument_ReturnsEmptyList()
    {
        // Arrange
        var doc = new XDocument();

        // Act
        var results = BggXmlParser.ParseSearchResults(doc);

        // Assert
        Assert.Empty(results);
    }
}
