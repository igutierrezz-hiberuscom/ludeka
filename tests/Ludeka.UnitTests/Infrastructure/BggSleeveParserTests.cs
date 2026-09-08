using System.Linq;
using System.Xml.Linq;
using Ludeka.Infrastructure.Bgg;
using Xunit;

namespace Ludeka.UnitTests.Infrastructure;

public class BggSleeveParserTests
{
    [Theory]
    [InlineData("Standard: 63.5 x 88 mm", 63.5, 88.0, 50, "Standard Card Game")]
    [InlineData("Chimera 57 x 89 mm (110 cards)", 57.0, 89.0, 110, "Chimera / USA")]
    [InlineData("Mini European: 44x68mm", 44.0, 68.0, 50, "Mini Euro")]
    [InlineData("Tarot (70 x 120 mm) 84 cartas", 70.0, 120.0, 84, "Tarot Grande")]
    public void ParseSingleSleeve_ExtractsDimensionsAndQuantities(string text, double expectedW, double expectedH, int expectedQty, string expectedFormat)
    {
        var sleeve = BggSleeveParser.ParseSingleSleeve(text);

        Assert.NotNull(sleeve);
        Assert.Equal(expectedW, sleeve.WidthMm);
        Assert.Equal(expectedH, sleeve.HeightMm);
        Assert.Equal(expectedQty, sleeve.CardCount);
        Assert.Equal(expectedFormat, sleeve.FormatName);
    }

    [Fact]
    public void ParseSingleSleeve_RespectsExplicitQuantity_WhenGiven()
    {
        var sleeve = BggSleeveParser.ParseSingleSleeve("56 x 87 mm", "73");

        Assert.NotNull(sleeve);
        Assert.Equal(56.0, sleeve.WidthMm);
        Assert.Equal(87.0, sleeve.HeightMm);
        Assert.Equal(73, sleeve.CardCount);
    }

    [Fact]
    public void ParseSleeves_ParsesMultipleSleeveLinks_FromXml()
    {
        string xml = @"
        <item type=""boardgame"" id=""174430"">
            <link type=""boardgamecardsleeve"" id=""101"" value=""Mini European: 44 x 68 mm"" qty=""73"" />
            <link type=""boardgamecardsleeve"" id=""102"" value=""Large: 65 x 100 mm"" qty=""12"" />
            <link type=""boardgamedesigner"" id=""201"" value=""Antoine Bauza"" />
        </item>";

        var element = XElement.Parse(xml);
        var sleeves = BggSleeveParser.ParseSleeves(element);

        Assert.Equal(2, sleeves.Count);

        var miniEuro = sleeves.FirstOrDefault(s => s.WidthMm == 44.0);
        Assert.NotNull(miniEuro);
        Assert.Equal(68.0, miniEuro.HeightMm);
        Assert.Equal(73, miniEuro.CardCount);
        Assert.Equal("Mini Euro", miniEuro.FormatName);

        var large = sleeves.FirstOrDefault(s => s.WidthMm == 65.0);
        Assert.NotNull(large);
        Assert.Equal(100.0, large.HeightMm);
        Assert.Equal(12, large.CardCount);
        Assert.Equal("Tarot / 7 Wonders", large.FormatName);
    }
}
