using System.Linq;
using Ludeka.Core.ValueObjects;
using Xunit;

namespace Ludeka.UnitTests.Domain;

public class CountryCatalogTests
{
    [Theory]
    [InlineData("ES", "España", "🇪🇸")]
    [InlineData("España", "España", "🇪🇸")]
    [InlineData("espana", "España", "🇪🇸")]
    [InlineData("MX", "México", "🇲🇽")]
    [InlineData("mexico", "México", "🇲🇽")]
    [InlineData("AR", "Argentina", "🇦🇷")]
    [InlineData("argentina", "Argentina", "🇦🇷")]
    [InlineData("CL", "Chile", "🇨🇱")]
    [InlineData("CO", "Colombia", "🇨🇴")]
    [InlineData("PE", "Perú", "🇵🇪")]
    [InlineData("peru", "Perú", "🇵🇪")]
    [InlineData("UY", "Uruguay", "🇺🇾")]
    [InlineData("INT", "Internacional", "🌎")]
    [InlineData("Internacional", "Internacional", "🌎")]
    [InlineData("global", "Internacional", "🌎")]
    [InlineData("mundial", "Internacional", "🌎")]
    public void FindByNameOrCode_ResolvesKnownCountriesAndAliases(string query, string expectedName, string expectedFlag)
    {
        var result = CountryCatalog.FindByNameOrCode(query);

        Assert.NotNull(result);
        Assert.Equal(expectedName, result.Name);
        Assert.Equal(expectedFlag, result.FlagEmoji);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("PaisInexistenteXYZ")]
    public void FindByNameOrCode_ReturnsNullForInvalidOrUnknown(string? query)
    {
        var result = CountryCatalog.FindByNameOrCode(query);
        Assert.Null(result);
    }

    [Fact]
    public void GetFlag_ReturnsFlagOrGlobeFallback()
    {
        Assert.Equal("🇪🇸", CountryCatalog.GetFlag("España"));
        Assert.Equal("🇲🇽", CountryCatalog.GetFlag("México"));
        Assert.Equal("🌎", CountryCatalog.GetFlag("Internacional"));
        Assert.Equal("🌐", CountryCatalog.GetFlag("Desconocido"));
        Assert.Equal("🌐", CountryCatalog.GetFlag(null));
    }

    [Fact]
    public void Normalize_ReturnsCanonicalNameOrOriginalText()
    {
        Assert.Equal("España", CountryCatalog.Normalize("espana"));
        Assert.Equal("México", CountryCatalog.Normalize("mexico"));
        Assert.Equal("Internacional", CountryCatalog.Normalize("global"));
        Assert.Equal("Reino Unido", CountryCatalog.Normalize("Reino Unido"));
        Assert.Equal(string.Empty, CountryCatalog.Normalize(null));
    }

    [Theory]
    [InlineData("Internacional", true)]
    [InlineData("INT", true)]
    [InlineData("global", true)]
    [InlineData("mundial", true)]
    [InlineData("España", false)]
    [InlineData("México", false)]
    [InlineData(null, false)]
    public void IsInternational_IdentifiesGlobalScopeCorrectly(string? query, bool expected)
    {
        var result = CountryCatalog.IsInternational(query);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetAllCountries_IncludesSpecificCountriesAndInternational()
    {
        var all = CountryCatalog.GetAllCountries();
        var specific = CountryCatalog.GetSpecificCountries();

        Assert.NotEmpty(all);
        Assert.NotEmpty(specific);
        Assert.Equal(all.Count, specific.Count + 1);
        Assert.Contains(all, c => c.Code == "INT");
        Assert.DoesNotContain(specific, c => c.Code == "INT");
    }
}
